// using System.Collections;
// using Unity.Netcode;
// using UnityEngine;

// public class PuzzlePieceNetworkManager : NetworkBehaviour
// {
//     public static PuzzlePieceNetworkManager Instance;

//     public GameObject panel;

//     public override void OnNetworkSpawn()
//     {
//         Instance = this;
//         panel.GetComponent<CanvasGroup>().alpha = 0;
//         panel.GetComponent<CanvasGroup>().interactable = false;
//         panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
//     }


//     [ServerRpc(RequireOwnership = false)]
//     public void PuzzleSolvedServerRpc()
//     {
//         Debug.Log("PUZZLE SOLVED!");
//         PuzzleSolvedClientRpc();
//     }

//     [ClientRpc]
//     void PuzzleSolvedClientRpc()
//     {
//         // contoh: buka map, scene, cutscene, dsb
//         Debug.Log("SHOW MAP!");
//     }
// }

using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PuzzlePieceNetworkManager : NetworkBehaviour
{
    public static PuzzlePieceNetworkManager Instance;

    [Header("UI")]
    public GameObject panel;

    [Header("SFX")]
    [SerializeField] private AudioClip openBoardSFX;   // SFX buka papan
    [SerializeField] private AudioClip closeBoardSFX;  // SFX tutup papan

    private CanvasGroup panelGroup;

    private void Awake()
    {
        Instance = this;

        if (panel != null)
            panelGroup = panel.GetComponent<CanvasGroup>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!panelGroup && panel != null)
            panelGroup = panel.GetComponent<CanvasGroup>();

        // awalnya papan disembunyikan (sama seperti code lamamu)
        SetPanelVisible(false, false);
    }

    // ========================
    // PANEL VISIBILITY + SFX
    // ========================
    private void SetPanelVisible(bool visible, bool playSfx)
    {
        if (!panelGroup) return;

        panelGroup.alpha = visible ? 1f : 0f;
        panelGroup.interactable = visible;
        panelGroup.blocksRaycasts = visible;

        if (!playSfx || AudioManager.Instance == null) return;

        AudioClip clip = visible ? openBoardSFX : closeBoardSFX;
        if (clip != null)
            AudioManager.Instance.PlaySFX(clip);
    }

    // Panggil ini kalau mau buka PaperUI
    public void OpenBoard()
    {
        SetPanelVisible(true, true);  // 🔊 mainkan SFX buka
    }

    // Panggil ini kalau mau tutup PaperUI
    public void CloseBoard()
    {
        SetPanelVisible(false, true); // 🔊 mainkan SFX tutup
    }

    // ========================
    // EXISTING: puzzle solved
    // ========================
    [ServerRpc(RequireOwnership = false)]
    public void PuzzleSolvedServerRpc()
    {
        Debug.Log("PUZZLE SOLVED!");
        PuzzleSolvedClientRpc();
    }

    [ClientRpc]
    void PuzzleSolvedClientRpc()
    {
        // contoh: buka map, scene, cutscene, dsb
        Debug.Log("SHOW MAP!");
    }
}