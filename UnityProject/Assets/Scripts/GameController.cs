using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 貪食蛇遊戲主控制器
/// 負責：
/// 1. 處理玩家輸入
/// 2. 控制遊戲流程
/// 3. 渲染遊戲畫面
/// 
/// 通訊邏輯由 PythonClient 處理
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("Python Client")]
    [SerializeField] private PythonClient pythonClient;

    [Header("Prefabs")]
    [SerializeField] private GameObject snakeSegmentPrefab;
    [SerializeField] private GameObject foodPrefab;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Game Settings")]
    [SerializeField] private float updateInterval = 0.15f;
    [SerializeField] private Color borderColor = Color.white;

    // 遊戲物件
    private List<GameObject> snakeObjects = new List<GameObject>();
    private GameObject foodObject;
    private GameObject borderObject;

    // 遊戲區域大小
    private int gameWidth = 20;
    private int gameHeight = 15;

    // 遊戲狀態
    private string currentDirection = "RIGHT";
    private string pendingDirection = "RIGHT";
    private bool isGameOver = false;
    private bool isGameStarted = false;
    private bool isWaitingResponse = false;

    // 計時器
    private float timer = 0f;

    void Start()
    {
        // 初始化 UI
        gameOverText.gameObject.SetActive(false);
        if (statusText != null)
            statusText.text = "Connecting to server...";

        // 檢查 PythonClient 是否設定
        if (pythonClient == null)
        {
            pythonClient = FindObjectOfType<PythonClient>();
            if (pythonClient == null)
            {
                Debug.LogError("PythonClient not found! Please add PythonClient to the scene.");
                return;
            }
        }

        // 開始遊戲
        StartCoroutine(InitializeGame());
    }

    IEnumerator InitializeGame()
    {
        // 先檢查 Server 狀態
        bool serverOnline = false;
        yield return pythonClient.CheckHealth(result => serverOnline = result);

        if (!serverOnline)
        {
            if (statusText != null)
                statusText.text = "Server offline!\nRun: python server.py";
            Debug.LogError("Python Server is not running!");
            yield break;
        }

        if (statusText != null)
            statusText.text = "";

        // 開始新遊戲
        yield return StartNewGame();
    }

    void Update()
    {
        if (!isGameStarted) return;

        // 處理鍵盤輸入
        HandleInput();

        // 定時更新遊戲
        if (!isGameOver && !isWaitingResponse)
        {
            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                timer = 0f;
                StartCoroutine(SendPlayerAction());
            }
        }

        // 按 R 重新開始
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(StartNewGame());
        }
    }

    /// <summary>
    /// 處理玩家鍵盤輸入
    /// </summary>
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (currentDirection != "DOWN")
                pendingDirection = "UP";
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (currentDirection != "UP")
                pendingDirection = "DOWN";
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentDirection != "RIGHT")
                pendingDirection = "LEFT";
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentDirection != "LEFT")
                pendingDirection = "RIGHT";
        }
    }

    /// <summary>
    /// 開始新遊戲
    /// </summary>
    IEnumerator StartNewGame()
    {
        // 清除舊物件
        ClearGameObjects();

        // 重置狀態
        isGameOver = false;
        gameOverText.gameObject.SetActive(false);
        currentDirection = "RIGHT";
        pendingDirection = "RIGHT";

        // 透過 PythonClient 請求開始新遊戲
        yield return pythonClient.StartGame(
            onSuccess: state =>
            {
                UpdateGameDisplay(state);
                isGameStarted = true;
                Debug.Log("Game started!");
            },
            onError: error =>
            {
                Debug.LogError(error);
                if (statusText != null)
                    statusText.text = "Error: " + error;
            }
        );
    }

    /// <summary>
    /// 發送玩家動作到 Server
    /// </summary>
    IEnumerator SendPlayerAction()
    {
        isWaitingResponse = true;

        yield return pythonClient.SendAction(
            pendingDirection,
            onSuccess: state =>
            {
                UpdateGameDisplay(state);
                currentDirection = pendingDirection;
            },
            onError: error =>
            {
                Debug.LogError(error);
            }
        );

        isWaitingResponse = false;
    }

    /// <summary>
    /// 根據 Server 回傳的狀態更新畫面
    /// </summary>
    void UpdateGameDisplay(GameState state)
    {
        // 更新遊戲區域大小
        gameWidth = state.width;
        gameHeight = state.height;

        // 創建/更新邊界
        UpdateBorder();

        UpdateSnake(state.snake);
        UpdateFood(state.food);
        UpdateScore(state.score);

        if (state.game_over)
        {
            isGameOver = true;
            gameOverText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 創建並更新遊戲邊界
    /// </summary>
    void UpdateBorder()
    {
        if (borderObject == null)
        {
            borderObject = new GameObject("Border");
            LineRenderer lr = borderObject.AddComponent<LineRenderer>();
            
            // 設定 LineRenderer
            lr.positionCount = 5;  // 四個角 + 回到起點
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;
            lr.loop = false;
            lr.useWorldSpace = true;

            // 使用預設材質並設定顏色
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = borderColor;
            lr.endColor = borderColor;
        }

        LineRenderer lineRenderer = borderObject.GetComponent<LineRenderer>();
        
        // 邊界座標（遊戲區域是 0 到 width-1, 0 到 height-1）
        // 邊界應該在遊戲區域外圍，所以用 -0.5 到 width-0.5
        float minX = -0.5f;
        float maxX = gameWidth - 0.5f;
        float minY = -0.5f;
        float maxY = gameHeight - 0.5f;

        lineRenderer.SetPosition(0, new Vector3(minX, minY, 0));  // 左下
        lineRenderer.SetPosition(1, new Vector3(maxX, minY, 0));  // 右下
        lineRenderer.SetPosition(2, new Vector3(maxX, maxY, 0));  // 右上
        lineRenderer.SetPosition(3, new Vector3(minX, maxY, 0));  // 左上
        lineRenderer.SetPosition(4, new Vector3(minX, minY, 0));  // 回到左下
    }

    /// <summary>
    /// 更新蛇的顯示
    /// </summary>
    void UpdateSnake(List<List<int>> snakePositions)
    {
        // 調整蛇物件數量
        while (snakeObjects.Count < snakePositions.Count)
        {
            GameObject segment = Instantiate(snakeSegmentPrefab);
            snakeObjects.Add(segment);
        }
        while (snakeObjects.Count > snakePositions.Count)
        {
            Destroy(snakeObjects[snakeObjects.Count - 1]);
            snakeObjects.RemoveAt(snakeObjects.Count - 1);
        }

        // 更新位置與顏色
        for (int i = 0; i < snakePositions.Count; i++)
        {
            snakeObjects[i].transform.position = new Vector3(
                snakePositions[i][0],
                snakePositions[i][1],
                0
            );

            // 頭部顏色較深
            var renderer = snakeObjects[i].GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = (i == 0) 
                    ? new Color(0f, 0.8f, 0f)   // 深綠色（頭）
                    : new Color(0f, 0.6f, 0f);  // 淺綠色（身體）
            }
        }
    }

    /// <summary>
    /// 更新食物的顯示
    /// </summary>
    void UpdateFood(List<int> foodPosition)
    {
        if (foodObject == null)
        {
            foodObject = Instantiate(foodPrefab);
        }

        foodObject.transform.position = new Vector3(
            foodPosition[0],
            foodPosition[1],
            0
        );
    }

    /// <summary>
    /// 更新分數顯示
    /// </summary>
    void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    /// <summary>
    /// 清除所有遊戲物件
    /// </summary>
    void ClearGameObjects()
    {
        foreach (var obj in snakeObjects)
        {
            Destroy(obj);
        }
        snakeObjects.Clear();

        if (foodObject != null)
        {
            Destroy(foodObject);
            foodObject = null;
        }
    }
}
