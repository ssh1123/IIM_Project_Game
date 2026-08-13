using UnityEngine;
using TMPro;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text pauseButtonText;

    public bool IsPaused { get; private set; }

    private void Start()
    {
        // 每次遊戲啟動時，確保不是暫停狀態
        SetPaused(false);
    }

    // 給按鈕 OnClick 呼叫
    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    private void SetPaused(bool shouldPause)
    {
        IsPaused = shouldPause;

        if (IsPaused)
        {
            Time.timeScale = 0f;

            if (pauseButtonText != null)
            {
                pauseButtonText.text = "繼續遊戲";
            }
        }
        else
        {
            Time.timeScale = 1f;

            if (pauseButtonText != null)
            {
                pauseButtonText.text = "暫停遊戲及使用AI";
            }
        }
    }

    private void OnDestroy()
    {
        // 防止物件被刪除時，遊戲卡在暫停狀態
        Time.timeScale = 1f;
    }
}