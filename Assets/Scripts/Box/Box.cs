using UnityEngine;

public class Box : MonoBehaviour, IObject, IStateObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI References")]
    public GameObject ControlUI;
    public GameObject BoxUI;

    private BoxUI puzzle;

    // 🔹 Tambahan untuk dependency
    public bool isSolved { get; private set; } = false;

    [Header("Components")]
    [SerializeField] private Collider2D interactionCollider;

    private ObjectState currentState;


    public void SetObjectState(ObjectState state)
    {
        currentState = state;

        switch (state)
        {
            case ObjectState.Disabled:
                interactionCollider.enabled = false;
                Debug.Log($"{name} state set to {state}");
                break;

            case ObjectState.Locked:
                interactionCollider.enabled = true;
                Debug.Log($"{name} state set to {state}");
                break;

            case ObjectState.Active:
                interactionCollider.enabled = true;
                Debug.Log($"{name} state set to {state}");
                break;
        }
    }

    void Start()
    {
        ID ??= GlobalHelper.GenerateUniqueID(gameObject);

        if (BoxUI != null)
        {
            BoxUI.SetActive(false);
            puzzle = BoxUI.GetComponent<BoxUI>();
            if (puzzle != null)
            {
                puzzle.onPuzzleDone += OnPuzzleDone;
                puzzle.onBatteryCollected += OnBatteryCollected;
            }
        }
    }

    private void OnBatteryCollected(int count)
    {
        isSolved = (count >= 2);

        // Kirim sinkronisasi ke ClockBack
        ClockBackBatterySync.Instance.UpdateBatteryCount(count);
    }


    public bool CanInteract()
    {
        return currentState == ObjectState.Active || currentState == ObjectState.Locked;
    }

    public void Interact(Transform player)
    {
        if (currentState == ObjectState.Disabled)
        {
            Debug.Log($"❌ {name} masih {currentState}, tidak bisa di-interact!");
            return;
        }

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
        Debug.Log("✅ Box Puzzle Done!");
        isSolved = true; // ✅ Puzzle selesai
        ClosePuzzle();
    }
}
