using UnityEngine;

public class Clock : MonoBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("Clock Puzzle UI")]
    public GameObject ClockPuzzleUI;
    public GameObject ControlUI;


    public bool CanInteract() => true;

    public void Interact()
    {
        ControlUI.SetActive(false);
        ClockPuzzleUI.SetActive(true);
        IsInteracted = true;
    }

    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);
        ClockPuzzleUI.SetActive(false);
        IsInteracted = false;
    }

    private void Start()
    {
        ID ??= GlobalHelper.GenerateUniqueID(gameObject);

        if (ClockPuzzleUI != null)
            ClockPuzzleUI.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
