using UnityEngine;
using System.Collections.Generic;

public class PuzzleZoneManager : MonoBehaviour
{
    public static PuzzleZoneManager Instance;

    public List<PuzzleDropZone> zones = new();
    public List<int> correctOrder = new();

    public float snapDistance = 80f;

    void Awake()
    {
        Instance = this;
    }

    public PuzzleDropZone GetClosest(Vector2 pos)
    {
        float best = snapDistance;
        PuzzleDropZone res = null;

        foreach (var z in zones)
        {
            float d = Vector2.Distance(pos, z.rect.anchoredPosition);
            if (d < best)
            {
                best = d;
                res = z;
            }
        }

        return res;
    }

    public void CheckSolved()
    {
        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i].currentPiece == null) return;
            if (zones[i].currentPiece.pieceId != correctOrder[i]) return;
        }

        PuzzleNetworkManager.Instance.PuzzleSolvedServerRpc();
    }
}
