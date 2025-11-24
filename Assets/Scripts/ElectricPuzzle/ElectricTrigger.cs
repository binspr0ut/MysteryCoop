// using UnityEngine;

// public class ElectricTrigger : MonoBehaviour, IObject
// {

//     private bool hasChangedState = false;

//     public bool CanInteract()
//     {
//         return !hasChangedState;
//     }

//     public void Interact(Transform player)
//     {
//         if (hasChangedState == false)
//         {
//             hasChangedState = true;
//             Scene1StateManager.Instance.ChangeState(Level1State.ExploreBuilding);

//         }
//     }

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {

//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }
// }

using UnityEngine;

public class ElectricTrigger : MonoBehaviour, IObject
{
    [Header("Puzzle Reference")]
    [SerializeField] private ElectricPuzzle electricPuzzle;

    // DEBUG: cek siapa yang masuk trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[ElectricTrigger] OnTriggerEnter2D by {other.name} (tag={other.tag}, layer={LayerMask.LayerToName(other.gameObject.layer)})");
    }

    // public bool CanInteract()
    // {
    //     // Kalau puzzle belum di-assign, ya nggak bisa
    //     if (electricPuzzle == null)
    //         return false;

    //     // Opsional: hanya bisa di-interact saat quest "TurnElectricity"
    //     if (Scene1StateManager.Instance == null)
    //         return true; // fallback aman

    //     return Scene1StateManager.Instance.CurrentState.Value == Level1State.TurnElectricity
    //            && !electricPuzzle.IsSolved;
    // }

     public bool CanInteract()
    {
        return Scene1StateManager.Instance.CurrentState.Value == Level1State.TurnElectricity
               && !electricPuzzle.IsOpen
               && !electricPuzzle.IsSolved;
    }


    public void Interact(Transform player)
    {
        if (!CanInteract())
            return;

        electricPuzzle.OpenForDetective();
    }

    void Start() { }
    void Update() { }
}
