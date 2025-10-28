using UnityEngine;
using Unity.Cinemachine;

public class DynamicConfinerBinder : MonoBehaviour
{
    [SerializeField] private string confinerName = "Confiner"; // bisa diubah di inspector

    private CinemachineConfiner2D confiner;

    void Awake()
    {
        confiner = GetComponent<CinemachineConfiner2D>();
    }

    public void BindConfiner()
    {
        // Cari berdasarkan nama GameObject
        GameObject confinerObj = GameObject.Find(confinerName);

        if (confinerObj != null)
        {
            var poly = confinerObj.GetComponent<PolygonCollider2D>();
            if (poly != null)
            {
                confiner.BoundingShape2D = poly;
                confiner.InvalidateBoundingShapeCache();
                Debug.Log("✅ Confiner assigned to: " + confinerObj.name);
            }
            else
            {
                Debug.LogWarning("❌ Confiner object found but no PolygonCollider2D!");
            }
        }
        else
        {
            Debug.LogWarning("❌ Confiner object not found in Scene!");
        }
    }
}
