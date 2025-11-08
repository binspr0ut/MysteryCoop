using System;
using Coop;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoleSelectUI : NetworkBehaviour
{
    [Header("Root")]
    [Tooltip("Overlay/panel induk yang menimpa layar")]
    public CanvasGroup rootOverlay;               // set alpha 0/blocksRaycasts false saat hidden

    [Header("Cards & Covers")]
    public GameObject detectiveCard;              // kartu detektif (gambar utama)
    public GameObject detectiveCover;             // penutup detektif saat tidak dipilih
    public GameObject spiritCard;                 // kartu spirit (gambar utama)
    public GameObject spiritCover;                // penutup spirit saat tidak dipilih

    [Header("Detail Panels (lokal)")]
    public GameObject detailDetective;            // panel info detektif
    public GameObject detailSpirit;               // panel info spirit

    [Header("Buttons")]
    public Button btnDetective;                   // klik pilih Detektif (host-only)
    public Button btnSpirit;                      // klik pilih Spirit (host-only)
    public Button btnInfoDetective;               // toggle detail
    public Button btnInfoSpirit;                  // toggle detail
    public Button btnStartPlay;                   // host-only, aktif jika role dipilih

    [Header("Texts")]
    public TMP_Text txtPlayer1;                   // label "Player 1"
    public TMP_Text txtPlayer2;                   // label "Player 2"
    public TMP_Text txtStatus;                    // optional: status kecil (e.g., "Waiting host...")

    [Header("Integration")]
    [Tooltip("Opsional. Komponen pengendali fase yang mengubah Cutscene -> Gameplay")]
    public GamePhaseController phaseController;   // boleh null, akan dicari otomatis

    // ===== Network state =====
    private readonly NetworkVariable<PlayerRole> hostSelectedRole =
        new NetworkVariable<PlayerRole>(PlayerRole.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool _isOpen;

    #region Unity / Network

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Siapa Player1/Player2
        if (txtPlayer1) txtPlayer1.text = "Player 1";
        if (txtPlayer2) txtPlayer2.text = "Player 2";

        // Host bisa meng-klik pilihan, client tidak
        bool isHost = IsServer;
        btnDetective.interactable = isHost;
        btnSpirit.interactable = isHost;
        btnStartPlay.interactable = false; // aktif setelah pilih

        // Info buttons: selalu boleh
        btnInfoDetective.onClick.AddListener(() => ToggleDetail(detailDetective));
        btnInfoSpirit.onClick.AddListener(() => ToggleDetail(detailSpirit));

        // Role buttons
        btnDetective.onClick.AddListener(() => { if (IsServer) SetRoleServerRpc(PlayerRole.Detective); });
        btnSpirit.onClick.AddListener(() => { if (IsServer) SetRoleServerRpc(PlayerRole.Spirit); });

        // Start
        btnStartPlay.onClick.AddListener(OnStartPlayClicked);

        hostSelectedRole.OnValueChanged += OnRoleChanged;

        ApplyVisual(hostSelectedRole.Value);
    }

    private void OnDestroy()
    {
        hostSelectedRole.OnValueChanged -= OnRoleChanged;
    }

    #endregion

    #region Public API

    /// <summary> Dipanggil saat cutscene selesai & player tap layar (dari CutsceneController).</summary>
    public void Open()
    {
        if (_isOpen) return;

        if (!phaseController) phaseController = FindObjectOfType<GamePhaseController>();
        ShowOverlay(true);
        _isOpen = true;

        // reset UI detail
        if (detailDetective) detailDetective.SetActive(false);
        if (detailSpirit) detailSpirit.SetActive(false);
        btnStartPlay.interactable = (IsServer && hostSelectedRole.Value != PlayerRole.None);

        // Client: beri tahu kalau menunggu host memilih
        if (!IsServer && txtStatus) txtStatus.text = "Waiting for host to select a role...";
        else if (txtStatus) txtStatus.text = "";
    }

    /// <summary> Tutup overlay secara lokal. Dipanggil setelah Start Play.</summary>
    public void Close()
    {
        ShowOverlay(false);
        _isOpen = false;
    }

    #endregion

    #region UI Helpers

    private void ShowOverlay(bool show)
    {
        if (!rootOverlay) return;
        rootOverlay.alpha = show ? 1f : 0f;
        rootOverlay.blocksRaycasts = show;
        rootOverlay.interactable = show;
    }

    private void ToggleDetail(GameObject go)
    {
        if (!go) return;

        // Tutup yang lain supaya tidak tumpang tindih
        if (go != detailDetective && detailDetective) detailDetective.SetActive(false);
        if (go != detailSpirit && detailSpirit) detailSpirit.SetActive(false);

        go.SetActive(!go.activeSelf);
    }

    private void OnStartPlayClicked()
    {
        if (!IsServer) return;
        if (hostSelectedRole.Value == PlayerRole.None) return;

        // Pindah fase ke Gameplay (masih dalam TextObjective scene)
        if (!phaseController) phaseController = FindObjectOfType<GamePhaseController>();
        if (phaseController) phaseController.StartGameplayServerRpc(); // pastikan method ini ada di controller-mu

        Close();
    }

    #endregion

    #region Visual Sync

    private void OnRoleChanged(PlayerRole prev, PlayerRole next)
    {
        ApplyVisual(next);
        if (IsServer) btnStartPlay.interactable = (next != PlayerRole.None);
    }

    private void ApplyVisual(PlayerRole hostRole)
    {
        // Host pilih X -> Client otomatis Y
        // Visual:
        //  - kartu yang DIPILIH: tampil (cover OFF)
        //  - kartu yang tidak dipilih: tertutup (cover ON)

        bool detectiveChosen = (hostRole == PlayerRole.Detective);
        bool spiritChosen = (hostRole == PlayerRole.Spirit);

        // Cover logic
        if (detectiveCover) detectiveCover.SetActive(!detectiveChosen);
        if (spiritCover)    spiritCover.SetActive(!spiritChosen);

        // Optional: highlight kartu aktif, dsb. (bisa tambahkan animasi/scale)
    }

    #endregion

    #region RPC

    [ServerRpc(RequireOwnership = false)]
    private void SetRoleServerRpc(PlayerRole role)
    {
        if (role == PlayerRole.None) return;
        hostSelectedRole.Value = role;

        // Kamu bisa simpan mapping role → clientId di Session/State milikmu di sini.
        // Misal:
        // var state = FindObjectOfType<SessionState>();
        // state.Assign(hostId, role); state.Assign(clientId, Inverse(role));
    }

    #endregion
}