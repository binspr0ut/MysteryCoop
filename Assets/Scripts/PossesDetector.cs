using UnityEngine;
using UnityEngine.InputSystem;
public class PossesDetector : MonoBehaviour
{
    public IPossess possessInRange = null;
    public GameObject possesIcon;
    private IPossess currentPossessed = null;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        possesIcon.SetActive(false);
    }


    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentPossessed != null)
            {
                // Sudah merasuki, jalankan aksi di dalam objek
                possessInRange?.Interact();
            }
            else
            {
                Debug.Log("button pressed");
                possessInRange?.Possess();
                currentPossessed = possessInRange;
            }

        }
        else if (context.canceled)
        {
            if (currentPossessed != null)
            {
                // Sudah merasuki, jalankan aksi di dalam objek
                possessInRange?.Interact();
            }
            else
            {
                Debug.Log("button pressed");
                possessInRange?.Possess();
                currentPossessed = possessInRange;
            }

        }
    }

    public void OnExitPossess(InputAction.CallbackContext context)
    {
        if (context.performed && currentPossessed != null)
        {
            currentPossessed.Unpossess();
            currentPossessed = null;
        }
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPossess iposses) && iposses.CanPossess())
        {
            possessInRange = iposses;
            possesIcon.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPossess iposses) && iposses == possessInRange)
        {
            possessInRange = null;
            possesIcon.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
