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

