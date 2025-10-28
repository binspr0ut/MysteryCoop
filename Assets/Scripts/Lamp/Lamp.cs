using Unity.Netcode;
using UnityEngine;

public class Lamp : NetworkBehaviour, IPossess
{
    public bool IsPossessed { get; private set; }
    public string ID { get; private set; }

    [Header("Lamp Visuals")]
    public SpriteRenderer lightRenderer;
    private Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.white };
    private int currentColorIndex = 0;

    // simpan spirit yang sedang possess
    private NetworkObject currentSpirit;

    public bool CanPossess() => !IsPossessed;

    // Dijalankan ketika hantu menekan tombol possess
    public void Possess()
    {
        if (IsPossessed) return;

        // client -> server
        if (!IsServer)
        {
            RequestPossessServerRpc(NetworkManager.Singleton.LocalClientId);
            return;
        }

        DoPossess(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPossessServerRpc(ulong clientId)
    {
        DoPossess(clientId);
    }

    private void DoPossess(ulong clientId)
    {
        if (IsPossessed) return;
        IsPossessed = true;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            currentSpirit = client.PlayerObject;
            Debug.Log($"Spirit {clientId} possessed lamp");
            UpdateSpiritVisibilityClientRpc(clientId, false); // sembunyikan di semua client
        }
    }

    public void Interact()
    {
        if (!IsPossessed)
        {
            Debug.LogWarning("Tried to interact, but lamp is not possessed!");
            return;
        }

        currentColorIndex = (currentColorIndex + 1) % colors.Length;
        lightRenderer.color = colors[currentColorIndex];
        Debug.Log($"Lamp color changed to {lightRenderer.color}");
    }


    public void Unpossess()
    {
        if (!IsPossessed) return;

        if (!IsServer)
        {
            RequestUnpossessServerRpc(NetworkManager.Singleton.LocalClientId);
            return;
        }

        DoUnpossess();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestUnpossessServerRpc(ulong clientId)
    {
        DoUnpossess();
    }

    private void DoUnpossess()
    {
        if (!IsPossessed) return;
        IsPossessed = false;

        if (currentSpirit != null)
        {
            Transform spiritTransform = currentSpirit.transform;
            spiritTransform.position = transform.position + Vector3.up * 1f;
            lightRenderer.color = Color.white;
            UpdateSpiritVisibilityClientRpc(currentSpirit.OwnerClientId, true); // tampilkan kembali
            Debug.Log("Lamp unpossessed!");
        }

        currentSpirit = null;
    }

    // === 🔹 Sinkronkan tampilan spirit antar client ===
    [ClientRpc]
    private void UpdateSpiritVisibilityClientRpc(ulong spiritOwnerId, bool visible)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(spiritOwnerId, out var client))
            return;

        var spirit = client.PlayerObject;
        if (spirit == null) return;

        var sprite = spirit.GetComponent<SpriteRenderer>();
        if (sprite) sprite.enabled = visible;

        var collider = spirit.GetComponent<Collider2D>();
        if (collider) collider.enabled = visible;

        Debug.Log($"[Client] Spirit {spiritOwnerId} visibility set to {visible}");
    }

    private void Start()
    {
        ID ??= System.Guid.NewGuid().ToString();
        if (lightRenderer)
            lightRenderer.color = Color.white;
    }
}
