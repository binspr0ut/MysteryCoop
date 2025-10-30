// using UnityEngine;

// public class BriefcaseNoteUI : MonoBehaviour
// {
//     [Header("Refs")]
//     public GameObject notePanel;   //NotePanel (berisi Dim + Note)
//     public GameObject noteButton;  // tombol kecil di atas koper (NoteBtn)

//     [Header("Inventory")]
//     public DetectiveInventory detectiveInventory; // referensi ke Detective
//     public ItemData noteItemData; // item note ScriptableObject

//     void Start()
//     {
//         if (notePanel) notePanel.SetActive(false);
//     }

//     // Dipanggil saat tombol kertas di atas koper ditekan
//     public void ShowNote()
//     {
//         if (notePanel) notePanel.SetActive(true);
//     }

//     // Dipanggil saat tombol Close atau background ditekan
//     public void HideNote()
//     {
//         if (notePanel) notePanel.SetActive(false);
//     }

//     // Dipanggil saat tombol "Take" di panel ditekan
//     public void TakeNote()
//     {
//         if (!detectiveInventory || !noteItemData)
//         {
//             Debug.LogWarning("[BriefcaseNoteUI] Inventory atau ItemData belum di-assign.");
//             return;
//         }

//         bool added = detectiveInventory.TryAdd(noteItemData);
//         if (added)
//         {
//             if (noteButton) noteButton.SetActive(false); // sembunyikan kertas di atas koper
//             HideNote(); // tutup panel
//             Debug.Log("Note berhasil ditambahkan ke inventory!");
//         }
//         else
//         {
//             Debug.Log("Inventory penuh atau gagal menambahkan note!");
//         }
//     }
// }

using UnityEngine;

public class BriefcaseNoteUI : MonoBehaviour
{
    [Header("Refs")]
    public GameObject notePanel;
    public GameObject noteButton;

    [Header("Inventory")]
    public DetectiveInventory detectiveInventory;
    public ItemData noteItemData;

    void Start()
    {
        if (notePanel) notePanel.SetActive(false);
    }

    public void ShowNote()
    {
        if (notePanel) notePanel.SetActive(true);
    }

    public void HideNote()
    {
        if (notePanel) notePanel.SetActive(false);
    }

    public void TakeNote()
    {
        if (!detectiveInventory || !noteItemData)
        {
            Debug.LogWarning("[BriefcaseNoteUI] Inventory atau ItemData belum di-assign.");
            return;
        }

        bool added = detectiveInventory.AddItem(noteItemData);
        if (added)
        {
            if (noteButton) noteButton.SetActive(false); // sembunyikan kertas di atas koper
            HideNote(); // tutup panel
            Debug.Log("Note berhasil ditambahkan ke inventory!");
        }
        else
        {
            Debug.Log("Inventory penuh atau gagal menambahkan note!");
        }
    }
}