using UnityEngine;
public enum ObjectState
{
    Disabled,   // collider off
    Locked,     // collider on, interact allowed but alternate behavior
    Active      // collider on, full behavior
}

public interface IStateObject
{
    void SetObjectState(ObjectState state);
}
