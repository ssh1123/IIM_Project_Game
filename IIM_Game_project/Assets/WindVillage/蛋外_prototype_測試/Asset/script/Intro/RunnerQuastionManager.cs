using UnityEngine;
using TMPro;
using System.Collections;

public class RunnerQuestionManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text topText;
    [SerializeField] private TMP_Text middleText;
    [SerializeField] private TMP_Text bottomText;
    [Header("Player")]
    [SerializeField] private RunnerPlayerController playerController;

    [Header("Data")]
    [SerializeField] private QuestionDatabase questionDatabase;

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
        if (questionDatabase == null || questionDatabase.questions.Count == 0)
        {
            questionText.text = "沒有題目資料";
            feedbackText.text = "";
            waitingAnswer = false;
            return;
        }

        if (currentQuestionIndex >= questionDatabase.questions.Count)
        {
            questionText.text = "題目結束";
            feedbackText.text = "";
            waitingAnswer = false;
            return;
        }
        var q = questionDatabase.questions[currentQuestionIndex];

        questionPanel.SetActive(true);
        questionText.text = q.questionText;
        topText.text = q.topText;
        middleText.text = q.middleText;
        bottomText.text = q.bottomText;
        feedbackText.text = "";
        waitingAnswer = true;
        isTransitioning = false;
        playerController.SetCanMove(true);
    }

    public void OnPlayerChooseLane(LaneType selectedLane)
    {
        if (!waitingAnswer || isTransitioning) return;

        var q = questionDatabase.questions[currentQuestionIndex];

        if (selectedLane == q.correctLane)
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