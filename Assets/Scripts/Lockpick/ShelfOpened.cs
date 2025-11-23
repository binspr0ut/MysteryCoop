using UnityEngine;

public class ShelfOpened : MonoBehaviour, IObject, IStateObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }
    public GameObject ControlUI;

    public GameObject PaperPuzzleUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private ObjectState currentState = ObjectState.Disabled;


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
        PaperPuzzleUI.SetActive(true);
        IsInteracted = true;
    }

    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);
        PaperPuzzleUI.SetActive(false);
        IsInteracted = false;
    }

    // Update is called once per frame
    void Update()
    {

    }


}
