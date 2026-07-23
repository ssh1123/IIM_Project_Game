using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AIChatDrawer : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Toggle Button")]
    [SerializeField] private Button toggleButton;

    [Header("Positions")]
    [SerializeField] private Vector2 openedPosition = new Vector2(0f, 0f);
    [SerializeField] private Vector2 closedPosition = new Vector2(360f, 0f);

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.25f;

    [Header("State")]
    [SerializeField] private bool startOpened = false;

    private Coroutine currentRoutine;
    private bool isOpen;

    private void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(TogglePanel);
        }

        isOpen = startOpened;
        ApplyStateInstant(isOpen);
    }

    public void TogglePanel()
    {
        SetPanelState(!isOpen);
    }

    public void OpenPanel()
    {
        SetPanelState(true);
    }

    public void ClosePanel()
    {
        SetPanelState(false);
    }

    public void SetPanelState(bool open)
    {
        isOpen = open;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(AnimatePanel(open));
    }

    private void ApplyStateInstant(bool open)
    {
        if (panelRect != null)
        {
            panelRect.anchoredPosition = open ? openedPosition : closedPosition;
        }

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = open;
            panelCanvasGroup.blocksRaycasts = open;
        }
    }

    private IEnumerator AnimatePanel(bool open)
    {
        if (panelRect == null)
            yield break;

        Vector2 startPos = panelRect.anchoredPosition;
        Vector2 targetPos = open ? openedPosition : closedPosition;

        if (panelCanvasGroup != null && open)
        {
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animationDuration);
            t = EaseOutCubic(t);

            panelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        panelRect.anchoredPosition = targetPos;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = open;
            panelCanvasGroup.blocksRaycasts = open;
        }

        currentRoutine = null;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}