using Unity.Netcode;
using UnityEngine;

public class LampRope : NetworkBehaviour, IObject
{
    [Header("Scene References")]
    [SerializeField] private GameObject BasementCover;
    [SerializeField] private SpriteRenderer BasementRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;


    private NetworkVariable<bool> isOn = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    public bool CanInteract() => true;

    public void Interact(Transform player)
    {
        // Hanya owner atau client yang berinteraksi yang kirim permintaan ke server
        if (IsOwner || !IsServer)
        {
            ToggleLampRopeServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleLampRopeServerRpc(ServerRpcParams rpcParams = default)
    {
        isOn.Value = !isOn.Value; // ubah status di server
        UpdateDoorStateClientRpc(isOn.Value); // broadcast ke semua client
    }

    [ClientRpc]
    private void UpdateDoorStateClientRpc(bool newState)
    {
        // Toggle cover visibility
        BasementCover.SetActive(newState);
        if (BasementRenderer != null)
        {
            BasementRenderer.sprite = newState ? offSprite : onSprite;
        }
    }

    private void Start()
    {
        isOn.Value = false;
        BasementCover.SetActive(!isOn.Value);
        BasementRenderer.sprite = offSprite;
    }

}
