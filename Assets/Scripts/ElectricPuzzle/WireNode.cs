using UnityEngine;
using UnityEngine.EventSystems; // biar bisa klik UI

public class WireNode : MonoBehaviour, IPointerClickHandler
{
    public WireColor color;
    public bool isTopNode = false;   // atas atau bawah, buatmu saja
    public ElectricPuzzle puzzle;

    // static untuk menyimpan node yang sedang dipilih
    private static WireNode selectedNode;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (puzzle == null)
        {
            Debug.LogWarning("[WireNode] puzzle is null");
            return;
        }

        if (selectedNode == null)
        {
            // pilih node pertama
            selectedNode = this;
            Debug.Log($"[WireNode] Selected {color} ({(isTopNode ? "Top" : "Bottom")})");
        }
        else
        {
            // klik kedua: cek apakah warna sama
            if (selectedNode == this)
            {
                // klik node yang sama → batal
                selectedNode = null;
                return;
            }

            if (selectedNode.color == this.color)
            {
                Debug.Log($"[WireNode] CONNECTED color {color}");

                // Kalau ada network → kirim ke server
                if (ElectricPuzzleNetwork.Instance != null && ElectricPuzzleNetwork.Instance.IsSpawned)
                {
                    ElectricPuzzleNetwork.Instance
                        .RequestSetCableConnectedServerRpc(color, true);
                }
                else
                {
                    // fallback: singleplayer / belum spawn network
                    if (puzzle != null)
                        puzzle.SetCableConnected(color, true);
                }
            }
            else
            {
                Debug.Log($"[WireNode] Wrong pair: {selectedNode.color} vs {color}");
                // bisa tambahin efek salah
            }

            // reset pilihan
            selectedNode = null;
        }
    }
}