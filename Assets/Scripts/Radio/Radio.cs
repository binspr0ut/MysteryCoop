using System;
using UnityEngine;

public class Radio : MonoBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }
    public GameObject ControlUI;
    public GameObject RadioUI;

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
