using UnityEngine;
using UnityEngine.InputSystem;

public class PossesDetector : MonoBehaviour
{

    [Header("SFX Possession")]
    [SerializeField] private AudioClip possessSFX;
    [SerializeField] private AudioClip unpossessSFX;

    private IPossess possessInRange = null;
    private IPossess currentPossessed = null;

    void Start()
    {
    }

    // Tekan tombol "interact" (ex: E)
    // public void OnInteract(InputAction.CallbackContext context)
    // {
    //     if (!context.performed) return;

    //     if (context.performed || context.canceled)
    //     {

    //         // 🔊 SFX POSSESS
    //         if (AudioManager.Instance != null)
    //         {
    //             AudioManager.Instance.PlaySFX(possessSFX);
    //         }

    //         possessInRange.Interact();

    //         // if (currentPossessed != null)
    //         // {
    //         //     Debug.Log("Interact while possessing");
    //         //     return;
    //         // }

    //         // // 🔸 Jika belum possess dan ada objek di range
    //         // if (possessInRange != null)
    //         // {
    //         //     Debug.Log("Possessing object...");
    //         //     possessInRange.Possess();
    //         //     currentPossessed = possessInRange; // simpan referensi aktif
    //         // }
    //     }

    // }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (possessInRange == null) return;

        // 🔍 Cek dulu: object ini lagi bisa dipossess atau tidak?
        // CanPossess() = true  → kita akan MASUK possess
        // CanPossess() = false → kita akan KELUAR possess (unpossess)
        bool willPossess = possessInRange.CanPossess();

        // 🔊 Pilih SFX sesuai aksi yang akan terjadi
        if (AudioManager.Instance != null)
        {
            AudioClip clip = willPossess ? possessSFX : unpossessSFX;
            if (clip != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
        }

        // Lalu jalankan logic sebenarnya di object (toggle possess/unpossess)
        possessInRange.Interact();
    }

    // Tekan tombol keluar (misal Q)
    public void OnExitPossess(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (currentPossessed != null)
        {
            Debug.Log("Unpossess pressed");

            // 🔊 SFX UNPOSSESS
            if (AudioManager.Instance != null && unpossessSFX != null)
            {
                AudioManager.Instance.PlaySFX(unpossessSFX);
            }

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

