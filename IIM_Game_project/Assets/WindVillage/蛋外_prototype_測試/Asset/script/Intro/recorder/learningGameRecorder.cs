using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class LearningGameResultUploader : MonoBehaviour
{
    [Serializable]
    public class QuestionAnswerData
    {
        public string question_id;
        public int question_order;
        public int attempt_number;
        public string selected_answer;
        public bool is_correct;

        public string question_started_at;
        public string answered_at;

        public float answer_seconds;
        public bool used_ai_hint;
    }

    [Serializable]
    public class LearningGameResultData
    {
        public string session_id;
        public string player_id;

        public bool ai_assistant;
        public string feedback_quality;

        public bool is_cleared;
        public float play_seconds;

        public int correct_count;
        public int wrong_count;

        public string finished_at;

        public List<QuestionAnswerData> answers;
    }
    [Serializable]
    private class LearningGameResultResponse
    {
        public bool success;
        public string message;

        public long game_result_id;
        public string session_id;

        public int answer_record_count;
    }
    
    [Header("FastAPI")]
    [SerializeField]
    private string apiUrl =
        "http://127.0.0.1:8000/learning-game-results";

    [Header("測試玩家資料")]
    [SerializeField]
    private string playerId = "unity_player_001";

    private readonly List<QuestionAnswerData> answers =
        new List<QuestionAnswerData>();

    private string currentSessionId;
    private DateTimeOffset gameStartedAt;
    //private bool aiAssistantUsed;

    public IReadOnlyList<QuestionAnswerData> Answers => answers;
    public long LatestLearningGameResultId { get; private set; } = -1;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewGameSession()
    {
        currentSessionId = Guid.NewGuid().ToString();
        gameStartedAt = DateTimeOffset.Now;
        
        answers.Clear();

        Debug.Log(
            $"新遊戲紀錄已開始。Session ID：{currentSessionId}"
        );
    }



    public void RecordAnswer(
        string questionId,
        int questionOrder,
        int attemptNumber,
        string selectedAnswer,
        bool isCorrect,
        DateTimeOffset questionStartedAt,
        bool usedAiHint)
    {
        if (string.IsNullOrWhiteSpace(currentSessionId))
        {
            Debug.LogWarning(
                "尚未開始遊戲 Session，將自動建立新的 Session。"
            );

            StartNewGameSession();
        }

        DateTimeOffset answeredAt = DateTimeOffset.Now;

        float answerSeconds = Mathf.Max(
            0f,
            (float)(answeredAt - questionStartedAt).TotalSeconds
        );

        QuestionAnswerData answer = new QuestionAnswerData
        {
            question_id = questionId,
            question_order = questionOrder,
            attempt_number = attemptNumber,
            selected_answer = selectedAnswer,
            is_correct = isCorrect,

            question_started_at = questionStartedAt.ToString("o"),
            answered_at = answeredAt.ToString("o"),

            answer_seconds = answerSeconds,
            used_ai_hint = usedAiHint
        };

        answers.Add(answer);

       

        Debug.Log(
         $"已記錄第 {questionOrder} 題，第 {attemptNumber} 次嘗試："
         + $"{questionId}，答對：{isCorrect}，耗時：{answerSeconds:F2} 秒"
        );
    }

    public void UploadCurrentGameResult(
        bool isCleared,
        string feedbackQuality)
    {
        if (string.IsNullOrWhiteSpace(currentSessionId))
        {
            Debug.LogError(
                "沒有目前的遊戲 Session，請先呼叫 StartNewGameSession()。"
            );
            return;
        }

        feedbackQuality = feedbackQuality.Trim().ToLower();

        if (feedbackQuality != "high" && feedbackQuality != "low")
        {
            Debug.LogError(
                "feedbackQuality 只能是 high 或 low。"
            );
            return;
        }

        int correctCount = 0;
        int wrongCount = 0;

        foreach (QuestionAnswerData answer in answers)
        {
            if (answer.is_correct)
            {
                correctCount++;
            }
            else
            {
                wrongCount++;
            }
        }

        DateTimeOffset finishedAt = DateTimeOffset.Now;

        float playSeconds = Mathf.Max(
            0f,
            (float)(finishedAt - gameStartedAt).TotalSeconds
        );

        LearningGameResultData payload =
            new LearningGameResultData
            {
                session_id = currentSessionId,
                player_id = playerId,

                ai_assistant = GameState.Instance.IsAIEnabled,
                feedback_quality = feedbackQuality,

                is_cleared = isCleared,
                play_seconds = playSeconds,

                correct_count = correctCount,
                wrong_count = wrongCount,

                finished_at = finishedAt.ToString("o"),
                answers = new List<QuestionAnswerData>(answers)
            };

        StartCoroutine(PostGameResult(payload));
    }

    private IEnumerator PostGameResult(
        LearningGameResultData payload)
    {
        string json = JsonUtility.ToJson(payload);

        Debug.Log("準備上傳遊戲紀錄 JSON：\n" + json);

        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        using UnityWebRequest request =
            new UnityWebRequest(apiUrl, UnityWebRequest.kHttpVerbPOST);

        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        request.timeout = 15;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;

            LearningGameResultResponse response =
                JsonUtility.FromJson<LearningGameResultResponse>(
                    responseJson
                );

            if (response != null && response.success)
            {
                LatestLearningGameResultId = response.game_result_id;

                Debug.Log(
                    "第一部分遊戲紀錄上傳成功。\n"
                    + $"已取得 Supabase game_result_id："
                    + $"{LatestLearningGameResultId}\n"
                    + $"完整回應：{responseJson}"
                );
            }
            else
            {
                Debug.LogError(
                    "第一部分上傳雖然 HTTP 成功，但 API 回傳的 JSON "
                    + "不是預期成功格式：\n"
                    + responseJson
                );
            }
        }
        else
        {
            Debug.LogError(
                "遊戲紀錄上傳失敗。\n"
                + $"HTTP 狀態碼：{request.responseCode}\n"
                + $"Unity 錯誤：{request.error}\n"
                + $"伺服器回應：{request.downloadHandler.text}"
            );
        }
    }

    [ContextMenu("測試上傳假資料")]
    private void TestUploadFakeData()
    {
        StartNewGameSession();

        DateTimeOffset q1Start =
            DateTimeOffset.Now.AddSeconds(-30);

        RecordAnswer(
            questionId: "TEST_Q001",
            questionOrder: 1,
            attemptNumber: 1,
            selectedAnswer: "A",
            isCorrect: true,
            questionStartedAt: q1Start,
            usedAiHint: false
        );

        DateTimeOffset q2Start =
            DateTimeOffset.Now.AddSeconds(-10);

        RecordAnswer(
            questionId: "TEST_Q002",
            questionOrder: 2,
            attemptNumber:1,
            selectedAnswer: "C",
            isCorrect: false,
            questionStartedAt: q2Start,
            usedAiHint: true
        );

        UploadCurrentGameResult(
            isCleared: true,
            feedbackQuality: "high"
        );
    }
}