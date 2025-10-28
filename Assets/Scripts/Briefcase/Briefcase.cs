using UnityEngine;

public class Briefcase : MonoBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI")]
    public GameObject LockPanel;   // panel input kode
    public GameObject ControlUI;   // HUD kontrol (disembunyikan saat panel tampil)

    [Header("Settings")]
    public string CorrectCode = "4931";

    public bool CanInteract() => true;

    public void Interact(Transform playerTransform)
    {
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

    public void OnUnlocked()
    {
        // Di tahap ini cukup tutup panel; nanti kita bisa tambah buka-isi koper.
        ClosePuzzle();
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