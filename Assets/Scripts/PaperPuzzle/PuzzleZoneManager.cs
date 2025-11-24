using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class PuzzleZoneManager : NetworkBehaviour
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
            var z = zones[i];
            if (z.currentPiece == null) return;

            // selama belum solved, unlock piece (prevent stuck)
            z.currentPiece.isLocked = false;

            if (z.currentPiece.pieceId != correctOrder[i])
                return;
        }

        // Puzzle solved
        if (IsServer)
        {
            LockAllPieces();
            PuzzleNetworkManager.Instance.PuzzleSolvedServerRpc();
        }
    }

    void LockAllPieces()
    {
        foreach (var z in zones)
        {
            var p = z.currentPiece;
            if (p != null)
            {
                p.SetLockedClientRpc(true);
            }
        }
    }

}
