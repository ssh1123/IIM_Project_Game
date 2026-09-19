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

    [Header("Location UI")]
    [SerializeField] private TMP_Text locationTitleText;
    [SerializeField] private Button enterButton;

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

    private void Start()
    {
        
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
}