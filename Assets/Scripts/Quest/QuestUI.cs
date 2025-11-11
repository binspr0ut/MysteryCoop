using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI questText;
    [SerializeField] private RectTransform bgObjective;
    [SerializeField] private RectTransform quest;


    [Header("Animation Settings")]
    public float slideDistance = 400f;  // seberapa jauh dari luar layar
    public float slideDuration = 0.4f;  // durasi animasi


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

        // --- Animasi slide in ---
        AnimateIn(bgObjective);
        AnimateIn(quest);
    }

    private void AnimateIn(RectTransform target)
    {
        if (target == null) return;

        // posisi awal: di atas layar (ke atas dari posisi aslinya)
        Vector2 startPos = target.anchoredPosition + Vector2.up * slideDistance;
        Debug.Log(startPos);

        // simpan posisi akhir
        Vector2 endPos = target.anchoredPosition;
        Debug.Log(endPos);

        // mulai dari atas
        target.anchoredPosition = startPos;
        Debug.Log(target.anchoredPosition);


        // animasi turun ke posisi semula
        LeanTween.move(target, endPos, slideDuration)
                 .setEaseOutBack();

        // opsional: efek fade-in
        CanvasGroup cg = target.GetComponent<CanvasGroup>() ?? target.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        LeanTween.value(target.gameObject, 0, 1, slideDuration).setOnUpdate(v => cg.alpha = v);
    }


}

