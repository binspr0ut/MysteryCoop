using UnityEngine;

public class TouchBlocker : MonoBehaviour
{
    public static TouchBlocker Instance;

    [Header("EventSystem blocker object")]
    public GameObject ObjectEvent;

    public static bool Blocked => Instance != null && Instance.ObjectEvent.activeSelf;

    void Awake()
    {
        Instance = this;
    }

    public static void SetBlocked(bool value)
    {
        if (Instance == null)
        {
            Debug.LogWarning("⚠ InputBlocker instance not found in scene!");
            return;
        }

        Instance.ObjectEvent.SetActive(value);
    }
}
