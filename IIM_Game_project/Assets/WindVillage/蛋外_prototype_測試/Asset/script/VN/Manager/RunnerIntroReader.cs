using UnityEngine;

public class RunnerIntroResultReader : MonoBehaviour
{
    private void Start()
    {
        if (GameState.Instance == null)
        {
            Debug.LogWarning("找不到 GameState。", this);
            return;
        }

        if (!GameState.Instance.runnerIntroFinished)
        {
            Debug.Log("玩家不是從 RunnerIntro 回來，走一般 VN 流程。", this);
            int score2 = GameState.Instance.runnerIntroScore;
            int total2 = GameState.Instance.runnerIntroTotalQuestions;
            bool passed2 = GameState.Instance.runnerIntroPassed;
            Debug.Log(
            $"Runner 前導未完成：{score2}/{total2}，通過：{passed2}",
            this
            );
            return;
        }

        int score = GameState.Instance.runnerIntroScore;
        int total = GameState.Instance.runnerIntroTotalQuestions;
        bool passed = GameState.Instance.runnerIntroPassed;

        Debug.Log(
            $"Runner 前導完成：{score}/{total}，通過：{passed}",
            this
        );

        if (passed)
        {
            Debug.Log("顯示：你已初步理解風待村面臨的選擇……");
        }
        else
        {
            Debug.Log("顯示：你已踏出理解風待村的第一步……");
        }
    }
}