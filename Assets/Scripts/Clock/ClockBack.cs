using UnityEngine;

public class ClockBack : MonoBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI References")]
    public GameObject ControlUI;
    public GameObject ClockBackUI;

    [Header("Puzzle Elements")]
    public GameObject battery; // Battery di UI
    public Clock clockTarget;  // Referensi ke Clock (IPossess)
    public Box boxDependency;  // Referensi ke Box puzzle
    public bool isSolved = false;

    private ClockBackUI puzzleUI;

    void Start()
    {
        ID ??= System.Guid.NewGuid().ToString();

        if (ClockBackUI != null)
        {
            ClockBackUI.SetActive(false);
            puzzleUI = ClockBackUI.GetComponent<ClockBackUI>();
            if (puzzleUI != null)
                puzzleUI.onPuzzleDone += OnPuzzleDone;
        }

        // Awalnya battery tidak aktif
        if (battery != null)
            battery.SetActive(false);
    }

    public bool CanInteract() => !isSolved;

    public void Interact(Transform player)
    {
        // Hanya detective yang bisa
        var detective = player.GetComponent<DetectiveMovement>();
        if (detective == null)
        {
            Debug.Log("❌ Only detective can interact with ClockBack!");
            return;
        }

        IsInteracted = true;
        ControlUI.SetActive(false);
        ClockBackUI.SetActive(true);

        // 🔹 Cek status puzzle Box
        if (boxDependency != null)
        {
            if (boxDependency.isSolved)
            {
                Debug.Log("🔋 Box solved! Battery visible.");
                battery.SetActive(true);
            }
            else
            {
                Debug.Log("🔒 Box puzzle not solved yet! Battery hidden.");
                battery.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("⚠ No Box dependency assigned on ClockBack.");
        }
    }

    public void ClosePuzzle()
    {
        IsInteracted = false;
        ControlUI.SetActive(true);
        ClockBackUI.SetActive(false);
    }

    private void OnPuzzleDone()
    {
        isSolved = true;
        Debug.Log("✅ ClockBack Puzzle Done! Unlocking Clock...");

        ClosePuzzle();

        // Aktifkan Clock agar bisa di-posses
        if (clockTarget != null)
        {
            clockTarget.enabled = true;
            Debug.Log("🕰 Clock puzzle unlocked for Spirit!");
        }
    }
}
