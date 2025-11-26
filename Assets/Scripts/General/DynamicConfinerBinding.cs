using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class DynamicConfinerBinder : MonoBehaviour
{
    [SerializeField] private string confinerTag = "Confiner"; // tag Confiner
    private CinemachineConfiner2D confiner;

    void Awake()
    {
        confiner = GetComponent<CinemachineConfiner2D>();

        if (confiner == null)
        {
            Debug.LogError("❌ Tidak menemukan CinemachineConfiner2D di object ini!");
            return;
        }
    }

    void Start()
    {
        // Mulai binding setelah kamera siap (1 frame delay)
        StartCoroutine(BindConfinerDelayed());
    }

    private IEnumerator BindConfinerDelayed()
    {
        // Tunggu 1–2 frame agar scene hierarchy sudah stabilize (Netcode delay fix)
        yield return null;
        yield return null;

        GameObject confinerObj = GameObject.FindWithTag(confinerTag);

        if (confinerObj == null)
        {
            Debug.LogWarning($"❌ Confiner dengan tag '{confinerTag}' TIDAK ditemukan!");
            PrintAllConfinerLikeObjects();
            yield break;
        }

        PolygonCollider2D poly = confinerObj.GetComponent<PolygonCollider2D>();

        if (poly == null)
        {
            Debug.LogWarning($"❌ Object '{confinerObj.name}' ditemukan tapi tidak ada PolygonCollider2D!");
            yield break;
        }

        confiner.BoundingShape2D = poly;
        confiner.InvalidateBoundingShapeCache();

        Debug.Log($"✅ CONFIRMED: Confiner berhasil di-assign → {confinerObj.name}");
    }

    // Debug tambahan untuk bantu user jika nama/tag salah
    private void PrintAllConfinerLikeObjects()
    {
        Debug.Log("🔍 Scanning all objects containing 'Conf' in name...");

        foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.name.ToLower().Contains("conf"))
            {
                Debug.Log("   → Found candidate: " + obj.name +
                          (obj.scene.IsValid() ? " (in scene)" : " (NOT in scene)"));
            }
        }
    }
}
