using UnityEngine;

public class ShelfOpened : MonoBehaviour, IObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }
    public GameObject ControlUI;

    public GameObject ShelfUI;
    public GameObject GuestbookUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void Interact(Transform player)
    {
        OpenShelf();
    }

    public bool CanInteract() => true;

    public void OpenGuestbook()
    {
        ControlUI.SetActive(false);
        GuestbookUI.SetActive(true);
        IsInteracted = true;
    }
    public void OpenShelf()
    {
        ControlUI.SetActive(false);
        ShelfUI.SetActive(true);
        IsInteracted = true;
    }

    public void ClosePuzzle()
    {
        ControlUI.SetActive(true);
        ShelfUI.SetActive(false);
        GuestbookUI.SetActive(false);
        IsInteracted = false;
    }

    // Update is called once per frame
    void Update()
    {

    }


}
