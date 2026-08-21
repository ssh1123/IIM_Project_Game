using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AIChatTester : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField questionInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_Text answerText;

    [Header("API")]
    [SerializeField] private string apiUrl = "http://127.0.0.1:8000/ask";

    [Header("System")]
    [SerializeField] private PythonServerLauncher serverLauncher;
    [SerializeField] private GameObject aiPanel;

    [System.Serializable]
    private class AskRequest
    {
        public string question;
    }

    [System.Serializable]
    private class CitationItem
    {
        public string label;
        public string chunk_id;
        public string document_id;
        public string title;
        public string section;
        public string source_file;
    }

    [System.Serializable]
    private class AskResponse
    {
        public string question;
        public string short_answer;
        public string detailed_answer;
        public string[] citation_labels;
        public CitationItem[] matched_citations;
        public string answer_text;
    }

    private void Start()
    {
        if (aiPanel != null)
            aiPanel.SetActive(false);

        StartCoroutine(WaitForServer());

        if (sendButton != null)
            sendButton.onClick.AddListener(OnClickSend);

        if (answerText != null)
            answerText.text = "請輸入問題後按送出。";
    }

    private IEnumerator WaitForServer()
    {
        while (serverLauncher != null && !serverLauncher.IsServerReady)
            yield return null;

        if (aiPanel != null)
            aiPanel.SetActive(true);
    }

    public void OnClickSend()
    {
        if (questionInput == null || answerText == null)
            return;

        string userQuestion = questionInput.text.Trim();

        if (string.IsNullOrEmpty(userQuestion))
        {
            answerText.text = "請先輸入問題。";
            return;
        }

        StartCoroutine(SendQuestion(userQuestion));
    }

    private IEnumerator SendQuestion(string question)
    {
        answerText.text = "傳送中...";
        if (sendButton != null) sendButton.interactable = false;

        AskRequest requestData = new AskRequest { question = question };
        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (sendButton != null) sendButton.interactable = true;

            if (request.result != UnityWebRequest.Result.Success)
            {
                answerText.text = "連線失敗: " + request.error + "\n" + request.downloadHandler.text;
                yield break;
            }

            string responseJson = request.downloadHandler.text;
            AskResponse responseData = JsonUtility.FromJson<AskResponse>(responseJson);

            if (responseData == null)
            {
                answerText.text = "回應解析失敗。\n" + responseJson;
                yield break;
            }

            if (string.IsNullOrEmpty(responseData.short_answer) && string.IsNullOrEmpty(responseData.detailed_answer))
            {
                answerText.text = "有收到回應，但格式不正確。\n" + responseJson;
                yield break;
            }

            answerText.text = BuildDisplayText(responseData);
        }
    }
    private string BuildDisplayText(AskResponse data)
    {
        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrEmpty(data.short_answer))
        {
            sb.AppendLine("簡短回答：");
            sb.AppendLine(data.short_answer);
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(data.detailed_answer))
        {
            sb.AppendLine("詳細說明：");
            sb.AppendLine(data.detailed_answer);
            sb.AppendLine();
        }

        if (data.matched_citations != null && data.matched_citations.Length > 0)
        {
            sb.AppendLine("引用來源：");
            for (int i = 0; i < data.matched_citations.Length; i++)
            {
                CitationItem item = data.matched_citations[i];
                sb.AppendLine($"- {item.label}｜{item.title}／{item.section}");
            }
        }

        return sb.ToString().Trim();
    }
    public void ResetPanel()
    {
        StopAllCoroutines();

        if (questionInput != null)
        {
            questionInput.SetTextWithoutNotify("");
            questionInput.DeactivateInputField();
        }

        if (answerText != null)
        {
            answerText.text = "請輸入問題後按送出。";
        }

        if (sendButton != null)
        {
            sendButton.interactable = true;
        }

        Debug.Log("AI 問答面板已重設。", this);
    }
}