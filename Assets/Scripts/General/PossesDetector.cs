using UnityEngine;
using UnityEngine.InputSystem;

public class PossesDetector : MonoBehaviour
{
    private IPossess possessInRange = null;
    private IPossess currentPossessed = null;

    void Start()
    {
    }

    // Tekan tombol "interact" (ex: E)
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (context.performed || context.canceled)
        {
            possessInRange.Interact();

            // if (currentPossessed != null)
            // {
            //     Debug.Log("Interact while possessing");
            //     return;
            // }

            // // 🔸 Jika belum possess dan ada objek di range
            // if (possessInRange != null)
            // {
            //     Debug.Log("Possessing object...");
            //     possessInRange.Possess();
            //     currentPossessed = possessInRange; // simpan referensi aktif
            // }
        }

    }

    // Tekan tombol keluar (misal Q)
    public void OnExitPossess(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (currentPossessed != null)
        {
            Debug.Log("Unpossess pressed");
            currentPossessed.Unpossess();
            currentPossessed = null; // reset
        }
    }

    public event System.Action<bool> OnRangeChanged;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPossess iposses) && iposses.CanPossess())
        {
            possessInRange = iposses;
            OnRangeChanged?.Invoke(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPossess iposses) && iposses == possessInRange)
        {
            possessInRange = null;
            OnRangeChanged?.Invoke(false);
        }
    }

}

