
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ENDManager : MonoBehaviour
{ 
    [Header("END 結果")]
    [SerializeField] private TMP_Text ENDText; //ENDText.text = 

    public void Start()
    {
        switch (GameState.Instance.GetFinalResult())
        {
            case 1:
                ENDText.text = "`永續共榮,完美結局";
                break;

            case 2:
                ENDText.text = "你破產了";
                break;

            case 3:
                ENDText.text = "一般結局";
                break;
        }
    }

    public void go_MainMenu()
    {

        GameState.Instance.ResetGameState();
        SceneManager.LoadScene("Main_Menu");
    }
}
