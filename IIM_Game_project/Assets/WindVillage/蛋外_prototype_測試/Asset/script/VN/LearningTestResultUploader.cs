using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class LearningTestResultUploader : MonoBehaviour
{
    [Serializable]
    public class TestAnswerData
    {
        public string question_group;
        public string question_id;
        public int question_order;
        public int overall_question_order;
        public int attempt_number;

        public string selected_answer;
        public bool is_correct;

        public string question_started_at;
        public string answered_at;
        public float answer_seconds;
        public bool used_ai_hint;
    }

    [Serializable]
    public class LearningTestResultData
    {
        public string session_id;
        public long learning_game_result_id;

        public float test_seconds;
        public int total_question_count;
        public int correct_count;
        public int wrong_count;

        public bool is_completed;
        public string finished_at;

        public List<TestAnswerData> answers;
    }

    [Header("FastAPI")]
    [SerializeField]
    private string apiUrl =
        "http://127.0.0.1:8000/learning-test-results";

    private readonly List<TestAnswerData> answers =
        new List<TestAnswerData>();

    private string currentSessionId;
    private DateTimeOffset testStartedAt;
    private bool isUploading;
    

    private void Awake()
    {
        Debug.Log(
            "LearningTestResultUploader 已建立："
            + gameObject.name,
            this
        );
    }
    public void StartNewTestSession()
    {
        currentSessionId = Guid.NewGuid().ToString();
        testStartedAt = DateTimeOffset.Now;

        answers.Clear();
        isUploading = false;

        Debug.Log(
            $"第二部分測驗 Session 已建立：{currentSessionId}"
        );
    }

    public void RecordTestAnswer(
        string questionGroup,
        string questionId,
        int questionOrder,
        int overallQuestionOrder,
        string selectedAnswer,
        bool isCorrect,
        DateTimeOffset questionStartedAt,
        bool usedAiHint)
    {
        if (string.IsNullOrWhiteSpace(currentSessionId))
        {
            Debug.LogWarning(
                "第二部分尚未開始 Session，將自動建立 Session。"
            );

            StartNewTestSession();
        }

        DateTimeOffset answeredAt = DateTimeOffset.Now;

        float answerSeconds = Mathf.Max(
            0f,
            (float)(answeredAt - questionStartedAt).TotalSeconds
        );

        TestAnswerData answer = new TestAnswerData
        {
            question_group = questionGroup,
            question_id = questionId,
            question_order = questionOrder,
            overall_question_order = overallQuestionOrder,
            attempt_number = 1,

            selected_answer = selectedAnswer,
            is_correct = isCorrect,

            question_started_at = questionStartedAt.ToString("o"),
            answered_at = answeredAt.ToString("o"),
            answer_seconds = answerSeconds,
            used_ai_hint = usedAiHint
        };

        answers.Add(answer);

        Debug.Log(
            $"第二部分答題已暫存："
            + $"題組={questionGroup}，"
            + $"題目={questionId}，"
            + $"組內第 {questionOrder} 題，"
            + $"全域第 {overallQuestionOrder} 題，"
            + $"答對={isCorrect}，"
            + $"耗時={answerSeconds:F2} 秒"
        );
    }

    public void UploadTestResult(bool isCompleted)
    {
        LearningGameResultUploader gameUploader =
    FindFirstObjectByType<LearningGameResultUploader>();

        long learningGameResultId = 0;

        if (gameUploader != null &&
            gameUploader.LatestLearningGameResultId > 0)
        {
            learningGameResultId =
                gameUploader.LatestLearningGameResultId;

            Debug.Log(
                "第二部分將使用第一部分的 game_result_id："
                + learningGameResultId
            );
        }
        else
        {
            Debug.LogWarning(
                "尚未取得第一部分 game_result_id；"
                + "learning_game_result_id 將傳送 null。"
            );
        }
        if (isUploading)
        {
            Debug.LogWarning("第二部分資料正在上傳，忽略重複上傳。");
            return;
        }

        if (string.IsNullOrWhiteSpace(currentSessionId))
        {
            Debug.LogError("尚未開始第二部分測驗，無法上傳。");
            return;
        }

        DateTimeOffset finishedAt = DateTimeOffset.Now;

        int correctCount = 0;
        int wrongCount = 0;

        foreach (TestAnswerData answer in answers)
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

        float testSeconds = Mathf.Max(
            0f,
            (float)(finishedAt - testStartedAt).TotalSeconds
        );

        LearningTestResultData payload =
            new LearningTestResultData
            {
                session_id = currentSessionId,
                learning_game_result_id = learningGameResultId,

                test_seconds = testSeconds,
                total_question_count = answers.Count,
                correct_count = correctCount,
                wrong_count = wrongCount,

                is_completed = isCompleted,
                finished_at = finishedAt.ToString("o"),

                answers = new List<TestAnswerData>(answers)
            };

        isUploading = true;
        StartCoroutine(PostTestResult(payload));
    }

    private IEnumerator PostTestResult(LearningTestResultData payload)
    {
        string json = JsonUtility.ToJson(payload);

        Debug.Log($"準備 POST 第二部分測驗資料至：{apiUrl}");
        Debug.Log($"第二部分測驗 JSON：\n{json}");

        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        using UnityWebRequest request =
            new UnityWebRequest(apiUrl, UnityWebRequest.kHttpVerbPOST);

        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 15;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(
                "第二部分測驗資料上傳成功：\n"
                + request.downloadHandler.text
            );
        }
        else
        {
            Debug.LogError(
                "第二部分測驗資料上傳失敗。\n"
                + $"HTTP 狀態碼：{request.responseCode}\n"
                + $"Unity 錯誤：{request.error}\n"
                + $"伺服器回應：{request.downloadHandler.text}"
            );

            isUploading = false;
        }
    }
}