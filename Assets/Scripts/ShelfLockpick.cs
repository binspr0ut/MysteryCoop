using Unity.Netcode;
using UnityEngine;

public class ShelfLockpick : NetworkBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("UI References")]
    public GameObject ControlUI;
    public GameObject LockpickOverlay;

    void Start()
    {
        if (LockpickOverlay != null)
            LockpickOverlay.SetActive(false);
    }

    public bool CanInteract() => true;

    public void Interact(Transform playerTransform)
    {
        ControlUI.SetActive(false);
        LockpickOverlay.SetActive(true);
        IsInteracted = true;
    }

    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);
        LockpickOverlay.SetActive(false);
        IsInteracted = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
