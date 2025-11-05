using UnityEngine;

public interface IObject
{
    void Interact(Transform player);
    bool CanInteract();
}
