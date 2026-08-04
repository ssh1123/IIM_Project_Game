using UnityEngine;
using TMPro;
using System.Collections;

public class RunnerQuestionManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Player")]
    [SerializeField] private RunnerPlayerController playerController;

    [Header("Questions")]
    [SerializeField] private string[] questions;
    [SerializeField] private LaneType[] correctLanes;

    private int currentQuestionIndex = 0;
    private bool waitingAnswer = false;
    private bool isTransitioning = false;

    public void StartFirstQuestion()
    {
        currentQuestionIndex = 0;
        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion()
    {
        if (currentQuestionIndex >= questions.Length || currentQuestionIndex >= correctLanes.Length)
        {
            questionText.text = "題目結束";
            feedbackText.text = "";
            waitingAnswer = false;
            return;
        }

        questionPanel.SetActive(true);
        questionText.text = questions[currentQuestionIndex];
        feedbackText.text = "";
        waitingAnswer = true;
        isTransitioning = false;
        playerController.SetCanMove(true);
    }

    public void OnPlayerChooseLane(LaneType selectedLane)
    {
        if (!waitingAnswer || isTransitioning) return;

        if (selectedLane == correctLanes[currentQuestionIndex])
        {
            feedbackText.text = "答對了！";
            waitingAnswer = false;
            isTransitioning = true;
            playerController.SetCanMove(false);
            StartCoroutine(GoNextQuestion());
        }
        else
        {
            feedbackText.text = "答錯了，再試一次。";
        }
    }

    private IEnumerator GoNextQuestion()
    {
        yield return new WaitForSeconds(0.8f);
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }
}