using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MainMenuUI ui;
    [SerializeField] private RelayManager relay;
    [SerializeField] private string cutsceneScene = "TextObjective";
    [SerializeField] private string firstFloorScene = "FirstFloor";

    [Header("Buttons")]
    [SerializeField] private Button btnStart;
    [SerializeField] private Button btnCreateRoom;
    [SerializeField] private Button btnJoinRoom;
    [SerializeField] private Button btnSettings;
    [SerializeField] private Button btnBack;

    private void Start()
    {
        btnStart.onClick.AddListener(() => ui.ShowPanel(ui.panelRoomSelect));
        btnCreateRoom.onClick.AddListener(() => ui.ShowPanel(ui.panelCreateRoom));
        btnJoinRoom.onClick.AddListener(() => ui.ShowPanel(ui.panelJoinRoom));
        btnBack.onClick.AddListener(() => ui.ShowPanel(ui.panelMain));
    }

    public void LoadFirstFloor() => SceneManager.LoadScene(firstFloorScene);
    public void LoadCutscene() => SceneManager.LoadScene(cutsceneScene);
}
