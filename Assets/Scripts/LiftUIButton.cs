using UnityEngine;
using UnityEngine.EventSystems;

public class LiftUIButton : MonoBehaviour, IPointerClickHandler
{
    public System.Action onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Clicked on {gameObject.name}");
        onClick?.Invoke();
    }

}
