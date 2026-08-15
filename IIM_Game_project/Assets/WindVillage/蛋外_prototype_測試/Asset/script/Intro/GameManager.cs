using UnityEngine;

public class RunnerGameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private RunnerQuestionManager questionManager;

    [Header("Optional")]
    [SerializeField] private GameObject gameStartPanel;
    [SerializeField] private GameObject gameEndPanel;

    private bool gameStarted = false;

    private void Start()
    {
        if (questionManager != null)
            questionManager.onAllQuestionsFinished += OnQuestionsFinished;
    }

    public void OnQuestionsFinished()
    {
        Debug.Log("所有題目完成，回到 VN_Main 或下一段流程");
        // 在這裡切回 VN_Main 或呼叫你的 VN 系統
    }

    public void StartGame()
    {
        gameStarted = true;

        if (gameStartPanel != null)
            gameStartPanel.SetActive(false);

        if (questionManager != null)
        {
            questionManager.StartFirstQuestion();
        }
        else
        {
            Debug.LogError("RunnerGameManager: questionManager is not assigned.");
        }
    }

    public void ShowFirstQuestion()
    {
        if (questionManager != null)
        {
            questionManager.StartFirstQuestion();
        }
        else
        {
            Debug.LogError("RunnerGameManager: questionManager is not assigned.");
        }
    }

    public void EndGame()
    {
        gameStarted = false;

        if (gameEndPanel != null && !gameStarted)
            gameEndPanel.SetActive(true);
    }
}