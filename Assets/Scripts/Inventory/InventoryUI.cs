using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DetectiveInventory inventory;
    [SerializeField] private GameObject panel;     // InventoryBar panel
    [SerializeField] private Image[] slotImages;

    [Header("Visual")]
    [SerializeField] private Color emptyColor = new Color(1,1,1,0.15f);

    // Inspect panel
    [Header("Inspect Panel")]
    [SerializeField] private GameObject itemInspectPanel;
    [SerializeField] private Image inspectArt;
    [SerializeField] private TMP_Text inspectBody;

    private void Awake()
    {
        if (!panel) panel = gameObject;
        panel.SetActive(false);
        if (itemInspectPanel) itemInspectPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (inventory != null) inventory.OnInventoryChanged += Refresh;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.OnInventoryChanged -= Refresh;
    }

    private void Start() => Refresh();

    public void Toggle()
    {
        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf) Refresh();
    }

    public void Refresh()
    {
        if (inventory == null || slotImages == null) return;
        var items = inventory.Items;
        int i = 0;
        for (; i < items.Count && i < slotImages.Length; i++)
        {
            slotImages[i].sprite = items[i].icon;
            slotImages[i].color = Color.white;
            slotImages[i].enabled = true;
        }
        for (; i < slotImages.Length; i++)
        {
            slotImages[i].sprite = null;
            slotImages[i].color = emptyColor;
            slotImages[i].enabled = true;
        }
    }

    // ==== Inspect support ====
    public void OnSlotClicked(int index)
    {
        if (inventory == null) return;
        var items = inventory.Items;
        if (index < 0 || index >= items.Count) return;

        var item = items[index];
        if (item.isInspectable) ShowInspect(item);
        // (future) else: Use/Combine, dsb.
    }

    public void ShowInspect(ItemData item)
    {
        if (!itemInspectPanel) return;
        if (inspectArt) inspectArt.sprite = item.inspectSprite ? item.inspectSprite : item.icon;
        if (inspectBody) inspectBody.text = string.IsNullOrEmpty(item.inspectText) ? item.displayName : item.inspectText;
        itemInspectPanel.SetActive(true);
    }

    public void HideInspect()
    {
        if (itemInspectPanel) itemInspectPanel.SetActive(false);
    }
}