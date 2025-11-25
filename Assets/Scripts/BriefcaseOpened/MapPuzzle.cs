

using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;

public class MapPuzzle : MonoBehaviour
{
    public enum MapLocation { Boarding, DetectiveOffice, TrainStation, GroceryStore }

    [Header("Buttons")]
    public Button boardingBtn;
    public Button detectiveBtn;
    public Button trainBtn;
    public Button groceryBtn;

    [Header("Colors")]
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.green;

    private List<MapLocation> seq = new();
    private bool solved = false;

    [Header("SFX")]
    [SerializeField] private AudioClip mapButtonSFX;


    private void Start()
    {
        boardingBtn.onClick.AddListener(() => Press(MapLocation.Boarding, boardingBtn));
        detectiveBtn.onClick.AddListener(() => Press(MapLocation.DetectiveOffice, detectiveBtn));
        trainBtn.onClick.AddListener(() => Press(MapLocation.TrainStation, trainBtn));
        groceryBtn.onClick.AddListener(() => Press(MapLocation.GroceryStore, groceryBtn));
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }


    private void Press(MapLocation loc, Button btn)
    {
        if (solved) return;
        if (seq.Count >= 3) return;

        PlaySFX(mapButtonSFX);

        seq.Add(loc);
        btn.image.color = Color.green;

        if (seq.Count == 3)
            Validate();
    }

    private void Validate()
    {
        bool correct =
            seq[0] == MapLocation.Boarding &&
            seq[1] == MapLocation.DetectiveOffice &&
            seq[2] == MapLocation.TrainStation;

        if (correct)
        {
            solved = true;
            PuzzleNetworkManager.Instance.SubmitSolvedServerRpc(
                NetworkManager.Singleton.LocalClientId
            );
        }
        else
        {
            ResetPuzzle();
        }
    }

    private void ResetPuzzle()
    {
        seq.Clear();
        solved = false;

        ResetVisual();
    }

    private void ResetVisual()
    {
        SetColor(boardingBtn, defaultColor);
        SetColor(detectiveBtn, defaultColor);
        SetColor(trainBtn, defaultColor);
        SetColor(groceryBtn, defaultColor);
    }

    private void SetColor(Button btn, Color c)
    {
        if (btn != null && btn.image != null)
        {
            btn.image.color = c;
        }
    }
}


