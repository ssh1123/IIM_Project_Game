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

    [Header("Answer Triggers")]
    [SerializeField] private AnswerTrigger[] answerTriggers;

    [Header("Player")]
    [SerializeField] private RunnerPlayerController playerController;

    [Header("Data")]
    [SerializeField] private QuestionDatabase questionDatabase;
    [SerializeField] private float baseJudgeX = 10f;
    [SerializeField] private float judgeStepX = 8f; // 每次往前移多少

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
        if (questionDatabase == null || questionDatabase.questions.Count == 0)
        {
            questionPanel.SetActive(true);
            questionText.text = "沒有題目資料";
            feedbackText.text = "";
            waitingAnswer = false;
            return;
        }

        if (currentQuestionIndex < 0 || currentQuestionIndex >= questionDatabase.questions.Count)
        {
            questionPanel.SetActive(false);
            feedbackText.text = "";
            waitingAnswer = false;

            onAllQuestionsFinished?.Invoke();   // ★ 用 null 條件運算子簡化

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
        // ★ 不在這裡 Reset Trigger，統一交給 PositionAndResetAnswerTriggers
    }

    public void OnPlayerChooseLane(LaneType selectedLane)
    {
        if (!waitingAnswer || isTransitioning) return;

        if (currentQuestionIndex < 0 || currentQuestionIndex >= questionDatabase.questions.Count)
            return;

        var q = questionDatabase.questions[currentQuestionIndex];

        if (selectedLane == q.correctLane)
        {
            // 答對：顯示文字 + 停止移動 + 準備進下一題
            feedbackText.text = "答對了！";
            waitingAnswer = false;
            isTransitioning = true;

            playerController.SetCanMove(false);

            // 這裡是否要立刻往前推判定區，視你設計而定
            PositionAndResetAnswerTriggers();

            StartCoroutine(GoNextQuestion());
        }
        else
        {
            // 答錯：只顯示提示，不停止移動
            feedbackText.text = "答錯了，再試一次。";

            // 目前設計：答錯也往前推判定區，讓玩家在下一個判定區再試
            PositionAndResetAnswerTriggers();
        }
    }

    private IEnumerator GoNextQuestion()
    {
        yield return new WaitForSeconds(0.8f);

        currentQuestionIndex++;
        ShowCurrentQuestion();
    }
}