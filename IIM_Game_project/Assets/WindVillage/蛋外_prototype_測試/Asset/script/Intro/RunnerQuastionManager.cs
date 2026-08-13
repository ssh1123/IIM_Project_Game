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
    [SerializeField] private TMP_Text progressText;      // 顯示「第幾題 / 共幾題」
    [SerializeField] private TMP_Text explanationText;   // 顯示答題後的簡短說明
    [Header("Answer Triggers")]
    [SerializeField] private AnswerTrigger[] answerTriggers;

    [Header("Player")]
    [SerializeField] private RunnerPlayerController playerController;
    [SerializeField] private RunnerIntroManager runnerIntroManager;

    [Header("Data")]
    [SerializeField] private QuestionDatabase questionDatabase;
    [SerializeField] private float baseJudgeX = 20f;
    [SerializeField] private float judgeStepX = 20f; // 每次往前移多少

    private int currentQuestionIndex = 0;
    private bool waitingAnswer = false;
    private bool isTransitioning = false;

    // 代表「目前是第幾個判定區」，不是第幾題
    private int judgeZoneIndex = 0;

    public System.Action onAllQuestionsFinished;

    public void StartFirstQuestion()
    {
        currentQuestionIndex = 0;
        judgeZoneIndex = 0;             // ★ 確保重新開始時判定區從頭算
        PositionAndResetAnswerTriggers();
        ShowCurrentQuestion();
    }

    private void PositionAndResetAnswerTriggers()
    {
        if (answerTriggers == null || answerTriggers.Length == 0)
            return;

        // 計算這一次要使用的判定位置
        float judgeX = baseJudgeX + judgeStepX * judgeZoneIndex;

        foreach (var trigger in answerTriggers)
        {
            if (trigger == null) continue;

            Vector3 p = trigger.transform.position;
            p.x = judgeX;
            trigger.transform.position = p;
            trigger.ResetTrigger();
        }

        // 下一次呼叫時，判定區再往前一格
        judgeZoneIndex += 1;
    }
    private void ShowCurrentQuestion()
    {
        if (explanationText != null)
        {
            explanationText.text = "";
        }
        if (questionDatabase == null || questionDatabase.questions.Count == 0)
        {
            questionPanel.SetActive(true);
            questionText.text = "沒有題目資料";
            feedbackText.text = "";
            progressText.text = "";
            explanationText.text = "";
            waitingAnswer = false;
            return;
        }

        if (currentQuestionIndex < 0 || currentQuestionIndex >= questionDatabase.questions.Count)
        {
            Debug.Log(
                $"所有題目完成：index={currentQuestionIndex}，" +
                $"total={questionDatabase.questions.Count}，準備呼叫事件。",
                this
            );
            questionPanel.SetActive(false);
            feedbackText.text = "結束";
            progressText.text = "";
            explanationText.text = "";
            waitingAnswer = false;

            onAllQuestionsFinished?.Invoke();
            return;
        }

        var q = questionDatabase.questions[currentQuestionIndex];

        questionPanel.SetActive(true);
        questionText.text = q.questionText;
        topText.text = q.topText;
        middleText.text = q.middleText;
        bottomText.text = q.bottomText;
        feedbackText.text = "";

        // ★ 題目進度：「第 x 題 / 共 y 題」
        if (progressText != null)
        {
            int displayIndex = currentQuestionIndex + 1;
            int total = questionDatabase.questions.Count;
            progressText.text = $"第 {displayIndex} 題 / 共 {total} 題";
        }

        // ★ 每題開始時先清空說明文字
        if (explanationText != null)
        {
            explanationText.text = "";
        }

        waitingAnswer = true;
        isTransitioning = false;
        playerController.SetCanMove(true);
    }


    public void OnPlayerChooseLane(LaneType selectedLane)
    {
        if (!waitingAnswer || isTransitioning) return;
        if (currentQuestionIndex < 0 || currentQuestionIndex >= questionDatabase.questions.Count)
            return;

        var q = questionDatabase.questions[currentQuestionIndex];

        if (selectedLane == q.correctLane)
        {
            feedbackText.text = "答對了！";
            runnerIntroManager?.AddCorrectAnswer();

            if (explanationText != null)
            {
                explanationText.text = q.correctExplanation;
            }

            waitingAnswer = false;
            isTransitioning = true;

            playerController.SetCanMove(false);
            PositionAndResetAnswerTriggers();
            StartCoroutine(GoNextQuestion());
        }
        else
        {
            feedbackText.text = "答錯了，再試一次。";

            if (explanationText != null)
            {
                explanationText.text = q.wrongExplanation;
            }

            PositionAndResetAnswerTriggers();
        }
    }

    private IEnumerator GoNextQuestion()
    {
        yield return new WaitForSeconds(0.8f);

        currentQuestionIndex++;
        ShowCurrentQuestion();
    }
    public int GetQuestionCount()
    {
        if (questionDatabase == null || questionDatabase.questions == null)
            return 0;

        return questionDatabase.questions.Count;
    }
}