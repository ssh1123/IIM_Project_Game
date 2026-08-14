using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string introSceneName = "RunnerIntro";
    [SerializeField] private string vnSceneName = "VN_Main";
    [SerializeField] private string storyDatabaseSceneName = "StoryDatabase";

    public void GoToIntro()
    {
        ResetTimeScale();
        SceneManager.LoadScene(introSceneName);
    }

    public void GoToVN()
    {
        ResetTimeScale();
        SceneManager.LoadScene(vnSceneName);
    }

    public void GoToStoryDatabase()
    {
        ResetTimeScale();
        SceneManager.LoadScene(storyDatabaseSceneName);
    }

    private void ResetTimeScale()
    {
        // 避免玩家從暫停狀態回主選單後，
        // 新場景仍然維持 Time.timeScale = 0。
        Time.timeScale = 1f;
    }
}