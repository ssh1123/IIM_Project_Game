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
        if (gameStartPanel != null)
            gameStartPanel.SetActive(false);

        if (gameEndPanel != null)
            gameEndPanel.SetActive(false);
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

        if (gameEndPanel != null)
            gameEndPanel.SetActive(true);
    }
}