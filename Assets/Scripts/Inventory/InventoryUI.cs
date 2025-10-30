using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DetectiveInventory inventory;
    [SerializeField] private GameObject panel;          // InventoryBar panel
    [SerializeField] private Image[] slotImages;        // isi dgn Icon dari slot2

    [Header("Visual")]
    [SerializeField] private Color emptyColor = new Color(1,1,1,0.15f);

    private void Awake()
    {
        if (!panel) panel = gameObject;
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += Refresh;
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    public void Toggle()
    {
        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf) Refresh();
    }

    public void Refresh()
    {
        if (inventory == null || slotImages == null) return;

        // tampilkan item
        var items = inventory.Items;
        int i = 0;
        for (; i < items.Count && i < slotImages.Length; i++)
        {
            slotImages[i].sprite = items[i].icon;
            slotImages[i].color = Color.white;
            slotImages[i].enabled = true;
        }
        // kosongkan sisa slot
        for (; i < slotImages.Length; i++)
        {
            slotImages[i].sprite = null;
            slotImages[i].color = emptyColor;
            slotImages[i].enabled = true; // tetap tampak sebagai slot kosong
        }
    }
}