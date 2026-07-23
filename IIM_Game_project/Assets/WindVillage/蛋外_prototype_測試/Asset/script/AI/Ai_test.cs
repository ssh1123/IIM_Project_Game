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

    [System.Serializable]
    private class AskRequest
    {
        public string question;
    }

    [System.Serializable]
    private class AskResponse
    {
        public string answer;
    }
    
    
    [SerializeField] private PythonServerLauncher serverLauncher;
    [SerializeField] private GameObject aiPanel;


    private void Start()
    {
        aiPanel.SetActive(false);
        StartCoroutine(WaitForServer());
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnClickSend);
        }

        if (answerText != null)
        {
            answerText.text = "請輸入問題後按送出。";
        }
    }
    private IEnumerator WaitForServer()
    {
        while (!serverLauncher.IsServerReady)
        {
            yield return null;
        }

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

        AskRequest requestData = new AskRequest();
        requestData.question = question;

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                answerText.text = "連線失敗: " + request.error;
                yield break;
            }

            string responseJson = request.downloadHandler.text;
            AskResponse responseData = JsonUtility.FromJson<AskResponse>(responseJson);

            if (responseData != null && !string.IsNullOrEmpty(responseData.answer))
            {
                answerText.text = responseData.answer;
            }
            else
            {
                answerText.text = "有收到回應，但格式不正確。\n" + responseJson;
            }
        }
    }

}