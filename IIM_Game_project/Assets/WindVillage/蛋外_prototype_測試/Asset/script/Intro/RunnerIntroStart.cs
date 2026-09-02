using UnityEngine;
using TMPro;
public class RunnerIntroStartPanel : MonoBehaviour
{
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private RunnerGameManager gameManager;
    [SerializeField] private TMP_Text introText;

    private bool hasStarted;

    private void Awake()
    {
        Time.timeScale = 0f;
        Debug.Log("Intro 開啟，Time Scale = " + Time.timeScale);

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
        if (introPanel != null)
        {
            introPanel.SetActive(true);
            if(GameState.Instance.IsAIEnabled)
            {
                introText.text = "使用方向鍵或W、S 鍵控制角色移動，\r\n與蛋蛋一同經歷一場「地方創生」為名冒險、\r\n吸收正確的知識能量完成關卡。\r\n(已解鎖右上AI功能、可暫停詢問教學內容)";
            }
            else
            {
                introText.text = "使用方向鍵或W、S 鍵控制角色移動，\r\n與蛋蛋一同經歷一場「地方創生」為名冒險、\r\n吸收正確的知識能量完成關卡。";
            }
        }
    }

    private void Update()
    {
        if (hasStarted)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (hasStarted)
            return;

        hasStarted = true;

        if (introPanel != null)
        {
            introPanel.SetActive(false);
        }
        gameManager.ShowFirstQuestion();
        Time.timeScale = 1f;

    }

    private void OnDestroy()
    {
        // 避免離開 RunnerIntro 時，下一個場景仍被暫停。
        Time.timeScale = 1f;
    }
}