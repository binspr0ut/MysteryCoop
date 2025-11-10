using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class InteractionDetector : MonoBehaviour
{
    private IObject objectInRange = null;


    void Start()
    {

    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        // Only when the key is PRESSED, not when released
        if (!context.performed) return;

        if (objectInRange == null)
        {
            Debug.LogWarning("No interactable object in range!");
            return;
        }

        var netObj = GetComponentInParent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogWarning("No NetworkObject found in parent!");
            return;
        }

        objectInRange.Interact(netObj.transform);
    }
    public event System.Action<bool> OnRangeChanged;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IObject iobject) && iobject.CanInteract())
        {
            objectInRange = iobject;
            OnRangeChanged?.Invoke(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IObject iobject) && iobject == objectInRange)
        {
            objectInRange = null;
            OnRangeChanged?.Invoke(false);
        }
    }

}
