using UnityEngine;
using TMPro;

public class QuestUI_Network : MonoBehaviour
{
    // --- Shim agar kompatibel dengan QuestManager_Network lama ---
    public static System.Action<string> UpdateAllClientsUI;

    [SerializeField] TextMeshProUGUI questText;

    string _lastTitle = null;
    bool _hooked = false;

    void OnEnable()
    {
        // hook shim
        UpdateAllClientsUI += SetText;

        TryHook();
        RefreshNow();
        InvokeRepeating(nameof(PollRefresh), 0.2f, 0.2f);
    }

    void OnDisable()
    {
        UpdateAllClientsUI -= SetText;

        Unhook();
        CancelInvoke(nameof(PollRefresh));
    }

    void TryHook()
    {
        if (_hooked) return;
        if (QuestManager_Network.I == null) return;

        // subscribe langsung ke NetworkVariable biar auto-update
        QuestManager_Network.I.currentState.OnValueChanged += OnStateChanged;
        _hooked = true;
    }

    void Unhook()
    {
        if (!_hooked) return;
        if (QuestManager_Network.I != null)
            QuestManager_Network.I.currentState.OnValueChanged -= OnStateChanged;
        _hooked = false;
    }

    void OnStateChanged(QuestState oldV, QuestState newV)
    {
        SetText(QuestManager_Network.I.GetCurrentTitle());
    }

    void PollRefresh()
    {
        if (QuestManager_Network.I == null)
        {
            TryHook(); // kalau manager baru muncul
            return;
        }
        var title = QuestManager_Network.I.GetCurrentTitle();
        if (title != _lastTitle)
            SetText(title);
    }

    void RefreshNow()
    {
        if (QuestManager_Network.I != null)
            SetText(QuestManager_Network.I.GetCurrentTitle());
    }

    void SetText(string s)
    {
        _lastTitle = s;
        if (questText != null) questText.text = s;
    }
}