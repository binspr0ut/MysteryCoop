using UnityEngine;

public class ElectricTriggerSpirit : MonoBehaviour, IPossess
{
    [Header("Puzzle Reference")]
    [SerializeField] private ElectricPuzzle electricPuzzle;
    private bool IsPossessed;

    // Helper: cek apakah puzzle boleh dipakai
    private bool CanUsePuzzle()
    {
        if (electricPuzzle == null) return false;
        if (electricPuzzle.IsSolved) return false;

        if (Scene1StateManager.Instance == null) return false;

        return Scene1StateManager.Instance.CurrentState.Value == Level1State.TurnElectricity;
    }

    public bool CanPossess()
    {
        return CanUsePuzzle();
    }

    // Dipanggil saat arwah mulai mem-possess panel ini
    public void Possess()
    {
        if (!CanUsePuzzle()) return;

        // Kalau mau, bisa langsung buka puzzle ketika possess
        electricPuzzle.OpenForSpirit();
        Debug.Log("[ElectricTriggerSpirit] Possess → open puzzle");
    }

    // Dipanggil saat arwah "interact" ketika lagi possess
    public void Interact()
    {
        if (!CanUsePuzzle()) return;

        // Untuk sekarang bisa kosong / sama seperti Possess
        // Nanti bisa dipakai untuk input lain (misal toggle mode kabel)
        Debug.Log("[ElectricTriggerSpirit] Interact()");

        if (IsPossessed)
        {
            Unpossess();
        }
        else
        {
            Possess();
        }
        
    }

    // Dipanggil saat arwah keluar dari benda ini
    public void Unpossess()
    {
        if (electricPuzzle != null && electricPuzzle.IsOpen)
        {
            electricPuzzle.ClosePuzzle();
            Debug.Log("[ElectricTriggerSpirit] Unpossess → close puzzle");
        }
    }
}