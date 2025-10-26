using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class Door : NetworkBehaviour, IObject
{
    public GameObject DoorOpen;
    public GameObject DoorClosed;
    public bool isOpen = false;

    public bool CanInteract() => true;

    public void Interact(Transform player)
    {
        isOpen = !isOpen;
        DoorOpen.SetActive(!isOpen);
        DoorClosed.SetActive(isOpen);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DoorOpen.SetActive(false);
        DoorClosed.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
