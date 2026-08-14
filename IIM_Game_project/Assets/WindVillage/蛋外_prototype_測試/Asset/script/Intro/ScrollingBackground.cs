using UnityEngine;

public class BackgroundLoopByPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform[] backgroundPieces;

    [Header("Background Size")]
    [SerializeField] private float pieceWidth = 20f;

    private void Update()
    {
        if (targetCamera == null || backgroundPieces == null)
            return;

        // Orthographic Camera 左側可見邊界
        float cameraHalfWidth =
            targetCamera.orthographicSize * targetCamera.aspect;

        float cameraLeftEdge =
            targetCamera.transform.position.x - cameraHalfWidth;

        foreach (Transform piece in backgroundPieces)
        {
            if (piece == null) continue;

            // 背景的右邊界已完全離開 Camera 左側
            float pieceRightEdge =
                piece.position.x + pieceWidth * 0.5f;

            if (pieceRightEdge < cameraLeftEdge)
            {
                MovePieceToRightmost(piece);
            }
        }
    }

    private void MovePieceToRightmost(Transform pieceToMove)
    {
        float rightmostX = float.MinValue;

        foreach (Transform piece in backgroundPieces)
        {
            if (piece == null || piece == pieceToMove)
                continue;

            if (piece.position.x > rightmostX)
            {
                rightmostX = piece.position.x;
            }
        }

        Vector3 newPosition = pieceToMove.position;
        newPosition.x = rightmostX + pieceWidth;
        pieceToMove.position = newPosition;
    }
}