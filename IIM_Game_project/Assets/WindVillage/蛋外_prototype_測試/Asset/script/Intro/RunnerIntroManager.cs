using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class RunnerIntroManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunnerQuestionManager questionManager;
    [SerializeField] private RunnerPlayerController playerController;

    [Header("Game State")]
    [SerializeField] private bool startGameOnStart = true;
 
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "VN_Main";
    [SerializeField] private float returnToVNDelay = 2f;

    private int correctAnswerCount = 0;
    private bool isGameFinished = false;
    private void Start()
    {
        if (questionManager == null)
        {
            Debug.LogError("RunnerIntroManager：questionManager 沒有指定。", this);
            return;
        }

        questionManager.onAllQuestionsFinished += FinishRunnerIntro;

        Debug.Log("RunnerIntroManager：已訂閱 onAllQuestionsFinished。", this);

        StartRunnerIntro();
    }
    /* void Start()
    {
        if (questionManager == null)
        {
            Debug.LogError("RunnerIntroManager：尚未指定 RunnerQuestionManager。");
            return;
        }

        // 訂閱：所有題目結束時，RunnerQuestionManager 會通知這裡
        questionManager.onAllQuestionsFinished += FinishRunnerIntro;

        if (startGameOnStart)
        {
            StartRunnerIntro();
        }
    }*/

    private void OnDestroy()
    {
        // 避免物件銷毀後，事件還保留舊的呼叫對象
        if (questionManager != null)
        {
            questionManager.onAllQuestionsFinished -= FinishRunnerIntro;
        }
    }

    public void StartRunnerIntro()
    {
        correctAnswerCount = 0;
        isGameFinished = false;

        //questionManager.StartFirstQuestion();
    }

    public void AddCorrectAnswer()
    {
        correctAnswerCount++;
    }
    private void FinishRunnerIntro()
    {
        if (isGameFinished) return;

        isGameFinished = true;

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        int totalQuestions = questionManager.GetQuestionCount();

        if (GameState.Instance != null)
        {
            GameState.Instance.SaveRunnerIntroResult(
                correctAnswerCount,
                totalQuestions
            );

            // 可選：讓你原本 VN 的旗標系統也能讀取
            GameState.Instance.SetFlag("RunnerIntroFinished");

            if (GameState.Instance.runnerIntroPassed)
            {
                GameState.Instance.SetFlag("RunnerIntroPassed");
            }
        }
        else
        {
            Debug.LogError("找不到 GameState，無法保存 Runner 結果。", this);
        }

        Debug.Log(
            $"跑酷前導完成。答對題數：{correctAnswerCount}/{totalQuestions}",
            this
        );

        StartCoroutine(ReturnToVNMain());
    }

    private IEnumerator ReturnToVNMain()
    {
        // 使用 RealTime：即使之後你把 Time.timeScale 設為 0，
        // 也仍然能在等待後順利切換場景。
        yield return new WaitForSecondsRealtime(returnToVNDelay);

        SceneManager.LoadScene(nextSceneName);
    }

}