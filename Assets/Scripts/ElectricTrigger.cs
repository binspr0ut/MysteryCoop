using UnityEngine;

public class ElectricTrigger : MonoBehaviour, IObject
{

    private bool hasChangedState = false;

    public bool CanInteract()
    {
        return !hasChangedState;
    }

    public void Interact(Transform player)
    {
        if (hasChangedState == false)
        {
            hasChangedState = true;
            Scene1StateManager.Instance.ChangeState(Level1State.ExploreBuilding);

        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
