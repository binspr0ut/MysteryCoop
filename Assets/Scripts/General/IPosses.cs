using UnityEngine;

public interface IPossess
{
    void Possess();

    void Interact();
    bool CanPossess();
    void Unpossess();

}
