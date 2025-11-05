using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI questText;
    private bool subscribed;

    void OnEnable()
    {
        TrySubscribe();
        InvokeRepeating(nameof(TrySubscribe), 0.5f, 0.5f);
    }

    void OnDisable()
    {
        Unsubscribe();
        CancelInvoke(nameof(TrySubscribe));
    }

    void TrySubscribe()
    {
        if (subscribed) return;
        if (Scene1StateManager.Instance == null) return;

        Scene1StateManager.Instance.OnQuestTitleChanged += OnQuestChanged;
        subscribed = true;

        questText.text = Scene1StateManager.Instance.GetTitle();
    }

    void Unsubscribe()
    {
        if (!subscribed) return;
        if (Scene1StateManager.Instance != null)
            Scene1StateManager.Instance.OnQuestTitleChanged -= OnQuestChanged;

        subscribed = false;
    }

    private void OnQuestChanged(Level1State state, string title)
    {
        questText.text = title;
    }
}


// using UnityEngine;
// using TMPro;

// public class QuestUI : MonoBehaviour
// {
//     // --- Shim agar kompatibel dengan QuestManager_Network lama ---
//     public static System.Action<string> UpdateAllClientsUI;

//     [SerializeField] TextMeshProUGUI questText;

//     string _lastTitle = null;
//     bool _hooked = false;

//     void OnEnable()
//     {
//         // hook shim
//         UpdateAllClientsUI += SetText;

//         TryHook();
//         RefreshNow();
//         InvokeRepeating(nameof(PollRefresh), 0.2f, 0.2f);
//     }

//     void OnDisable()
//     {
//         UpdateAllClientsUI -= SetText;

//         Unhook();
//         CancelInvoke(nameof(PollRefresh));
//     }

//     void TryHook()
//     {
//         if (_hooked) return;
//         if (QuestManager_Network.I == null) return;

//         // subscribe langsung ke NetworkVariable biar auto-update
//         QuestManager_Network.I.currentState.OnValueChanged += OnStateChanged;
//         _hooked = true;
//     }

//     void Unhook()
//     {
//         if (!_hooked) return;
//         if (QuestManager_Network.I != null)
//             QuestManager_Network.I.currentState.OnValueChanged -= OnStateChanged;
//         _hooked = false;
//     }

//     void OnStateChanged(QuestState oldV, QuestState newV)
//     {
//         SetText(QuestManager_Network.I.GetCurrentTitle());
//     }

//     void PollRefresh()
//     {
//         if (QuestManager_Network.I == null)
//         {
//             TryHook(); // kalau manager baru muncul
//             return;
//         }
//         var title = QuestManager_Network.I.GetCurrentTitle();
//         if (title != _lastTitle)
//             SetText(title);
//     }

//     void RefreshNow()
//     {
//         if (QuestManager_Network.I != null)
//             SetText(QuestManager_Network.I.GetCurrentTitle());
//     }

//     void SetText(string s)
//     {
//         _lastTitle = s;
//         if (questText != null) questText.text = s;
//     }
// }