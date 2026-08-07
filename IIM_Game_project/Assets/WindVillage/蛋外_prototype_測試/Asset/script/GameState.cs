using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [Header("VN Flags")]
    private HashSet<string> flags = new HashSet<string>();

    [Header("Runner Intro Result")]
    public bool runnerIntroFinished;
    public int runnerIntroScore;
    public int runnerIntroTotalQuestions;
    public bool runnerIntroPassed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log($"GameState 已建立並保留：{name}", this);
    }

    // ===== VN Flag Methods =====

    public void SetFlag(string flag)
    {
        if (!string.IsNullOrEmpty(flag))
        {
            flags.Add(flag);
        }
    }

    public bool HasFlag(string flag)
    {
        return flags.Contains(flag);
    }

    public bool HasAllFlags(List<string> requiredFlags)
    {
        if (requiredFlags == null || requiredFlags.Count == 0)
            return true;

        foreach (string flag in requiredFlags)
        {
            if (!flags.Contains(flag))
                return false;
        }

        return true;
    }

    public void ClearFlags()
    {
        flags.Clear();
    }

    // ===== Runner Intro Methods =====

    public void SaveRunnerIntroResult(int score, int totalQuestions)
    {
        runnerIntroFinished = true;
        runnerIntroScore = score;
        runnerIntroTotalQuestions = totalQuestions;

        // 目前 MVP 規則：全部答對才算通過
        runnerIntroPassed = score >= totalQuestions;

        Debug.Log(
            $"已保存 Runner 結果：{runnerIntroScore}/{runnerIntroTotalQuestions}，" +
            $"通過：{runnerIntroPassed}"
        );
    }

    public void ResetRunnerIntroResult()
    {
        Debug.LogWarning(
        "ResetRunnerIntroResult 被呼叫：Runner 資料即將清空。",
        this
         );
        runnerIntroFinished = false;
        runnerIntroScore = 0;
        runnerIntroTotalQuestions = 0;
        runnerIntroPassed = false;
    }
}