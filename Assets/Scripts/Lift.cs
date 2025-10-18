using System;
using UnityEngine;

public class Lift : MonoBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("Lift UI References")]
    public GameObject LiftOverlay;   // Canvas overlay lift
    public GameObject ControlUI;     // Tombol interaksi di dunia (ikon E, dll)

    [Header("Player Reference")]
    public Transform player;         // Drag player di inspector

    public bool CanInteract() => true;

    public void Interact()
    {
        ControlUI.SetActive(false);
        LiftOverlay.SetActive(true);
        IsInteracted = true;
    }

    public void CloseLift()
    {
        Debug.Log("CloseLift triggered");
        LiftOverlay?.SetActive(false);
        ControlUI?.SetActive(true);
        IsInteracted = false;
    }

    public void GoUp()
    {
        Debug.Log("GoUp triggered");
        if (player != null)
        {
            Vector3 newPos = player.position;
            newPos.y += 300f;
            player.position = newPos;
        }

        CloseLift();
    }

    private void Start()
    {
        ID ??= GlobalHelper.GenerateUniqueID(gameObject);

        if (LiftOverlay != null)
            LiftOverlay.SetActive(false);
    }
}
