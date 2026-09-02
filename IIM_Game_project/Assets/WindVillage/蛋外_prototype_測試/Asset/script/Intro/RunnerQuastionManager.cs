using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RunnerQuestionManager : MonoBehaviour
{
    [Header("Question UI")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text topText;
    [SerializeField] private TMP_Text middleText;
    [SerializeField] private TMP_Text bottomText;
    [SerializeField] private TMP_Text progressText;

    [Header("Feedback UI")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text explanationText;
    [SerializeField] private TMP_Text continueHintText;

    [Header("Optional Feedback Images")]
    [SerializeField] private Image feedbackBackgroundImage;
    [SerializeField] private Sprite correctFeedbackSprite;
    [SerializeField] private Sprite wrongFeedbackSprite;

    [Header("Answer Triggers")]
    [SerializeField] private AnswerTrigger[] answerTriggers;

    [Header("Player")]
    [SerializeField] private RunnerPlayerController playerController;
    [SerializeField] private RunnerIntroManager runnerIntroManager;

    [Header("Data")]
    [SerializeField] private QuestionDatabase questionDatabase;
    [SerializeField] private float baseJudgeX = 20f;
    [SerializeField] private float judgeStepX = 20f;

    private int currentQuestionIndex = 0;
    private int judgeZoneIndex = 0;

    private bool waitingAnswer = false;
    private bool waitingForContinue = false;
    private bool lastAnswerWasCorrect = false;

    public System.Action onAllQuestionsFinished;

    private void Update()
    {
        // 只有顯示回饋對話框時，Space 才有作用
        if (!waitingForContinue)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ContinueAfterFeedback();
        }
    }

    public void StartFirstQuestion()
    {
        currentQuestionIndex = 0;
        judgeZoneIndex = 0;

        waitingAnswer = false;
        waitingForContinue = false;

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        PositionAndResetAnswerTriggers();
        ShowCurrentQuestion();
    }

    private void PositionAndResetAnswerTriggers()
    {
        if (answerTriggers == null || answerTriggers.Length == 0)
            return;

        float judgeX = baseJudgeX + judgeStepX * judgeZoneIndex;

        foreach (var trigger in answerTriggers)
        {
            if (trigger == null) continue;

            Vector3 p = trigger.transform.position;
            p.x = judgeX;
            trigger.transform.position = p;

            trigger.ResetTrigger();
        }

        judgeZoneIndex++;
    }

    private void ShowCurrentQuestion()
    {
        if (questionDatabase == null ||
            questionDatabase.questions == null ||
            questionDatabase.questions.Count == 0)
        {
            if (questionPanel != null)
            {
                questionPanel.SetActive(true);
            }

            questionText.text = "沒有題目資料";
            waitingAnswer = false;
            return;
        }

        if (currentQuestionIndex >= questionDatabase.questions.Count)
        {
            if (questionPanel != null)
            {
                questionPanel.SetActive(false);
            }

            if (feedbackPanel != null)
            {
                feedbackPanel.SetActive(false);
            }

            waitingAnswer = false;
            waitingForContinue = false;

            Debug.Log("所有題目完成，準備呼叫 onAllQuestionsFinished。", this);

            onAllQuestionsFinished?.Invoke();
            return;
        }

        QuestionData q = questionDatabase.questions[currentQuestionIndex];

        if (questionPanel != null)
        {
            questionPanel.SetActive(true);
        }

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        questionText.text = q.questionText;
        topText.text = q.topText;
        middleText.text = q.middleText;
        bottomText.text = q.bottomText;

        if (progressText != null)
        {
            int displayIndex = currentQuestionIndex + 1;
            int totalQuestionCount = questionDatabase.questions.Count;

            progressText.text =
                $"第 {displayIndex} 題   共 {totalQuestionCount} 題";
        }

        waitingAnswer = true;
        waitingForContinue = false;

        if (playerController != null)
        {
            playerController.SetCanMove(true);
        }
    }

    public void OnPlayerChooseLane(LaneType selectedLane)
    {
        if (!waitingAnswer || waitingForContinue)
            return;

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= questionDatabase.questions.Count)
            return;

        QuestionData q = questionDatabase.questions[currentQuestionIndex];

        waitingAnswer = false;
        lastAnswerWasCorrect = selectedLane == q.correctLane;

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        ShowFeedback(q);
    }

    private void ShowFeedback(QuestionData q)
    {
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
        }

        if (lastAnswerWasCorrect)
        {
            feedbackText.text = "答對了！";

            if (explanationText != null)
            {
                explanationText.text = q.correctExplanation;
            }
            
            


            if (feedbackBackgroundImage != null &&
                correctFeedbackSprite != null)
            {
                feedbackBackgroundImage.sprite = correctFeedbackSprite;
            }

            runnerIntroManager?.AddCorrectAnswer();
        }
        else
        {
            feedbackText.text = "答錯了，再想想看！";

            if (explanationText != null)
            {
                explanationText.text = q.wrongExplanation;
            }

            if (feedbackBackgroundImage != null &&
                wrongFeedbackSprite != null)
            {
                feedbackBackgroundImage.sprite = wrongFeedbackSprite;
            }
        }
        
        if (!GameState.Instance.IsFeedbackEnabled)
        {
            explanationText.gameObject.SetActive(false);
        }

        if (continueHintText != null)
        {
            continueHintText.text = "按下 Space 繼續";
        }

        waitingForContinue = true;
    }

    private void ContinueAfterFeedback()
    {
        if (!waitingForContinue)
            return;

        waitingForContinue = false;

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        if (lastAnswerWasCorrect)
        {
            // 答對：進入下一題
            currentQuestionIndex++;

            // 若還有下一題，將 Trigger 搬到下一個判定位置
            if (currentQuestionIndex < questionDatabase.questions.Count)
            {
                PositionAndResetAnswerTriggers();
            }

            ShowCurrentQuestion();
        }
        else
        {
            // 答錯：題目不換，但判定區往前移，
            // 讓 Player 繼續向右後，在下一區重新選答案
            PositionAndResetAnswerTriggers();

            ShowCurrentQuestion();
        }
    }

    public int GetQuestionCount()
    {
        if (questionDatabase == null || questionDatabase.questions == null)
            return 0;

        return questionDatabase.questions.Count;
    }
}