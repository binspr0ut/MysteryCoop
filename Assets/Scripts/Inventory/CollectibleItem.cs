using UnityEngine;
using UnityEngine.EventSystems; // IPointerClickHandler
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class CollectibleItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Item")]
    public string itemID = "item_id";
    public string displayName = "Item";
    public Sprite icon; // boleh kosong, fallback dari SpriteRenderer

    [Header("Pickup Rule")]
    public bool requireProximity = true;
    public float pickupRadius = 2.0f;      // jarak max dari detektif
    public string detectiveTag = "Detective";

    [Header("After Pickup")]
    public bool destroyOnPickup = true;

    // Dipanggil saat user klik/tap object ini (butuh Physics2DRaycaster di Camera)
    public void OnPointerClick(PointerEventData eventData)
    {
        // cari detektif
        var detectives = GameObject.FindGameObjectsWithTag(detectiveTag);
        if (detectives == null || detectives.Length == 0) return;

        // ambil yang terdekat
        GameObject nearest = detectives
            .OrderBy(d => Vector2.Distance(d.transform.position, transform.position))
            .First();

        if (requireProximity)
        {
            float dist = Vector2.Distance(nearest.transform.position, transform.position);
            if (dist > pickupRadius) return; // terlalu jauh, abaikan
        }

        var inv = nearest.GetComponent<DetectiveInventory>();
        if (!inv) return;

        if (icon == null)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr) icon = sr.sprite;
        }

        ItemData data = new ItemData { id = itemID, displayName = displayName, icon = icon };

        bool ok = inv.AddItem(data);
        if (ok && destroyOnPickup)
            Destroy(gameObject);
        // kalau penuh: nanti kita bisa tampilkan popup "Inventory penuh"
    }
}
