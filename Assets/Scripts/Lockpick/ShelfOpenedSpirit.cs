using Unity.Netcode;
using UnityEngine;

public class ShelfOpenedSpirit : NetworkBehaviour, IPossess, IStateObject
{

    public bool IsPossessed { get; private set; }
    public string ID { get; private set; }
    public GameObject ControlUI;
    private SpiritMovement PossessedSpirit;


    public GameObject PaperPuzzleUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private ObjectState currentState = ObjectState.Locked;


    [Header("Components")]
    [SerializeField] private Collider2D interactionCollider;

    [Header("SFX")]
    [SerializeField] private AudioClip lockedShelfSFX;
    [SerializeField] private AudioClip openBoardSFX;
    [SerializeField] private AudioClip closeBoardSFX;


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
                Debug.Log("ShelfOpenedSpiritObject state to active");
                interactionCollider.enabled = true;
                break;
        }
    }

    void Start()
    {

    }

    public void Interact()
    {

        if (IsPossessed)
        {
            Debug.Log("Unposess Clock");
            Unpossess();
            // IsPossessed = false;
        }
        else
        {
            Debug.Log("Possess Clock");
            Possess();
            // IsPossessed = true;
        }
    }

    public void Possess()
    {
        if (currentState == ObjectState.Disabled) return;

        if (currentState == ObjectState.Locked)
        {

            // 🔊 SFX: lemari masih terkunci saat hantu coba possess
            if (AudioManager.Instance != null && lockedShelfSFX != null)
            {
                AudioManager.Instance.PlaySFX(lockedShelfSFX);
            }

            SubtitleManager.Instance.ShowSubtitle(
                            "This locker’s locked tight. Think you can help me, Agung?",
                            SubtitleTarget.Spirit,
                            SubtitleScope.Global
                        );

            // global untuk semua spirit
            SubtitleManager.Instance.ShowSubtitle(
                "I’ll try. Give me a moment.",
                SubtitleTarget.Detective,
                SubtitleScope.Global
            ); return;
        }

        if (currentState == ObjectState.Active)
        {

            // Cari spirit milik player lokal
            var spirit = FindFirstObjectByType<SpiritMovement>();
            if (spirit != null && spirit.IsOwner)
            {
                // 🔹 Sembunyikan spirit di semua client
                spirit.SetVisibleServerRpc(false);
                PossessedSpirit = spirit;
            }

            OpenPaperPuzzle();

            IsPossessed = true;
        }
    }

    public bool CanPossess() => currentState == ObjectState.Active || currentState == ObjectState.Locked;


    public void Unpossess()
    {
        if (IsHost)
        {
            ClosePuzzle();
            return;
        }

        if (PossessedSpirit != null)
        {
            // 🔹 Tampilkan kembali spirit di semua client
            PossessedSpirit.SetVisibleServerRpc(true);
            PossessedSpirit = null;
        }

        // 🔹 Tutup UI puzzle dan kembalikan control
        ClosePuzzle();

        IsPossessed = false;

        Debug.Log("[Clock] Unpossessed and puzzle closed.");
    }
    public bool CanInteract() => currentState == ObjectState.Active || currentState == ObjectState.Locked;

    // public void OpenPaperPuzzle()
    // {
    //     ControlUI.SetActive(false);
    //     PaperPuzzleUI.GetComponent<CanvasGroup>().alpha = 1;
    //     PaperPuzzleUI.GetComponent<CanvasGroup>().interactable = true;
    //     PaperPuzzleUI.GetComponent<CanvasGroup>().blocksRaycasts = true; IsPossessed = true;

    //     // 🔊 SFX buka papan (spirit)
    //     PlaySFX(openBoardSFX);
    // }

    public void OpenPaperPuzzle()
    {
        ControlUI.SetActive(false);

        var cg = PaperPuzzleUI.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        // 🔊 SFX buka papan (spirit)
        PlaySFX(openBoardSFX);
    }

    // public void ClosePuzzle()
    // {
    //     ControlUI.SetActive(true);
    //     PaperPuzzleUI.GetComponent<CanvasGroup>().alpha = 0;
    //     PaperPuzzleUI.GetComponent<CanvasGroup>().interactable = false;
    //     PaperPuzzleUI.GetComponent<CanvasGroup>().blocksRaycasts = false; IsPossessed = false;

    //     // 🔊 SFX tutup papan (spirit)
    //     PlaySFX(closeBoardSFX);
    // }

    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);

        var cg = PaperPuzzleUI.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        // 🔊 SFX tutup papan (spirit)
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
