using UnityEngine;
using UnityEngine.EventSystems;

public class ElectricSwitch : MonoBehaviour, IPointerClickHandler
{
    public WireColor color;
    public ElectricPuzzle puzzle;

    public void OnPointerClick(PointerEventData eventData)
    {
        // kalau ada network → lewat server
        if (ElectricPuzzleNetwork.Instance != null && ElectricPuzzleNetwork.Instance.IsSpawned)
        {
            ElectricPuzzleNetwork.Instance
                .RequestToggleSwitchServerRpc(color);
        }
        else
        {
            // fallback singleplayer
            if (puzzle != null)
                puzzle.ToggleSwitch(color);
        }
    }
}