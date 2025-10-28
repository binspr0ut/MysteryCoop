using UnityEngine;

public class BriefcaseContentsPanel : MonoBehaviour
{
    private Briefcase briefcase;

    // kalau nanti ada item di dalam, kamu bisa expose event di sini
    // [SerializeField] private Button takeNoteButton; ...

    public void Init(Briefcase owner)
    {
        briefcase = owner;
        // siapkan isi panel (spawn item, reset state, dsb.)
    }

    public void PressClose()
    {
        briefcase?.ClosePuzzle();
    }
}