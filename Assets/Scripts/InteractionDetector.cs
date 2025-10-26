using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IObject objectInRange = null;
    public GameObject interactionIcon;

    void Start()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IObject iobject) && iobject.CanInteract())
        {
            objectInRange = iobject;
            if (interactionIcon != null)
                interactionIcon.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IObject iobject) && iobject == objectInRange)
        {
            objectInRange = null;
            if (interactionIcon != null)
                interactionIcon.SetActive(false);
        }
    }
}
