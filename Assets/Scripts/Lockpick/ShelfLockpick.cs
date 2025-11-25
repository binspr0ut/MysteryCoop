using Unity.Netcode;
using UnityEngine;

public class ShelfLockpick : NetworkBehaviour, IObject, IStateObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI References")]
    public GameObject ControlUI;
    public GameObject LockpickOverlay;
    public GameObject OpenedShelf;

    public GameObject LockpickTrigger;
    public bool isSolved;
    private SpiritMovement PossessedSpirit;

    public MonoBehaviour ShelfOpened;
    public MonoBehaviour ShelfOpenedSpirit;

    [Header("SFX")]
    [SerializeField] private AudioClip openLockpickUISFX; //buka UI lockPIG
    [SerializeField] private AudioClip lockpickSolvedSFX;    // semua pin benar


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

    private void Awake()
    {
        Debug.Log($"ShelfLockpick implements IStateObject? {this is IStateObject}");
    }


    void Start()
    {
        if (LockpickOverlay != null)
            LockpickOverlay.SetActive(false);

        // Hubungkan otomatis LockpickUI dengan Shelf ini
        var ui = LockpickOverlay?.GetComponentInChildren<LockpickUI>();
        if (ui != null)
            ui.GetType().GetField("shelfLockpick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              ?.SetValue(ui, this);
    }

    public void Interact(Transform player)
    {
        if (currentState == ObjectState.Disabled) return;


        if (currentState == ObjectState.Locked)
        {
            Debug.Log("🔒 Objek masih terkunci. Kamu memerlukan kunci.");
            // tampilkan UI "Memerlukan kunci"
            return;
        }

        if (currentState == ObjectState.Active)
        {
            ControlUI.SetActive(false);
            LockpickOverlay.SetActive(true);
            IsInteracted = true;

            // 🔊 SFX: buka UI lockpick
            PlaySFX(openLockpickUISFX);
        }
    }


    public bool CanInteract() => currentState == ObjectState.Active || currentState == ObjectState.Locked;

    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);
        LockpickOverlay.SetActive(false);
        IsInteracted = false;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }

    public System.Action OnShelfUnlocked;

    [ServerRpc(RequireOwnership = false)]
    public void UnlockShelfServerRpc()
    {
        isSolved = true;
        UpdateShelfClientRpc();

        // 🔥 Trigger event untuk beri tahu State Manager
        OnShelfUnlocked?.Invoke();

    }

    [ClientRpc]
    private void UpdateShelfClientRpc()
    {
        LockpickOverlay.SetActive(false);
        OpenedShelf.SetActive(true);
        LockpickTrigger.SetActive(false);
        if (ShelfOpened is IStateObject so)
            so.SetObjectState(ObjectState.Active);

        if (ShelfOpenedSpirit is IStateObject si)
            si.SetObjectState(ObjectState.Active);

        // 🔊 SFX: semua pin benar, lemari berhasil kebuka
        PlaySFX(lockpickSolvedSFX);
    }
}
