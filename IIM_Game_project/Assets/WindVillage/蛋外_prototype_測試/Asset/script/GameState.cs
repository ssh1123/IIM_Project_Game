using System;
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

    [Header("Index")]
    [SerializeField] private int funding = 50000;
    [SerializeField] private int interest = 50;
    [SerializeField] private int sustainability = 30;

    public int Funding => funding;
    public int Interest => interest;
    public int Sustainability => sustainability;
    public bool pre_VN_Finished = false;

    public event Action<int, int, int> OnIndexChanged;


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
    //=====VN Check ====
    public bool CheckPreVN()
    {
        if(pre_VN_Finished)
        {
            return true;
        }
        return false;
    }
    public void SetPreVN(bool isset)
    {
        pre_VN_Finished = isset;
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
    //========VN index function========
    public void ResetIndex()
    {
        Debug.LogWarning(
       "ResetIndex 被呼叫：Index 資料將被重設。",
       this
        );
        funding = 50000;
        interest = 50;
        sustainability = 30;
        OnIndexChanged?.Invoke(funding,interest,sustainability);

    }

    public void GetScore(int funding_delta , int interest_delta , int sustainability_delta)
    {
        Debug.Log(
       $"資金增加{funding_delta}，好感增加{interest_delta}，永續增加{sustainability_delta}"
        );

        funding += funding_delta;
        interest += interest_delta;
        sustainability += sustainability_delta;
        Debug.Log(
      $"目前index:資金 = {funding}，好感 = {interest}，永續 = {sustainability}"
       );
        OnIndexChanged?.Invoke(funding, interest, sustainability);
    }
    //========Final result function========

    public int GetFinalResult()
    {
        if(Funding >= 33500 && Interest >=80 && Sustainability >=80)
        {
            return 1;
        }
        else if(Funding <= 0)
        {
            return 2;   
        }
        else
        {
            return 3;
        }
    }




}