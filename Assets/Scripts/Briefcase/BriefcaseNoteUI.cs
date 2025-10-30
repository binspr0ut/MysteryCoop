using UnityEngine;

public class BriefcaseNoteUI : MonoBehaviour
{
    [Header("Refs")]
    public GameObject notePanel;   // parent: NotePanel (berisi Dim + Note)

    void Start()
    {
        if (notePanel) notePanel.SetActive(false);
    }

    // dipanggil Button
    public void ShowNote()
    {
        if (notePanel) notePanel.SetActive(true);
    }

    // dipanggil Button
    public void HideNote()
    {
        if (notePanel) notePanel.SetActive(false);
    }
}