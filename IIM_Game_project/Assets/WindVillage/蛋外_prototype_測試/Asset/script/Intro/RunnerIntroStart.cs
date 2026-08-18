using UnityEngine;

public class RunnerIntroStartPanel : MonoBehaviour
{
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private RunnerGameManager gameManager;

    private bool hasStarted;

    private void Awake()
    {
        Time.timeScale = 0f;
        Debug.Log("Intro 開啟，Time Scale = " + Time.timeScale);

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
        if (introPanel != null)
        {
            introPanel.SetActive(true);
        }
    }

    private void Update()
    {
        if (hasStarted)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (hasStarted)
            return;

        hasStarted = true;

        if (introPanel != null)
        {
            introPanel.SetActive(false);
        }
        gameManager.ShowFirstQuestion();
        Time.timeScale = 1f;

    }

    private void OnDestroy()
    {
        // 避免離開 RunnerIntro 時，下一個場景仍被暫停。
        Time.timeScale = 1f;
    }
}