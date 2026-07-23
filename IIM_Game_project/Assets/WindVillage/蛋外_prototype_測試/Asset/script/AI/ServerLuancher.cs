using System.Collections;
using System.Diagnostics;
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
    [SerializeField] private string healthUrl = "http://127.0.0.1:8000/health";
    [SerializeField] private float retryInterval = 1f;
    [SerializeField] private int maxRetryCount = 10;

    private Process pythonProcess;
    public bool IsServerReady { get; private set; }

    private void Start()
    {
        StartPythonServer();
        StartCoroutine(CheckServerReady());
    }

    private void StartPythonServer()
    {
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
                    Debug.LogWarning("[Python Error] " + args.Data);
            };

            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();

            Debug.Log("已啟動 Python server process。");
        }
        catch (System.Exception e)
        {
            Debug.LogError("啟動 Python server 失敗：" + e.Message);
        }
    }

    private IEnumerator CheckServerReady()
    {
        int count = 0;

        while (count < maxRetryCount)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(healthUrl))
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