using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class Setting : MonoBehaviour
{


    [SerializeField]
    private TMP_InputField playerID_Input;
    [SerializeField] private TMP_Text playerIDText;

    public void SetAIEnabled(bool isEnabled)
    {
        GameState.Instance.SetAIEnabled(isEnabled);
    }
    public void SetFeedbackEnabled(bool isEnabled)
    {
        GameState.Instance.SetFeedbackEnabled(isEnabled);
    }
    /*public void SentPlayerId()
    {
        if(playerID_Input != null)
        {
            LearningGameResultUploader.Instance.playerId = playerID_Input.text.Trim();
            Debug.Log(
           $"已設定playerID:{LearningGameResultUploader.Instance.playerId}"
             );
        }
        else
        {
            Debug.Log(
           "playerID為unknown!!!!"
             );
        }
        
    }*/
    public void SendPlayerId()
    {
        LearningGameResultUploader uploader =
            FindFirstObjectByType<LearningGameResultUploader>();

        if (uploader == null)
        {
            Debug.LogError(
                "找不到 LearningGameResultUploader，無法設定 Player ID。",
                this
            );
            return;
        }

        if (playerID_Input == null)
        {
            Debug.LogError(
                "playerID_Input 尚未在 Inspector 指定。",
                this
            );
            return;
        }

        string inputPlayerId = playerID_Input.text.Trim();

        if (string.IsNullOrWhiteSpace(inputPlayerId))
        {
            Debug.LogWarning(
                "請先輸入 Player ID，不能是空白。",
                this
            );
            return;
        }

        uploader.SetPlayerId(inputPlayerId);
        playerIDText.text = "PlayerID已設定為：" + inputPlayerId;
        Debug.Log(
            $"已設定 Player ID：{uploader.playerId}",
            this
        );
    }
}