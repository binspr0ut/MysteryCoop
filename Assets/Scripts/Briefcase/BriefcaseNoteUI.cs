using UnityEngine;

public class BriefcaseNoteUI : MonoBehaviour
{
    [Header("Refs")]
    public GameObject notePanel;
    public GameObject noteButton;

    [Header("Inventory")]
    public DetectiveInventory detectiveInventory;
    public ItemData noteItemData;

    private bool hasTriggeredStateChange = false;

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
        // 🔥 Trigger state change only ONCE
        if (!hasTriggeredStateChange)
        {
            hasTriggeredStateChange = true;
            TriggerStateChangeToServer();
        }

        // noteButton.SetActive(false);
        HideNote();

        if (!detectiveInventory || !noteItemData)
        {
            Debug.LogWarning("[BriefcaseNoteUI] Inventory atau ItemData belum di-assign.");
            return;
        }

        bool added = detectiveInventory.AddItem(noteItemData);
        if (added)
        {
            if (noteButton) noteButton.SetActive(false);
            Debug.Log("[BriefcaseNoteUI] Note berhasil ditambahkan ke inventory!");
        }
        else
        {
            Debug.Log("Inventory penuh atau gagal menambahkan note!");
        }
    }

    private void TriggerStateChangeToServer()
    {
        if (Scene1StateManager.Instance != null)
        {
            Scene1StateManager.Instance.OnBriefcaseNoteTakenServerRpc();
            Debug.Log("[BriefcaseNoteUI] 🔥 STATE CHANGED: Move to FindSuitcaseCode");
        }
        else
        {
            Debug.LogWarning("[BriefcaseNoteUI] Scene1StateManager not found!");
        }
    }
}
