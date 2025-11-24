using UnityEngine;
using UnityEngine.EventSystems;

public class WireNode : MonoBehaviour, IPointerClickHandler
{
    public WireColor color;
    public bool isTopNode = false;        // kalau mau dipakai buat logika tambahan
    public ElectricPuzzle puzzle;

    // node pertama yang diklik
    private static WireNode firstSelected;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (puzzle == null)
        {
            Debug.LogWarning("[WireNode] puzzle is null");
            return;
        }

        // kalau belum ada node yang dipilih → simpan sebagai firstSelected
        if (firstSelected == null)
        {
            firstSelected = this;
            Debug.Log($"[WireNode] Selected {color} ({(isTopNode ? "Top" : "Bottom")})");
            return;
        }

        // kalau klik node yang sama → batal
        if (firstSelected == this)
        {
            Debug.Log("[WireNode] Same node clicked, clearing selection");
            firstSelected = null;
            return;
        }

        // cek warna
        if (firstSelected.color == this.color)
        {
            Debug.Log($"[WireNode] CONNECTED color {color}");

            // multiplayer: kirim ke server
            if (ElectricPuzzleNetwork.Instance != null &&
                ElectricPuzzleNetwork.Instance.IsSpawned)
            {
                ElectricPuzzleNetwork.Instance
                    .RequestSetCableConnectedServerRpc(color, true);
            }
            else
            {
                // singleplayer / fallback
                puzzle.SetCableConnected(color, true);
            }
        }
        else
        {
            Debug.Log($"[WireNode] WRONG pair: {firstSelected.color} vs {color}");
            // di sini kamu bisa tambahin efek salah kalau mau
        }

        // reset selection
        firstSelected = null;
    }
}