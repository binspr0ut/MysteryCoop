using UnityEngine;
using Unity.Netcode;

public class Briefcase : NetworkBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI")]
    public GameObject LockPanel;   // Panel input kode
    public GameObject ControlUI;   // HUD kontrol (disembunyikan saat panel tampil)

    [Header("Settings")]
    public string CorrectCode = "389";

    // Referensi internal
    private BriefcaseLockPanel lockPanelScript;

    // Mencegah trigger berulang
    private bool explorationMarkedOnServer = false;

    private int counter = 0;

    // ========================================================================
    // INTERACTION
    // ========================================================================
    public bool CanInteract() => true;

    public void Interact(Transform playerTransform)
    {
        if (counter == 0)
        {
            SubtitleManager.Instance.ShowSubtitle(
                "Agung: It seems this briefcase needs a code to be opened!",
                SubtitleTarget.Detective,
                SubtitleScope.Global
            );

            SubtitleManager.Instance.ShowSubtitle(
                "Agung: Look, there's a note lying on the briefcase",
                SubtitleTarget.Detective,
                SubtitleScope.Global
            );

            counter++;
        }

        if (ControlUI != null)
            ControlUI.SetActive(false);

        if (LockPanel == null)
        {
            Debug.LogError("[Briefcase] ❌ LockPanel belum di-assign!");
            return;
        }

        lockPanelScript = LockPanel.GetComponent<BriefcaseLockPanel>();
        if (lockPanelScript == null)
        {
            Debug.LogError("[Briefcase] ❌ Komponen BriefcaseLockPanel tidak ditemukan di LockPanel!");
            return;
        }

        // Aktifkan UI lokal
        LockPanel.SetActive(true);
        lockPanelScript.Init(this, CorrectCode);
        IsInteracted = true;
    }

    public void ClosePuzzle()
    {
        if (LockPanel != null) LockPanel.SetActive(false);
        if (ControlUI != null) ControlUI.SetActive(true);
        IsInteracted = false;
    }

    // ========================================================================
    // VALIDATION LOGIC (dipanggil dari LockPanel)
    // ========================================================================
    public void ValidateCodeFromUI(string entered)
    {
        // Kalau client yang tekan Enter
        if (!IsServer)
        {
            ValidateCodeServerRpc(entered);
            return;
        }

        // Kalau host yang tekan Enter
        ValidateCode(entered);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ValidateCodeServerRpc(string entered)
    {
        ValidateCode(entered);
    }

    private void ValidateCode(string entered)
    {
        Debug.Log($"[Briefcase] Entered code: {entered}");

        if (entered == CorrectCode)
        {
            Debug.Log("[Briefcase] ✅ Correct code! Notifying all players...");
            OnCorrectCodeClientRpc();
        }
        else
        {
            Debug.Log("[Briefcase] ❌ Wrong code! Shaking digits...");
            OnWrongCodeClientRpc();
        }
    }

    // ========================================================================
    // RPC BROADCASTS
    // ========================================================================
    [ClientRpc]
    private void OnCorrectCodeClientRpc()
    {
        Debug.Log("[Briefcase] 🎉 Correct code received on all clients!");

        // Tutup panel
        ClosePuzzle();

        if (IsServer && SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.PlayCutscene("KoperCutscene", "BriefcaseOpenedScene");
        }

    }

    [ClientRpc]
    private void OnWrongCodeClientRpc()
    {
        if (lockPanelScript != null)
        {
            lockPanelScript.StartCoroutine(lockPanelScript.ShakeDigits());
        }
    }

    // ========================================================================
    // LIFECYCLE
    // ========================================================================
    private void Start()
    {
        ID ??= GlobalHelper.GenerateUniqueID(gameObject);
        if (LockPanel != null)
        {
            LockPanel.SetActive(false);
            lockPanelScript = LockPanel.GetComponent<BriefcaseLockPanel>();
        }
    }
}
