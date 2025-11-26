using UnityEngine;
using Unity.Netcode;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelMain;
    public GameObject panelRoomSelect;
    public GameObject panelCreateRoom;
    public GameObject panelJoinRoom;


    [Header("Guide Panels")]
    public GameObject panelGuideDetective;   // drag Panel_GuideDetective
    public GameObject panelGuideSpirit;      // drag Panel_GuideSpirit

    private void Start()
    {
        ShowPanel(panelMain);
    }

    public void ShowPanel(GameObject target)
    {
        panelMain.SetActive(target == panelMain);
        panelRoomSelect.SetActive(target == panelRoomSelect);
        panelCreateRoom.SetActive(target == panelCreateRoom);
        panelJoinRoom.SetActive(target == panelJoinRoom);

        // kalau masih di menu biasa, pastikan guide ketutup
        if (panelGuideDetective != null) panelGuideDetective.SetActive(false);
        if (panelGuideSpirit != null) panelGuideSpirit.SetActive(false);
    }


    // === NEW ===
    public void ShowGuideForLocalPlayer()
    {
        // matikan semua panel menu biasa
        panelMain.SetActive(false);
        panelRoomSelect.SetActive(false);
        panelCreateRoom.SetActive(false);
        panelJoinRoom.SetActive(false);

        bool isHost = NetworkManager.Singleton && NetworkManager.Singleton.IsHost;

        if (panelGuideDetective != null) panelGuideDetective.SetActive(isHost);
        if (panelGuideSpirit != null)    panelGuideSpirit.SetActive(!isHost);
    }
}