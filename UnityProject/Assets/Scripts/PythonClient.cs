using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Python Server 通訊客戶端
/// 封裝所有與 Python Server 的 HTTP 通訊邏輯
/// 
/// 這個類別展示了 Unity ↔ Python 通訊的核心概念：
/// 1. 使用 HTTP POST/GET 請求
/// 2. 使用 JSON 格式傳遞資料
/// 3. 使用 Coroutine 處理異步通訊
/// </summary>
public class PythonClient : MonoBehaviour
{
    [Header("Server Configuration")]
    [SerializeField] private string serverUrl = "http://localhost:5000";
    [SerializeField] private float timeout = 5f;

    /// <summary>
    /// 檢查 Server 是否在線
    /// </summary>
    public IEnumerator CheckHealth(Action<bool> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl + "/health"))
        {
            request.timeout = (int)timeout;
            yield return request.SendWebRequest();
            callback?.Invoke(request.result == UnityWebRequest.Result.Success);
        }
    }

    /// <summary>
    /// 開始新遊戲
    /// POST /start -> 回傳初始遊戲狀態
    /// </summary>
    public IEnumerator StartGame(Action<GameState> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(serverUrl + "/start", ""))
        {
            request.timeout = (int)timeout;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    GameState state = GameState.FromJson(request.downloadHandler.text);
                    onSuccess?.Invoke(state);
                }
                catch (Exception e)
                {
                    onError?.Invoke("JSON Parse Error: " + e.Message);
                }
            }
            else
            {
                onError?.Invoke(GetErrorMessage(request));
            }
        }
    }

    /// <summary>
    /// 發送玩家動作（方向）
    /// POST /action with JSON body -> 回傳更新後的遊戲狀態
    /// </summary>
    public IEnumerator SendAction(string direction, Action<GameState> onSuccess, Action<string> onError)
    {
        // 建立 JSON 請求內容
        string jsonBody = "{\"direction\": \"" + direction + "\"}";

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/action", "POST"))
        {
            // 設定請求內容
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = (int)timeout;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    GameState state = GameState.FromJson(request.downloadHandler.text);
                    onSuccess?.Invoke(state);
                }
                catch (Exception e)
                {
                    onError?.Invoke("JSON Parse Error: " + e.Message);
                }
            }
            else
            {
                onError?.Invoke(GetErrorMessage(request));
            }
        }
    }

    /// <summary>
    /// 取得當前遊戲狀態
    /// GET /state -> 回傳當前遊戲狀態
    /// </summary>
    public IEnumerator GetState(Action<GameState> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl + "/state"))
        {
            request.timeout = (int)timeout;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    GameState state = GameState.FromJson(request.downloadHandler.text);
                    onSuccess?.Invoke(state);
                }
                catch (Exception e)
                {
                    onError?.Invoke("JSON Parse Error: " + e.Message);
                }
            }
            else
            {
                onError?.Invoke(GetErrorMessage(request));
            }
        }
    }

    /// <summary>
    /// 產生錯誤訊息
    /// </summary>
    private string GetErrorMessage(UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            return "Connection Error: 無法連接到 Python Server\n" +
                   "請確認：\n" +
                   "1. Python Server 是否已啟動 (python server.py)\n" +
                   "2. Server URL 是否正確 (" + serverUrl + ")";
        }
        return request.error;
    }
}

/// <summary>
/// 動作請求資料結構
/// 對應 Python Server 接收的 JSON 格式
/// </summary>
[Serializable]
public class ActionRequest
{
    public string direction;
}
