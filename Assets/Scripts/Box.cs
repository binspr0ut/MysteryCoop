using UnityEngine;

public class Box : MonoBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI References")]
    public GameObject ControlUI;
    public GameObject BoxUI;

    private BoxUI puzzle;

    void Start()
    {
        ID ??= GlobalHelper.GenerateUniqueID(gameObject);

        if (BoxUI != null)
        {
            BoxUI.SetActive(false);
            puzzle = BoxUI.GetComponent<BoxUI>();
            if (puzzle != null)
                puzzle.onPuzzleDone += OnPuzzleDone;
        }
    }

    public bool CanInteract() => true;

    public void Interact(Transform player)
    {
        IsInteracted = true;
        ControlUI.SetActive(false);
        BoxUI.SetActive(true);
    }

    public void ClosePuzzle()
    {
        IsInteracted = false;
        ControlUI.SetActive(true);
        BoxUI.SetActive(false);
    }

    private void OnPuzzleDone()
    {
        Debug.Log("Box Puzzle Done!");
        ClosePuzzle();
    }
}
