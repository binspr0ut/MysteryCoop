using Unity.Netcode;
using UnityEngine;

public class ShelfLockpick : NetworkBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI References")]
    public GameObject ControlUI;
    public GameObject LockpickOverlay;
    public GameObject OpenedShelf;
    public GameObject ShelfUI;
    public GameObject GuestbookUI;

    public bool isSolved;
    void Start()
    {
        if (LockpickOverlay != null)
            LockpickOverlay.SetActive(false);
        if (OpenedShelf != null)
            OpenedShelf.SetActive(false);

        // Hubungkan otomatis LockpickUI dengan Shelf ini
        var ui = LockpickOverlay?.GetComponentInChildren<LockpickUI>();
        if (ui != null)
            ui.GetType().GetField("shelfLockpick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              ?.SetValue(ui, this);
    }

    public bool CanInteract() => true;

    public void Interact(Transform playerTransform)
    {
        if (!isSolved)
        {
            ControlUI.SetActive(false);
            LockpickOverlay.SetActive(true);
            IsInteracted = true;
        }
        else
        {
            ControlUI.SetActive(false);
            ShelfUI.SetActive(true);
            IsInteracted = true;
        }
    }

    public void OpenGuestbook()
    {
        ControlUI.SetActive(false);
        GuestbookUI.SetActive(true);
        IsInteracted = true;
    }

    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);
        LockpickOverlay.SetActive(false);
        ShelfUI.SetActive(false);
        GuestbookUI.SetActive(false);
        IsInteracted = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
