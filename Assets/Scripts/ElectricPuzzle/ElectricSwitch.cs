using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElectricSwitch : MonoBehaviour, IPointerClickHandler
{
    [Header("Switch Visual")]
    public Image switchImage;   // Image UI untuk saklar
    public Sprite spriteOn;     // IMG_0108_1
    public Sprite spriteOff;    // IMG_0108_2

    [Header("Data")]
    public WireColor color;
    public ElectricPuzzle puzzle;

    // state lokal, diset tiap kali puzzle mengubah switch
    private bool isOn;

    /// <summary>
    /// Dipanggil ketika player tap switch.
    /// </summary>
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
            {
                puzzle.ToggleSwitch(color);
                // sinkronkan visual dengan state di puzzle (lihat helper di ElectricPuzzle)
                if (puzzle != null)
                {
                    bool newState = puzzle.IsSwitchOn(color);
                    ApplyState(newState);
                }
            }
        }
    }

    /// <summary>
    /// Dipanggil dari network (atau fallback) untuk menerapkan ON/OFF
    /// lalu mengganti sprite.
    /// </summary>
    public void ApplyState(bool on)
    {
        isOn = on;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (switchImage == null) return;

        if (isOn)
        {
            if (spriteOn != null)
                switchImage.sprite = spriteOn;
        }
        else
        {
            if (spriteOff != null)
                switchImage.sprite = spriteOff;
        }
    }
}