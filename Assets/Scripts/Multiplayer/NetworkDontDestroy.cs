using UnityEngine;
[ExecuteInEditMode]
public class NetworkDontDestroy : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
