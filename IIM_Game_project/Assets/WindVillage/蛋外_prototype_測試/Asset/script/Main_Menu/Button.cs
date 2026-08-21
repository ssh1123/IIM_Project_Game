using UnityEngine;
using UnityEngine.SceneManagement;

public class Button_Main : MonoBehaviour
{
    public void ToVN()
    {
        SceneManager.LoadScene("StartStory");
    }
    public void QuitGame()
    {
        Debug.Log("退出遊戲");

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }
}