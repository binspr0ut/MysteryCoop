using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Paper paper;  // drag Paper ke sini lewat inspector

    public void Start()
    {
        if (paper != null)
        {
            paper.ClearCanvas();
        }
        else
        {
            Debug.LogWarning("Paper reference not set on ResetButton!");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (paper != null)
        {
            paper.ClearCanvas();
        }
        else
        {
            Debug.LogWarning("Paper reference not set on ResetButton!");
        }
    }
}
