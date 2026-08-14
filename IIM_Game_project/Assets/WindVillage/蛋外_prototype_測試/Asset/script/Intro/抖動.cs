using UnityEngine;

public class UIFloatUpAndDown : MonoBehaviour
{
    [Header("Bobbing Settings")]
    [SerializeField] private float height = 20f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float startOffset = 0f;

    private RectTransform rectTransform;
    private Vector2 startAnchoredPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("UIFloatUpAndDown 必須掛在 Canvas UI 物件上。", this);
            enabled = false;
            return;
        }

        startAnchoredPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(
            (Time.time + startOffset) * speed
        ) * height;

        rectTransform.anchoredPosition = new Vector2(
            startAnchoredPosition.x,
            startAnchoredPosition.y + yOffset
        );
    }
}