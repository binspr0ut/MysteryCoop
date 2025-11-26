using Unity.Netcode;
using UnityEngine;

public class BriefcaseNoteUI : NetworkBehaviour
{
    [Header("Refs")]
    public GameObject notePanel;
    public GameObject noteButton;

    [Header("SFX")]
    [SerializeField] private AudioClip openNoteSFX;

    [Header("Inventory")]
    public DetectiveInventory detectiveInventory;
    public ItemData noteItemData;

    private bool hasTriggeredStateChange = false;
    private bool hasTaken = false;

    void Start()
    {
        if (notePanel) notePanel.SetActive(false);
    }

    public void ShowNote()
    {
        InventoryController.Instance.HideInventory();

        // 🔊 SFX buka note
        if (AudioManager.Instance != null && openNoteSFX != null)
        {
            AudioManager.Instance.PlaySFX(openNoteSFX);
        }

        // 🔥 Trigger state change only ONCE
        if (!hasTriggeredStateChange)
        {
            hasTriggeredStateChange = true;
            TriggerStateChangeToServer();


            SubtitleManager.Instance.ShowSubtitle(
                "Dinda: Could that be the answer to unlocking the briefcase?",
                SubtitleTarget.Spirit,
                SubtitleScope.Global
            );

            SubtitleManager.Instance.ShowSubtitle(
                "Agung: Maybe, we should try",
                SubtitleTarget.Detective,
                SubtitleScope.Global
            );

        }

        if (notePanel) notePanel.SetActive(true);
    }

    public void HideNote()
    {

        if (notePanel) notePanel.SetActive(false);
    }

    public void TakeNote()
    {
        if (!hasTaken)
        {
            GetNoteServerRpc();
            hasTaken = true;
        }

        HideNote();
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

    [ServerRpc]
    void GetNoteServerRpc()
    {
        GetNoteClientRpc();
    }

    [ClientRpc]
    void GetNoteClientRpc()
    {
        InventoryController.Instance.GetNote();
    }
}
