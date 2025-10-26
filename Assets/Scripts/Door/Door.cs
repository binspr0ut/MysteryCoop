using Unity.Netcode;
using UnityEngine;

public class Door : NetworkBehaviour, IObject
{
    [Header("Door References")]
    public GameObject DoorOpen;
    public GameObject DoorClosed;

    private NetworkVariable<bool> isOpen = new NetworkVariable<bool>(
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
            ToggleDoorServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleDoorServerRpc(ServerRpcParams rpcParams = default)
    {
        isOpen.Value = !isOpen.Value; // ubah status di server
        UpdateDoorStateClientRpc(isOpen.Value); // broadcast ke semua client
    }

    [ClientRpc]
    private void UpdateDoorStateClientRpc(bool newState)
    {
        DoorOpen.SetActive(newState);
        DoorClosed.SetActive(!newState);
    }

    private void OnEnable()
    {
        // Pastikan setiap kali nilai berubah, pintu update juga
        isOpen.OnValueChanged += (oldValue, newValue) =>
        {
            DoorOpen.SetActive(newValue);
            DoorClosed.SetActive(!newValue);
        };
    }

    void Start()
    {
        // Pastikan kondisi awal pintu sinkron dengan NetworkVariable
        DoorOpen.SetActive(isOpen.Value);
        DoorClosed.SetActive(!isOpen.Value);
    }
}
