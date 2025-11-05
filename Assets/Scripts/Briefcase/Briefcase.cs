// using UnityEngine;

// public class Briefcase : MonoBehaviour, IObject
// {
//     public bool IsInteracted { get; private set; }
//     public string ID { get; private set; }

//     [Header("UI")]
//     public GameObject LockPanel;   // panel input kode
//     public GameObject ControlUI;   // HUD kontrol (disembunyikan saat panel tampil)

//     [Header("Settings")]
//     public string CorrectCode = "4931";

//     public bool CanInteract() => true;

//     public void Interact(Transform playerTransform)
//     {
//         if (ControlUI != null) ControlUI.SetActive(false);

//         if (LockPanel == null)
//         {
//             Debug.LogError("[Briefcase] LockPanel belum di-assign.");
//             return;
//         }

//         var panel = LockPanel.GetComponent<BriefcaseLockPanel>();
//         if (panel == null)
//         {
//             Debug.LogError("[Briefcase] Komponen BriefcaseLockPanel tidak ditemukan di LockPanel.");
//             return;
//         }

//         LockPanel.SetActive(true);
//         panel.Init(this, CorrectCode);
//         IsInteracted = true;
//     }

//     public void OnUnlocked()
//     {
//         // Di tahap ini cukup tutup panel; nanti kita bisa tambah buka-isi koper.
//         ClosePuzzle();
//     }

//     public void ClosePuzzle()
//     {
//         if (LockPanel != null) LockPanel.SetActive(false);
//         if (ControlUI != null) ControlUI.SetActive(true);
//         IsInteracted = false;
//     }

//     private void Start()
//     {
//         ID ??= GlobalHelper.GenerateUniqueID(gameObject);
//         if (LockPanel != null) LockPanel.SetActive(false);
//     }

//     private void Update() { }
// }




// using UnityEngine;

// public class Briefcase : MonoBehaviour, IObject
// {
//     public bool IsInteracted { get; private set; }
//     public string ID { get; private set; }

//     [Header("UI")]
//     public GameObject LockPanel;   // panel input kode
//     public GameObject ControlUI;   // HUD kontrol (disembunyikan saat panel tampil)

//     [Header("Settings")]
//     public string CorrectCode = "4931";

//     [Header("Role Restriction")]
//     [Tooltip("Hanya detektif yang boleh interaksi koper")]
//     public bool onlyDetectiveCanInteract = true;
//     public string detectiveTag = "Detective";

//     // mencegah double-trigger quest 1 jika dipencet berkali-kali
//     private bool explorationMarkedOnServer = false;

//     public bool CanInteract() => true;

//     public void Interact(Transform playerTransform)
//     {
//         // --- Batasi hanya Detective ---
//         if (onlyDetectiveCanInteract && (playerTransform == null || !playerTransform.CompareTag(detectiveTag)))
//         {
//             // Optional: tampilkan hint ke player (Ghost)
//             Debug.Log("[Briefcase] Hanya detektif yang bisa menginteraksi koper ini.");
//             return;
//         }

//         if (ControlUI != null) ControlUI.SetActive(false);

//         if (LockPanel == null)
//         {
//             Debug.LogError("[Briefcase] LockPanel belum di-assign.");
//             return;
//         }

//         var panel = LockPanel.GetComponent<BriefcaseLockPanel>();
//         if (panel == null)
//         {
//             Debug.LogError("[Briefcase] Komponen BriefcaseLockPanel tidak ditemukan di LockPanel.");
//             return;
//         }

//         LockPanel.SetActive(true);
//         panel.Init(this, CorrectCode);
//         IsInteracted = true;

//         // --- Hook Quest 1 → Quest 2 (Explore -> Find Passcode) ---
//         // Hanya panggil sekali dan hanya jika state saat ini memang Explore
//         if (!explorationMarkedOnServer && QuestManager_Network.I != null)
//         {
//             var state = QuestManager_Network.I.currentState.Value;
//             if (state == QuestState.ExploreBoardingHouse)
//             {
//                 // Minta server ganti state; RequireOwnership=false di manager, jadi aman dipanggil client mana pun
//                 QuestManager_Network.I.MarkExploreDone_ServerRpc();
//                 explorationMarkedOnServer = true;
//             }
//         }
//     }

//     public void OnUnlocked()
//     {
//         // --- Hook Quest 2 selesai (Find Passcode -> Completed) ---
//         if (QuestManager_Network.I != null &&
//             QuestManager_Network.I.currentState.Value == QuestState.FindBriefcasePasscode)
//         {
//             QuestManager_Network.I.MarkAllDone_ServerRpc();
//         }

//         // Di tahap ini cukup tutup panel; nanti kita bisa tambah buka isi koper
//         ClosePuzzle();
//     }

//     public void ClosePuzzle()
//     {
//         if (LockPanel != null) LockPanel.SetActive(false);
//         if (ControlUI != null) ControlUI.SetActive(true);
//         IsInteracted = false;
//     }

//     private void Start()
//     {
//         ID ??= GlobalHelper.GenerateUniqueID(gameObject);
//         if (LockPanel != null) LockPanel.SetActive(false);
//     }

//     private void Update() { }
// }



using UnityEngine;
using Unity.Netcode;

public class Briefcase : NetworkBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI")]
    public GameObject LockPanel;   // panel input kode
    public GameObject ControlUI;   // HUD kontrol (disembunyikan saat panel tampil)

    [Header("Settings")]
    public string CorrectCode = "4931";


    // mencegah double-trigger quest 1 jika dipencet berkali-kali
    private bool explorationMarkedOnServer = false;

    public bool CanInteract() => true;

    public void Interact(Transform playerTransform)
    {
        // --- Perilaku asli: sembunyikan HUD & tampilkan LockPanel ---
        if (ControlUI != null) ControlUI.SetActive(false);

        if (LockPanel == null)
        {
            Debug.LogError("[Briefcase] LockPanel belum di-assign.");
            return;
        }

        var panel = LockPanel.GetComponent<BriefcaseLockPanel>();
        if (panel == null)
        {
            Debug.LogError("[Briefcase] Komponen BriefcaseLockPanel tidak ditemukan di LockPanel.");
            return;
        }

        LockPanel.SetActive(true);
        panel.Init(this, CorrectCode);
        IsInteracted = true;
    }

    public void ClosePuzzle()
    {
        if (LockPanel != null) LockPanel.SetActive(false);
        if (ControlUI != null) ControlUI.SetActive(true);
        IsInteracted = false;
    }

    private void Start()
    {
        ID ??= GlobalHelper.GenerateUniqueID(gameObject);
        if (LockPanel != null) LockPanel.SetActive(false);
    }

    private void Update() { }
}