using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IObject objectInRange = null;
    public GameObject interactionIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("button pressed");
            objectInRange?.Interact();
        }
        else if (context.canceled)
        {
            Debug.Log("button slightly pressed");
            objectInRange?.Interact();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IObject iobject) && iobject.CanInteract())
        {
            objectInRange = iobject;
            interactionIcon.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IObject iobject) && iobject == objectInRange)
        {
            objectInRange = null;
            interactionIcon.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
