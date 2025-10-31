// using UnityEngine;
// using UnityEngine.EventSystems; // IPointerClickHandler
// using System.Linq;

// [RequireComponent(typeof(Collider2D))]
// public class CollectibleItem : MonoBehaviour, IPointerClickHandler
// {
//     [Header("Item")]
//     public string itemID = "item_id";
//     public string displayName = "Item";
//     public Sprite icon; // boleh kosong, fallback dari SpriteRenderer

//     [Header("Pickup Rule")]
//     public bool requireProximity = true;
//     public float pickupRadius = 2.0f;      // jarak max dari detektif
//     public string detectiveTag = "Detective";

//     [Header("After Pickup")]
//     public bool destroyOnPickup = true;

//     // Dipanggil saat user klik/tap object ini (butuh Physics2DRaycaster di Camera)
//     public void OnPointerClick(PointerEventData eventData)
//     {
//         // cari detektif
//         var detectives = GameObject.FindGameObjectsWithTag(detectiveTag);
//         if (detectives == null || detectives.Length == 0) return;

//         // ambil yang terdekat
//         GameObject nearest = detectives
//             .OrderBy(d => Vector2.Distance(d.transform.position, transform.position))
//             .First();

//         if (requireProximity)
//         {
//             float dist = Vector2.Distance(nearest.transform.position, transform.position);
//             if (dist > pickupRadius) return; // terlalu jauh, abaikan
//         }

//         var inv = nearest.GetComponent<DetectiveInventory>();
//         if (!inv) return;

//         if (icon == null)
//         {
//             var sr = GetComponent<SpriteRenderer>();
//             if (sr) icon = sr.sprite;
//         }

//         ItemData data = new ItemData { id = itemID, displayName = displayName, icon = icon };

//         bool ok = inv.AddItem(data);
//         if (ok && destroyOnPickup)
//             Destroy(gameObject);
//         // kalau penuh: nanti kita bisa tampilkan popup "Inventory penuh"
//     }
// }

using UnityEngine;
using UnityEngine.EventSystems; // IPointerClickHandler
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class CollectibleItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Item (ScriptableObject)")]
    public ItemData data;                 // <- drag SO di Inspector
    public Sprite overrideIcon;           // opsional: kalau mau beda ikon di inventory

    [Header("Pickup Rule")]
    public bool requireProximity = true;
    public float pickupRadius = 2f;
    [Tooltip("Tag untuk object pemain yang punya DetectiveInventory")]
    public string detectiveTag = "Detective";

    [Header("After Pickup")]
    public bool destroyOnPickup = true;   // kalau false, bisa SetActive(false)

    // klik/tap (butuh EventSystem + Physics2DRaycaster pada Camera)
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1) cari detektif terdekat
        var players = GameObject.FindGameObjectsWithTag(detectiveTag);
        if (players == null || players.Length == 0) return;

        GameObject nearest = players
            .OrderBy(p => Vector2.Distance(p.transform.position, transform.position))
            .First();

        if (requireProximity)
        {
            float dist = Vector2.Distance(nearest.transform.position, transform.position);
            if (dist > pickupRadius) return; // terlalu jauh
        }

        // 2) ambil inventory dari detektif
        var inv = nearest.GetComponent<DetectiveInventory>();
        if (!inv) return;

        // 3) siapkan data yg akan disimpan (pakai overrideIcon jika diisi)
        var item = data;
        if (item == null)
        {
            Debug.LogWarning("[CollectibleItem] ItemData belum di-assign.");
            return;
        }
        if (overrideIcon != null)
            item.icon = overrideIcon; // sederhana (kalau mau non-destruktif, simpan via ItemInstance)

        // 4) coba masukkan ke inventory
        bool ok = inv.TryAdd(item);
        if (!ok)
        {
            // TODO: tampilkan popup "Inventory penuh"
            return;
        }

        // 5) sukses → hilangkan objek dunia
        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!requireProximity) return;
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, pickupRadius);
        Gizmos.color = new Color(0f, 0.6f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
#endif
}