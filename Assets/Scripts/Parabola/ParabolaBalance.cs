using UnityEngine;

public class ParabolaBalance : MonoBehaviour, IPossess, IStateObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI")]
    public GameObject BalanceUI;   // panel mini-game (ParabolaUI)
    public GameObject ControlUI;   // UI kontrol umum (disembunyikan saat puzzle)

    [Header("Visual (opsional)")]
    public Transform dishHead;     // kepala parabola
    [Range(0f, 30f)] public float maxHeadAngle = 15f;
    [Range(0f, 10f)] public float headFollow = 4f;

    private float _stability01;
    private bool _isPossessed;
    private Quaternion _dishStartRot;

    [Header("Components")]
    [SerializeField] private Collider2D interactionCollider;

    private ObjectState currentState = ObjectState.Disabled;


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

    // ------------------------- IPossess Implementation -------------------------

    public bool CanPossess() => true;

    public void Possess()
    {
        _isPossessed = true;
        OpenPuzzle();
    }

    public void Unpossess()
    {
        _isPossessed = false;
        ClosePuzzle();
    }

    public void Interact()
    {
        // tidak perlu apa-apa, karena puzzle langsung terbuka di Possess()
    }

    // ---------------------------- Puzzle Flow ----------------------------------

    private void OpenPuzzle()
    {
        if (ControlUI) ControlUI.SetActive(false);

        if (BalanceUI)
        {
            var ui = BalanceUI.GetComponent<ParabolaBalanceUI>();
            if (ui) ui.Init(this);
            BalanceUI.SetActive(true);
        }

        IsInteracted = true;
    }

    public void ClosePuzzle()
    {
        if (BalanceUI) BalanceUI.SetActive(false);
        if (ControlUI) ControlUI.SetActive(true);
        IsInteracted = false;

        if (dishHead) dishHead.localRotation = _dishStartRot;
    }

    public void ReportStability(float s01)
    {
        _stability01 = Mathf.Clamp01(s01);

        // gerakan kepala parabola procedural
        if (dishHead)
        {
            float targetZ = Mathf.Lerp(-maxHeadAngle, maxHeadAngle, _stability01);
            var e = dishHead.localEulerAngles;
            float z = Mathf.LerpAngle(e.z, targetZ, headFollow * Time.unscaledDeltaTime);
            dishHead.localEulerAngles = new Vector3(e.x, e.y, z);
        }
    }

    public void OnSolved()
    {
        // di sini kamu bisa pasang flag untuk Radio puzzle
        ClosePuzzle();
    }

    private void Start()
    {
        ID ??= GlobalHelper.GenerateUniqueID(gameObject);
        if (BalanceUI) BalanceUI.SetActive(false);
        if (dishHead) _dishStartRot = dishHead.localRotation;
    }
}