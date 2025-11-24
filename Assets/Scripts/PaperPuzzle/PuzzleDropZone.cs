using UnityEngine;
using Unity.Netcode;

public class PuzzleDropZone : NetworkBehaviour
{
    public int slotIndex;
    public PuzzlePieceDragHandler currentPiece;

    public RectTransform rect => GetComponent<RectTransform>();


    // Dipanggil lokal -> kirim ke server
    public void PlacePiece(PuzzlePieceDragHandler piece)
    {
        ulong pieceNetId = piece.GetComponent<NetworkObject>().NetworkObjectId;
        UpdateDropZoneServerRpc(slotIndex, pieceNetId);
    }


    // SERVER: terima perubahan dan broadcast ke semua client
    [ServerRpc(RequireOwnership = false)]
    void UpdateDropZoneServerRpc(int zoneIndex, ulong pieceId)
    {
        UpdateDropZoneClientRpc(zoneIndex, pieceId);
    }


    // Semua client sync posisi
    [ClientRpc]
    void UpdateDropZoneClientRpc(int zoneIndex, ulong pieceId)
    {
        var zone = PuzzleZoneManager.Instance.zones[zoneIndex];

        var pieceObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[pieceId];
        var piece = pieceObj.GetComponent<PuzzlePieceDragHandler>();

        zone.currentPiece = piece;
        piece.rect.anchoredPosition = zone.rect.anchoredPosition;

        PuzzleZoneManager.Instance.CheckSolved();
    }
}
