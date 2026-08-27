using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class PythonServerLauncher : MonoBehaviour
{
    [Header("Server Settings")]
    [Tooltip("StreamingAssets 內的資料夾名稱")]
    [SerializeField] private string serverFolder = "AI_Server";

    [Tooltip("打包後的執行檔名稱")]
    [SerializeField] private string exeName = "AI_Server.exe";

    [SerializeField] private string host = "127.0.0.1";
    [SerializeField] private string port = "8000";

    [Header("Check Settings")]
    [SerializeField] private float retryInterval = 1.5f;
    [SerializeField] private int maxRetryCount = 20;

    private Process serverProcess;
    public bool IsServerReady { get; private set; }
    public string LastServerError { get; private set; }

    private string HealthUrl => $"http://{host}:{port}/health";

    private void Start()
    {
        StartCoroutine(BootstrapServer());
    }

    private IEnumerator BootstrapServer()
    {
        yield return StartCoroutine(CheckServerReadyOnce());

        if (IsServerReady)
        {
            Debug.Log("偵測到 AI Server 已經在背景執行。");
            yield break;
        }

        StartServerProcess();

        if (serverProcess == null)
        {
            yield break;
        }

        yield return StartCoroutine(CheckServerReady());
    }

    private void StartServerProcess()
    {
        // 自動組合相對於遊戲執行目錄的路徑
        string workingDirectory = Path.Combine(Application.streamingAssetsPath, serverFolder);
        string exePath = Path.Combine(workingDirectory, exeName);

        if (!File.Exists(exePath))
        {
            LastServerError = $"找不到 AI Server 執行檔：{exePath}";
            Debug.LogError(LastServerError);
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            serverProcess = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            serverProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    Debug.Log("[AI Server] " + args.Data);
            };

            serverProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    LastServerError = args.Data;
                    Debug.LogWarning("[AI Server Error] " + args.Data);
                }
            };

            serverProcess.Exited += (sender, args) =>
            {
                Debug.LogWarning($"AI Server 已結束，ExitCode: {serverProcess.ExitCode}");
            };

            bool started = serverProcess.Start();
            if (!started)
            {
                LastServerError = "無法啟動 AI Server 行程。";
                Debug.LogError(LastServerError);
                return;
            }

            serverProcess.BeginOutputReadLine();
            serverProcess.BeginErrorReadLine();

            Debug.Log("已啟動 AI Server 服務。");
        }
        catch (Exception e)
        {
            LastServerError = e.Message;
            Debug.LogError("啟動 AI Server 失敗：" + e.Message);
        }
    }

    private IEnumerator CheckServerReadyOnce()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(HealthUrl))
        {
            request.timeout = 2;
            yield return request.SendWebRequest();
            IsServerReady = request.result == UnityWebRequest.Result.Success;
        }
    }

    private IEnumerator CheckServerReady()
    {
        for (int count = 1; count <= maxRetryCount; count++)
        {
            if (serverProcess != null && serverProcess.HasExited)
            {
                Debug.LogError($"AI Server 異常終止，ExitCode: {serverProcess.ExitCode}");
                IsServerReady = false;
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(HealthUrl))
            {
                request.timeout = 3;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string body = request.downloadHandler.text;
                    if (body.Contains("\"status\":\"ok\"") || body.Contains("\"status\": \"ok\""))
                    {
                        Debug.Log("AI Server 連線成功，模型載入就緒！");
                        IsServerReady = true;
                        yield break;
                    }
                    else
                    {
                        Debug.Log("Server 已回應，模型載入中...");
                    }
                }
            }

            yield return new WaitForSeconds(retryInterval);
        }

        Debug.LogError("AI Server 連線逾時。");
        IsServerReady = false;
    }

    private void OnApplicationQuit()
    {
        StopServerProcess();
    }

    private void OnDestroy()
    {
        StopServerProcess();
    }

    private void StopServerProcess()
    {
        try
        {
            if (serverProcess != null && !serverProcess.HasExited)
            {
                serverProcess.Kill();
                serverProcess.WaitForExit(2000);
                serverProcess.Dispose();
                Debug.Log("已關閉 AI Server 行程。");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("關閉 AI Server 時發生錯誤：" + e.Message);
        }
    }
}