using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class PythonServerLauncher : MonoBehaviour
{
    [Header("Python Settings")]
    [SerializeField] private string pythonExePath = @"C:\Users\YourName\AppData\Local\Programs\Python\Python311\python.exe";
    [SerializeField] private string uvicornModule = "uvicorn";
    [SerializeField] private string appEntry = "main:app";
    [SerializeField] private string host = "127.0.0.1";
    [SerializeField] private string port = "8000";
    [SerializeField] private string workingDirectory = @"C:\YourProject\ai_test_server";

    [Header("Check Settings")]
    [SerializeField] private float retryInterval = 1f;
    [SerializeField] private int maxRetryCount = 10;

    private Process pythonProcess;
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
            Debug.Log("偵測到 Python server 已經在執行。");
            yield break;
        }

        StartPythonServer();
        yield return StartCoroutine(CheckServerReady());
    }

    private void StartPythonServer()
    {
        if (!File.Exists(pythonExePath))
        {
            Debug.LogError("找不到 Python 執行檔：" + pythonExePath);
            return;
        }

        if (!Directory.Exists(workingDirectory))
        {
            Debug.LogError("找不到工作目錄：" + workingDirectory);
            return;
        }

        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            Debug.Log("Python server 已經在執行中。");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonExePath,
            Arguments = $"-m {uvicornModule} {appEntry} --host {host} --port {port}",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            pythonProcess = new Process();
            pythonProcess.StartInfo = startInfo;

            pythonProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    Debug.Log("[Python] " + args.Data);
            };

            pythonProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    LastServerError = args.Data;
                    Debug.LogWarning("[Python Error] " + args.Data);
                }
            };

            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();

            Debug.Log("已啟動 Python server process。");
        }
        catch (System.Exception e)
        {
            LastServerError = e.Message;
            Debug.LogError("啟動 Python server 失敗：" + e.Message);
        }
    }

    private IEnumerator CheckServerReadyOnce()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(HealthUrl))
        {
            yield return request.SendWebRequest();
            IsServerReady = request.result == UnityWebRequest.Result.Success;
        }
    }

    private IEnumerator CheckServerReady()
    {
        int count = 0;

        while (count < maxRetryCount)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(HealthUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Python server 已連線成功。");
                    IsServerReady = true;
                    yield break;
                }
            }

            count++;
            Debug.Log("等待 Python server 啟動中... 第 " + count + " 次檢查");
            yield return new WaitForSeconds(retryInterval);
        }

        Debug.LogError("Python server 啟動逾時，無法連線。");
        if (!string.IsNullOrEmpty(LastServerError))
            Debug.LogError("最近的 Python 錯誤：" + LastServerError);

        IsServerReady = false;
    }

    private void OnApplicationQuit()
    {
        try
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                pythonProcess.Kill();
                pythonProcess.Dispose();
                Debug.Log("已關閉 Python server process。");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("關閉 Python server 時發生問題：" + e.Message);
        }
    }
}