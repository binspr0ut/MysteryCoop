using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Collider2D))]
public class SubtitleTrigger : MonoBehaviour
{
    [Header("Subtitle Settings")]
    [TextArea(2, 5)]
    public string subtitleText = "Ini adalah subtitle contoh.";
    [TextArea(2, 5)]
    public string subtitleText2 = "Ini adalah subtitle contoh.";


    public SubtitleTarget target = SubtitleTarget.Detective;
    public SubtitleTarget target2 = SubtitleTarget.Spirit;

    public SubtitleScope scope = SubtitleScope.Global;

    [Header("Trigger Settings")]
    public int counter = 0;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true; // otomatis jadikan trigger
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (counter != 0) { return; }

        // pastikan hanya player yang bisa memicu
        if (!other.CompareTag("Detective")) return;

        // Pastikan SubtitleManager sudah aktif
        if (SubtitleManager.Instance == null)
        {
            Debug.LogWarning("[SubtitleTrigger] SubtitleManager belum ada di scene.");
            return;
        }

        SubtitleManager.Instance.ShowSubtitle(subtitleText, target, scope);
        SubtitleManager.Instance.ShowSubtitle(subtitleText, target2, scope);
        SubtitleManager.Instance.ShowSubtitle(subtitleText2, target, scope);
        SubtitleManager.Instance.ShowSubtitle(subtitleText2, target2, scope);

    }
}
