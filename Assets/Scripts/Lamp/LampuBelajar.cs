using Unity.Netcode;
using UnityEngine;

public class LampuBelajar : NetworkBehaviour, IPossess
{
    public bool IsPossessed { get; private set; }
    public string ID { get; private set; }

    [Header("Lamp Visuals")]
    [SerializeField] private GameObject spotLight;

    [Header("UI References")]
    [SerializeField] private GameObject uiControl;      // parent UIControl
    [SerializeField] private GameObject leftStick;      // joystick yang dimatikan saat possess
    [SerializeField] private GameObject interactButton; // tombol tetap aktif untuk unpossess
    [SerializeField] private GameObject possesIcon;     // tidak dimatikan

    private SpiritMovement possessedSpirit;

    // Disinkronkan antar client: lampu ON/OFF
    private NetworkVariable<bool> isLampOn = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public bool CanPossess() => !IsPossessed;

    void Start()
    {
        // Matikan lampu saat mulai
        ApplyLampState(false);
    }

    public override void OnNetworkSpawn()
    {
        // Terapkan state awal (sinkron bagi late joiner)
        ApplyLampState(isLampOn.Value);

        // Dengarkan perubahan state dari server
        isLampOn.OnValueChanged += (_, newVal) => ApplyLampState(newVal);
    }

    private void ApplyLampState(bool on)
    {
        if (spotLight != null && spotLight.activeSelf != on)
            spotLight.SetActive(on);
    }

    // ==== POSSESS LOGIC ====
    public void Possess()
    {
        if (IsPossessed)
        {
            IsPossessed = false;
            Unpossess();
        }
        else
        {
            // cari spirit milik local player
            var spirit = FindFirstObjectByType<SpiritMovement>();
            if (spirit != null && spirit.IsOwner)
            {
                spirit.SetVisibleServerRpc(false); // 🔹 sembunyikan spirit di semua client
                possessedSpirit = spirit;
            }

            // 🔹 Nonaktifkan LeftStick (tidak bisa gerak saat possess)
            if (leftStick == null)
            {
                var ui = GameObject.Find("UIControl");
                if (ui != null)
                    leftStick = ui.transform.Find("Left Stick")?.gameObject;
            }
            if (leftStick != null)
                leftStick.SetActive(false);

            // 🔹 InteractButton tetap aktif untuk Unpossess
            if (interactButton == null)
            {
                var ui = GameObject.Find("UIControl");
                if (ui != null)
                    interactButton = ui.transform.Find("InteractButton")?.gameObject;
            }
            if (interactButton != null)
                interactButton.SetActive(true);

            // 🔹 Tidak matikan possesIcon
            Debug.Log("[LampuBelajar] Possessing object...");
            IsPossessed = true;

            // 🔹 Sinkronkan ON ke server
            ToggleLampServerRpc(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleLampServerRpc(bool on)
    {
        isLampOn.Value = on;
    }

    public void Interact() { } // not used

    public void Unpossess()
    {
        if (!IsPossessed) return;

        if (possessedSpirit != null)
        {
            possessedSpirit.SetVisibleServerRpc(true);
            possessedSpirit = null;
        }

        // 🔹 Aktifkan kembali LeftStick
        if (leftStick != null)
            leftStick.SetActive(true);

        // 🔹 Lampu OFF di server
        ToggleLampServerRpc(false);

        IsPossessed = false;
        Debug.Log("[LampuBelajar] Unpossessed & Lamp turned OFF");
    }
}
