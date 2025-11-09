using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelMain;
    public GameObject panelRoomSelect;
    public GameObject panelCreateRoom;
    public GameObject panelJoinRoom;

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
    }
}