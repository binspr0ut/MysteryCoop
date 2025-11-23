using UnityEngine;

public class PuzzleDropZone : MonoBehaviour
{
    public int slotIndex;
    public PuzzlePieceDragHandler currentPiece;

    public RectTransform rect => GetComponent<RectTransform>();

    public void PlacePiece(PuzzlePieceDragHandler piece)
    {
        if (currentPiece != null && currentPiece != piece)
        {
            var old = currentPiece;
            currentPiece = piece;

            piece.rect.anchoredPosition = rect.anchoredPosition;
            old.rect.anchoredPosition = old.rect.parent.GetComponent<RectTransform>().anchoredPosition;

            PuzzleZoneManager.Instance.CheckSolved();
            return;
        }

        currentPiece = piece;
        piece.rect.anchoredPosition = rect.anchoredPosition;

        PuzzleZoneManager.Instance.CheckSolved();
    }
}
