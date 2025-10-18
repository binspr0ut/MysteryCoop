using UnityEngine;

public class Clock : MonoBehaviour, IObject
{
    public bool IsInteracted { get; private set; }

    public void Interact()
    {

    }

    public bool CanInteract()
    {
        return true;
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
