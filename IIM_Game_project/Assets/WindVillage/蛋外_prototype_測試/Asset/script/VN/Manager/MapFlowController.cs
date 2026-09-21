using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class MapFlowController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject locationPanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject CharactorLayer;
    [SerializeField] private GameObject Reminder;


    [Header("Location UI")]
    [SerializeField] private TMP_Text locationTitleText;
    [SerializeField] private Button enterButton;

    [Header("Reminder")]
    [SerializeField] private TMP_Text reminderText;
    [SerializeField] private Button reminderButton;



    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Locations")]
    [SerializeField] private LocationData[] locations;

    [Header("MapCompelet")]
    [SerializeField] private ImageAndFlag[] imageFlag;
    [Header("Required Flags")]
    [SerializeField] private List<string> requiredFlags = new List<string>();

    [Header("Learning Test Recording")]
    //[SerializeField] private LearningTestResultUploader testResultUploader;
    private LearningTestResultUploader testResultUploader;

    [Header("轉場設定")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private string targetSceneName = "FinalScene";
    [SerializeField] private float delayTime = 2f;
    private LocationData selectedLocation;
    private bool isLoading = false;

    private int reminderIndex = 0;

    private void Start()
    {
        StartCoroutine(WaitForMouseRelease());
        GameState.Instance.SetPreVN(true);

        if (testResultUploader == null)
        {
            testResultUploader =
                FindFirstObjectByType<LearningTestResultUploader>();
        }

        LearningTestResultUploader uploader = GetTestUploader();

        if (uploader != null)
        {
            uploader.StartNewTestSession();
        }
        else
        {
            Debug.LogError(
                "找不到 LearningTestResultUploader。"
                + "請確認它掛在 PersistentControlBoard 下，"
                + "且 PersistentControlBoard 在之前的場景已建立。",
                this
            );
        }


        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        mapPanel.SetActive(true);

        Reminder.SetActive(true);
        reminderButton.interactable = false;
        reminderIndex = 0;
        ShowReminder(reminderIndex);

        locationPanel.SetActive(false);
        dialoguePanel.SetActive(false);
        CharactorLayer.SetActive(false);
        for (int i = 0; i < imageFlag.Length; i++)
        {
            if (!imageFlag[i].isset) { imageFlag[i].imageObject.SetActive(false); }
            
        }
    }
    private void Update()
    {
        for (int i = 0; i < imageFlag.Length; i++)
        {
            if (GameState.Instance.HasFlag(imageFlag[i].flagName) &&
                !imageFlag[i].isset)
            {
                imageFlag[i].imageObject.SetActive(true);
                imageFlag[i].isset = true;
            }
        }
        if(GameState.Instance.HasAllFlags(requiredFlags) && !dialoguePanel.activeInHierarchy)
        {
            if (isLoading) return;

            StartCoroutine(FadeOutAndLoadScene());
        }

    }

    //****Rminder Function****0~3
    public void ShowReminder(int index)
    {
        if(index == 0)
        {
            reminderText.text = "在風待村的冒險中，你的每一次選擇都會影響村子的未來。善用曾經學到的知識與技能，協助村民活用空屋、傳統技藝、共享交通與在地資源。";
        }
        else if(index == 1)
        {
            reminderText.text = "「資金」代表風待村改變所需的經費，可用於修繕老屋、準備材料與維護共享設施。活用在地資源或發展合理收益，能增加村子收益及長期利益。";
        }
        else if (index == 2)
        {
            reminderText.text = "「居民好感度」代表村民對你的信任。尊重居民生活習慣、善用長者技藝，並讓成果公平分享，都能提升好感度。忽略村民感受，大家可能失去信心，使計畫難以推動。";
        }
        else if (index == 3)
        {
            reminderText.text = "「永續度」代表村子的改變能否長久運作，而不是只有一時的熱鬧。選擇兼顧環境、文化與居民生活的方案，能提升永續度，並在結算時帶來更多長期收益。";
            reminderButton.interactable = true;
        }
    }
    public void increaseReminderIndex()
    {
        reminderIndex++;
        if (reminderIndex > 3)
        {
            reminderIndex = 3;
        }
        ShowReminder(reminderIndex);
    }
    public void decreaseReminderIndex()
    {
        reminderIndex--;
        if (reminderIndex < 0)
        {
            reminderIndex = 0;
        }
        ShowReminder(reminderIndex);
    }
    public void CloseReminder()
    {
        Reminder.SetActive(false);
    }
    //**********************

    private LearningTestResultUploader GetTestUploader()
    {
        if (testResultUploader == null)
        {
            testResultUploader =
                FindFirstObjectByType<LearningTestResultUploader>();

            Debug.Log(
                "尋找 LearningTestResultUploader 結果："
                + (testResultUploader != null
                    ? testResultUploader.gameObject.name
                    : "找不到"),
                this
            );
        }

        return testResultUploader;
    }
    private IEnumerator FadeOutAndLoadScene()
    {
        isLoading = true;

        LearningTestResultUploader uploader = GetTestUploader();

        if (uploader != null)
        {
            uploader.UploadTestResult(isCompleted: true);
        }
        else
        {
            Debug.LogError(
                "找不到 LearningTestResultUploader，"
                + "無法上傳第二部分測驗資料。",
                this
            );
        }

        // 判斷所有題組完成後，先等待 1 秒
        yield return new WaitForSecondsRealtime(1f);
        fadeCanvasGroup.blocksRaycasts = true;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / fadeDuration;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        SceneManager.LoadScene(targetSceneName);
    }



    public void SelectLocation(string locationId)
    {
        Debug.Log("SelectLocation 被呼叫，ID: " + locationId);

        selectedLocation = FindLocation(locationId);

        if (selectedLocation == null)
        {
            Debug.LogWarning("找不到地點: " + locationId);
            return;
        }

        locationTitleText.text = "要前往 " + selectedLocation.displayName + " 嗎";

        locationPanel.SetActive(true);
        locationPanel.transform.SetAsLastSibling();

        if (enterButton != null)
        {
            enterButton.interactable = true;
            enterButton.gameObject.SetActive(true);
        }
    }


    public void EnterSelectedLocation()
    {
        Debug.Log("EnterSelectedLocation 被呼叫");

        if (selectedLocation == null)
        {
            Debug.LogWarning("尚未選擇地點");
            return;
        }

        Debug.Log("準備進入：" + selectedLocation.locationId);

        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueManager 沒有設定");
            return;
        }

        if (selectedLocation.storyData == null)
        {
            Debug.LogWarning("StoryData 沒有設定：" + selectedLocation.locationId);
            return;
        }

        locationPanel.SetActive(false);

        if (selectedLocation.locationId == "L003")
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (CharactorLayer != null)
                CharactorLayer.SetActive(false);

            Time.timeScale = 1f;
            SceneManager.LoadScene("FirstScene");
            return;
        }

        mapPanel.SetActive(false);
        dialoguePanel.SetActive(true);
        CharactorLayer.SetActive(true);

        string questionGroup =
        GetQuestionGroup(selectedLocation.locationId);

        dialogueManager.StartTestGroup(questionGroup);

        Debug.Log("呼叫 StartStory：" + selectedLocation.storyData.name);
        dialogueManager.StartStory(selectedLocation.storyData);
    }
    /*public void SelectLocation(string locationId)
    {
        selectedLocation = FindLocation(locationId);

        if (selectedLocation == null)
        {
            Debug.LogWarning("找不到地點: " + locationId);
            return;
        }

        locationTitleText.text = "要前往 " + selectedLocation.displayName + " 嗎";
        locationPanel.SetActive(true);
    }*/

    public void CloseLocationPanel()
    {
        locationPanel.SetActive(false);
        selectedLocation = null;
    }

    private void ResetTimeScale()
    {
        // 避免玩家從暫停狀態回主選單後，
        // 新場景仍然維持 Time.timeScale = 0。
        Time.timeScale = 1f;
    }
    /*
    public void EnterSelectedLocation()
    {
        if (selectedLocation == null)
        {
            Debug.LogWarning("尚未選擇地點");
            return;
        }

        locationPanel.SetActive(false);
        

        if (dialogueManager != null && selectedLocation.storyData != null)
        {
            if (selectedLocation.locationId == "L003")
            {

                if (locationPanel != null)
                    locationPanel.SetActive(false);

                if (dialoguePanel != null)
                    dialoguePanel.SetActive(false);

                if (CharactorLayer != null)
                    CharactorLayer.SetActive(false);
                ResetTimeScale();
                SceneManager.LoadScene("FirstScene");
                return;

            }
            else
            {
                mapPanel.SetActive(false);
                dialoguePanel.SetActive(true);
                CharactorLayer.SetActive(true);
                dialogueManager.StartStory(selectedLocation.storyData);

            }
        }
        else
        {
            Debug.LogWarning("DialogueManager 或 StoryData 沒有設定");
        }
    }*/

    private LocationData FindLocation(string locationId)
    {
        foreach (LocationData loc in locations)
        {
            if (loc.locationId == locationId)
                return loc;
        }

        return null;
    }

    public void BackToMap()
    {
        if (locationPanel != null)
            locationPanel.SetActive(false);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (CharactorLayer != null)
            CharactorLayer.SetActive(false);
        
        if (mapPanel != null)
            mapPanel.SetActive(true);
    }
    private string GetQuestionGroup(string locationId)
    {
        switch (locationId)
        {
            case "L001":
                return "group_1";

            case "L002":
                return "group_2";

            case "L004":
                return "group_3";

            case "L005":
                return "group_4";

            default:
                Debug.LogWarning(
                    $"地點 {locationId} 沒有設定題組，預設使用 group_1。"
                );

                return "group_1";
        }
    }
    private IEnumerator WaitForMouseRelease()
    {
        

        // 等待前一個場景留下的滑鼠點擊放開
        while (Input.GetMouseButton(0))
        {
            yield return null;
        }

        
    }
}