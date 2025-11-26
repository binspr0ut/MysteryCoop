using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Door : NetworkBehaviour, IObject
{
    [Header("Door References")]
    public GameObject DoorOpen;
    public GameObject DoorClosed;

    [Header("SFX")]
    [SerializeField] private AudioClip doorOpenSFX;
    [SerializeField] private AudioClip doorCloseSFX;

    // Hanya server yang boleh write
    private readonly NetworkVariable<bool> isOpen = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // ==== Debounce client-side supaya satu tekan = satu RPC ====
    private static float s_lastInteractTime;
    private const float InteractCooldown = 0.2f;

    // ==== Anti-spam server-side per-client (opsional tapi aman) ====
    private static readonly Dictionary<ulong, float> s_serverLastToggle = new();

    public bool CanInteract() => true;

    public void Interact(Transform player)
    {
        // Debounce di sisi yang menekan tombol
        if (Time.unscaledTime - s_lastInteractTime < InteractCooldown) return;
        s_lastInteractTime = Time.unscaledTime;

        if (IsServer && IsOwner) // host menekan (server+client): toggle langsung
        {
            ToggleDoor_ServerAuthoritative();
        }
        else if (!IsServer) // pure client: kirim RPC sekali
        {
            ToggleDoorServerRpc();
        }
        // Catatan: Dedicated server tidak punya input; cabang di atas sudah cukup.
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleDoorServerRpc(ServerRpcParams rpcParams = default)
    {
        var sender = rpcParams.Receive.SenderClientId;

        // Anti-spam di server
        if (s_serverLastToggle.TryGetValue(sender, out var last) &&
            Time.unscaledTime - last < InteractCooldown)
            return;

        s_serverLastToggle[sender] = Time.unscaledTime;

        ToggleDoor_ServerAuthoritative();
    }

    // Selalu panggil ini HANYA di server
    private void ToggleDoor_ServerAuthoritative()
    {
        isOpen.Value = !isOpen.Value;   // memicu OnValueChanged di semua client
        UpdateDoorVisual(isOpen.Value); // juga update lokal (server)
        // Debug.Log($"[SERVER] Door toggled -> {isOpen.Value}");
    }

    private void UpdateDoorVisual(bool open)
    {
        if (DoorOpen) DoorOpen.SetActive(open);
        if (DoorClosed) DoorClosed.SetActive(!open);
    }

    // === Sinkron awal dan event binding pakai lifecycle Netcode ===
    public override void OnNetworkSpawn()
    {
        // Saat object spawn di client, NetworkVariable sudah tersinkron → pakai nilainya
        UpdateDoorVisual(isOpen.Value);

        isOpen.OnValueChanged += OnDoorNetworkChanged;
    }

    public override void OnNetworkDespawn()
    {
        isOpen.OnValueChanged -= OnDoorNetworkChanged;
    }

    private void OnDoorNetworkChanged(bool oldValue, bool newValue)
    {
        UpdateDoorVisual(newValue);

        // 🔊 SFX: play saat pintu benar-benar berubah state
        if (AudioManager.Instance != null)
        {
            AudioClip clip = newValue ? doorOpenSFX : doorCloseSFX;
            if (clip != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
        }

        // Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Door visual <- {newValue}");
    }
}
