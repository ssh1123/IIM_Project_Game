using UnityEngine;
using UnityEngine.SceneManagement;

public class Button_Main : MonoBehaviour
{
    public void ToVN()
    {
        SceneManager.LoadScene("StartStory");
    }
}