using UnityEngine;

public class ShelfOpened : MonoBehaviour, IObject, IStateObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }
    public GameObject ControlUI;

    public GameObject PaperPuzzleUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private ObjectState currentState = ObjectState.Disabled;



    [Header("SFX")]
    [SerializeField] private AudioClip openBoardSFX;
    [SerializeField] private AudioClip closeBoardSFX;



    [Header("Components")]
    [SerializeField] private Collider2D interactionCollider;

    public void SetObjectState(ObjectState state)
    {
        currentState = state;

        switch (state)
        {
            case ObjectState.Disabled:
                interactionCollider.enabled = false;
                break;

            case ObjectState.Locked:
                interactionCollider.enabled = true;
                break;

            case ObjectState.Active:
                interactionCollider.enabled = true;
                break;
        }
    }

    void Start()
    {

    }

    public void Interact(Transform player)
    {
        if (currentState == ObjectState.Disabled) return;

        if (currentState == ObjectState.Locked)
        {
            Debug.Log("🔒 Objek masih terkunci. Kamu memerlukan kunci.");

        }

        if (currentState == ObjectState.Active)
        {
            OpenPaperPuzzle();
        }

    }

    public bool CanInteract() => currentState == ObjectState.Active || currentState == ObjectState.Locked;

    public void OpenPaperPuzzle()
    {
        ControlUI.SetActive(false);
        PaperPuzzleUI.GetComponent<CanvasGroup>().alpha = 1;
        PaperPuzzleUI.GetComponent<CanvasGroup>().interactable = true;
        PaperPuzzleUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
        IsInteracted = true;

        // 🔊 SFX buka papan (detektif)
        PlaySFX(openBoardSFX);
    }

    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);
        PaperPuzzleUI.GetComponent<CanvasGroup>().alpha = 0;
        PaperPuzzleUI.GetComponent<CanvasGroup>().interactable = false;
        PaperPuzzleUI.GetComponent<CanvasGroup>().blocksRaycasts = false;
        IsInteracted = false;

        // 🔊 SFX buka papan (detektif)
        PlaySFX(closeBoardSFX);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }


}
