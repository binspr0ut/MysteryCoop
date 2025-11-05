using System;
using UnityEngine;

public class Radio : MonoBehaviour, IObject, IStateObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }
    public GameObject ControlUI;
    public GameObject RadioUI;

    [Header("Components")]
    [SerializeField] private Collider2D interactionCollider;

    private ObjectState currentState = ObjectState.Disabled;


    public void SetObjectState(ObjectState state)
    {
        currentState = state;

        switch (state)
        {
            case ObjectState.Disabled:
                interactionCollider.enabled = false;
                break;

            case ObjectState.Locked:
                interactionCollider.enabled = true;
                break;

            case ObjectState.Active:
                interactionCollider.enabled = true;
                break;
        }
    }

    public bool CanInteract() => true;

    public void Interact(Transform player)
    {
        ControlUI.SetActive(false);
        RadioUI.SetActive(true);
        IsInteracted = true;
    }

    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);
        RadioUI.SetActive(false);
        IsInteracted = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ID ??= GlobalHelper.GenerateUniqueID(gameObject);

        if (RadioUI != null)
            RadioUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
