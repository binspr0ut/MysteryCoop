using UnityEngine;
using Unity.Netcode;

public class PuzzleDropZone : NetworkBehaviour
{

    [Header("SFX")]
    [SerializeField] private AudioClip dropPaperSFX;

    public int slotIndex;
    public PuzzlePieceDragHandler currentPiece;

    public RectTransform rect => GetComponent<RectTransform>();


    // Dipanggil lokal -> kirim ke server
    public void PlacePiece(PuzzlePieceDragHandler piece)
    {
        ulong pieceNetId = piece.GetComponent<NetworkObject>().NetworkObjectId;
        UpdateDropZoneServerRpc(slotIndex, pieceNetId);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
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

        // 🔊 SFX: paper berhasil di-drop ke slot ini
        PlaySFX(dropPaperSFX);

        PuzzleZoneManager.Instance.CheckSolved();
    }
}
