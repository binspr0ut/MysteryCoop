using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : NetworkBehaviour
{
    [Header("References")]
    public RectTransform inventoryPanel;   // drag Inventory Bar
    public Canvas mainCanvas;              // drag canvas tempat inventory berada
    public GameObject ControlUI;

    [Header("Item")]
    public GameObject noteButton;
    public GameObject battery1Button;
    public GameObject battery2Button;


    [Header("Battery")]
    public GameObject Battery1;
    public GameObject Battery2;

    [Header("Note")]
    public GameObject NotePanel;
    public GameObject Note;
    public Sprite DetectiveNote;
    public Sprite SpiritNote;

    [Header("Animation Settings")]
    public float slideDuration = 0.35f;
    public float hiddenY = 300f;           // jarak panel saat disembunyikan
                                           // (sesuai layout HP kamu)


    private bool isOpen = true;
    private bool freezeClose = false;

    private Vector2 shownPos;
    private Vector2 hiddenPos;

    public static InventoryController Instance;

    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        bool isDetective = IsHost;  // Host = Detective, Client = Spirit

        ApplyNoteTexture(isDetective);
    }

    private void ApplyNoteTexture(bool isDetective)
    {
        if (isDetective)
        {
            Note.GetComponent<Image>().sprite = DetectiveNote;

        }
        else
        {
            Note.GetComponent<Image>().sprite = SpiritNote;
        }
    }

    void Start()
    {
        // posisi original panel (yang seharusnya terlihat)
        Debug.Log("shownPos: " + shownPos);
        shownPos = inventoryPanel.anchoredPosition;

        // posisi tersembunyi (di atas layar)
        Debug.Log("hiddenPos: " + shownPos);
        hiddenPos = shownPos + new Vector2(0, hiddenY);

        // mulai dalam keadaan tertutup
        inventoryPanel.anchoredPosition = hiddenPos;

        noteButton.SetActive(false);
        battery1Button.SetActive(false);
        battery2Button.SetActive(false);

    }

    public void ToggleInventory()
    {
        Debug.Log("IsOpen: " + isOpen);
        if (isOpen)
            ShowInventory();
        else
            HideInventory();
    }

    public void GetNote()
    {
        noteButton.SetActive(true);
    }

    public void GetBattery1()
    {
        battery1Button.SetActive(true);
    }

    public void GetBattery2()
    {
        battery2Button.SetActive(true);
    }

    public void ShowBattery1()
    {
        ClockBack.Instance.ShowBattery(Battery1);
        HideBattery1();
    }

    public void ShowBattery2()
    {
        ClockBack.Instance.ShowBattery(Battery2);
        HideBattery2();
    }

    public void ShowNotePanel()
    {
        HideInventory();
        NotePanel.SetActive(true);
        ControlUI.SetActive(false);
    }

    public void HideNotePanel()
    {
        NotePanel.SetActive(false);
        ControlUI.SetActive(true);
    }

    public void HideBattery1()
    {
        battery1Button.SetActive(false);
    }

    public void HideBattery2()
    {
        battery2Button.SetActive(false);
    }

    public void ShowInventoryUI()
    {
        isOpen = true;

        ShowInventory();
    }

    public void ShowInventoryFreeze()
    {
        isOpen = true;
        freezeClose = true;
        ShowInventory();
    }

    // -----------------------------------------------------
    // ANIMATIONS
    // -----------------------------------------------------
    public void ShowInventory()
    {
        LeanTween.cancel(inventoryPanel);
        LeanTween.moveY(inventoryPanel, shownPos.y, slideDuration).setEaseOutCubic();
        isOpen = !isOpen;

    }

    public void HideInventory()
    {
        LeanTween.cancel(inventoryPanel);
        LeanTween.moveY(inventoryPanel, hiddenPos.y, slideDuration).setEaseInCubic();
        freezeClose = false;
        Debug.Log("freezeClose" + freezeClose);
        isOpen = !isOpen;

    }

    // -----------------------------------------------------
    // HIDE WHEN CLICK OUTSIDE PANEL
    // -----------------------------------------------------
    void Update()
    {
        if (!isOpen) return;

        if (Input.GetMouseButtonDown(0))
        {
            // cek apakah klik mengenai UI inventory
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                    inventoryPanel,
                    Input.mousePosition,
                    mainCanvas.worldCamera))
            {
                isOpen = false;
                HideInventory();
            }
        }
    }
}
