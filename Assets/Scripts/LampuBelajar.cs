using Unity.Netcode;
using UnityEngine;

public class LampuBelajar : NetworkBehaviour, IPossess
{
    public bool IsPossessed { get; private set; }
    public string ID { get; private set; }

    [Header("Lamp Visuals")]
    public GameObject spotLight;

    private NetworkObject currentSpirit;

    //status nyala lampu (disinkron ke semua client)
    private NetworkVariable<bool> spotlightOn =
        new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool CanPossess() => !IsPossessed;

    // Possess / Unpossess Logic

    // di dalam class LampuBelajar
    public void Interact()
    {
        
    }

    public void Possess()
    {
        if (IsPossessed) return;

        if (!IsServer)
        {
            RequestPossessServerRpc(NetworkManager.Singleton.LocalClientId);
            return;
        }

        DoPossess(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPossessServerRpc(ulong clientId) => DoPossess(clientId);

    private void DoPossess(ulong clientId)
    {
        if (IsPossessed) return;
        IsPossessed = true;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            currentSpirit = client.PlayerObject;
            UpdateSpiritVisibilityClientRpc(clientId, false);
        }

        // nyalakan lampu di server
        spotlightOn.Value = true;
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
    private void RequestUnpossessServerRpc(ulong clientId) => DoUnpossess();

    private void DoUnpossess()
    {
        if (!IsPossessed) return;
        IsPossessed = false;

        if (currentSpirit != null)
        {
            Transform spiritTransform = currentSpirit.transform;
            spiritTransform.position = transform.position + Vector3.up * 1f;

            UpdateSpiritVisibilityClientRpc(currentSpirit.OwnerClientId, true);
        }

        //matikan lampu di server
        spotlightOn.Value = false;

        currentSpirit = null;
    }
    // Sync Visuals

    private void ApplySpotlight(bool on)
    {
        if (spotLight && spotLight.activeSelf != on)
            spotLight.SetActive(on);
    }

    [ClientRpc]
    private void UpdateSpiritVisibilityClientRpc(ulong spiritOwnerId, bool visible)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(spiritOwnerId, out var client))
            return;

        var spirit = client.PlayerObject;
        if (!spirit) return;

        var sprite = spirit.GetComponent<SpriteRenderer>();
        if (sprite) sprite.enabled = visible;

        var collider = spirit.GetComponent<Collider2D>();
        if (collider) collider.enabled = visible;
    }

    //Lifecycle Methods
    public override void OnNetworkSpawn()
    {
        // apply status awal
        ApplySpotlight(spotlightOn.Value);

        // auto-update semua client saat nilai spotlight berubah
        spotlightOn.OnValueChanged += (_, newVal) => ApplySpotlight(newVal);
    }

    private void OnDestroy()
    {
        spotlightOn.OnValueChanged -= (_, __) => { };
    }

    private void Start()
    {
        ID ??= System.Guid.NewGuid().ToString();

        // pastikan lampu mati di awal
        ApplySpotlight(false);
    }
}