using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Snake : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    private bool countdownStarted = false;
    private Queue<Vector2Int> inputQueue = new Queue<Vector2Int>();
    private Vector2Int lastQueuedDirection;
    private Vector2Int direction = Vector2Int.right; // куда движется змейка сейчас
    private Vector2Int gridPosition;    
    private bool gameStarted = false;              // текущая позиция в клетках сетки
    private float moveTimer;
    public float moveInterval = 0.2f;                 // раз в сколько секунд двигаться
    public GameObject segmentPrefab; // сюда перетащим префаб в инспекторе
    private List<Vector2Int> bodyPositions = new List<Vector2Int>();
    private List<Transform> bodySegments = new List<Transform>();
    private FoodSpawner foodSpawner; // ← добавили сюда
    public int gridSize = 11; // размер сетки, чтобы не выйти за границы
    private bool isGameOver = false; // флаг окончания игры
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    private Vector2 touchStartPos;
    private bool touchActive = false;
    public float swipeThreshold = 50f; // минимальное расстояние свайпа в пикселях, чтобы засчитать поворот
    private int score = 0;
    public bool useButtons = false;
    public GameObject buttonPanel; // ссылка на панель кнопок из пункта 3


    public void PressUp() => TryQueueDirection(Vector2Int.up);
    public void PressDown() => TryQueueDirection(Vector2Int.down);
    public void PressLeft() => TryQueueDirection(Vector2Int.left);
    public void PressRight() => TryQueueDirection(Vector2Int.right);

    public bool IsOccupied(Vector2Int pos)
    {
        return bodyPositions.Contains(pos);
    }

    void Start()
    {
        gridPosition = new Vector2Int(gridSize / 2, gridSize / 2);
        bodyPositions.Add(gridPosition);
        foodSpawner = FindObjectOfType<FoodSpawner>(); // ← добавили сюда
        lastQueuedDirection = direction; // инициализируем lastQueuedDirection
        UpdateScoreText();
    }
    
    void HandleInput()
    {
        Vector2Int? newDir = null;

        // клавиатура — оставляем всегда, не мешает
        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
            newDir = Vector2Int.up;
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
            newDir = Vector2Int.down;
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
            newDir = Vector2Int.left;
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
            newDir = Vector2Int.right;

        if (newDir.HasValue)
        {
            TryQueueDirection(newDir.Value);
        }

        if (!useButtons)
        {
            HandleTouchInput();
        }
    }

    public void SetControlMode(bool buttonsEnabled)
    {
        useButtons = buttonsEnabled;
        buttonPanel.SetActive(buttonsEnabled);
    }
    void HandleTouchInput()
    {
        if (Touchscreen.current == null) return; // на устройстве без тача просто выходим

        var touch = Touchscreen.current.primaryTouch;

        if (touch.press.wasPressedThisFrame)
        {
            touchStartPos = touch.position.ReadValue();
            touchActive = true;
        }

        if (touch.press.wasReleasedThisFrame && touchActive)
        {
            touchActive = false;
            Vector2 touchEndPos = touch.position.ReadValue();
            Vector2 delta = touchEndPos - touchStartPos;

            if (delta.magnitude < swipeThreshold) return; // слишком короткий свайп, игнорируем

            Vector2Int swipeDir;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                swipeDir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                swipeDir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            TryQueueDirection(swipeDir);
        }
    }

    void TryQueueDirection(Vector2Int newDir)
    {
        if (inputQueue.Count >= 2) return;

        bool isOpposite = newDir == -lastQueuedDirection;
        bool isSame = newDir == lastQueuedDirection;

        if (!isOpposite && !isSame)
        {
            inputQueue.Enqueue(newDir);
            lastQueuedDirection = newDir;
        }
    }

    void Update()
    {
        if (isGameOver) return;

        if (!gameStarted)
        {
            if (!countdownStarted)
            {
                bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
                bool touchPressed = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

                if (keyPressed || touchPressed)
                {
                    countdownStarted = true;
                    StartCoroutine(StartCountdown());
                }
            }
            return;
        }

        HandleInput();

        moveTimer += Time.deltaTime;

        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Move();
        }
    }

    IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        countdownText.gameObject.SetActive(false);
        gameStarted = true;
    }

    void Move()
    {
        if (inputQueue.Count > 0)
        {
            direction = inputQueue.Dequeue();
        }

        Vector2Int newHeadPos = gridPosition + direction;

        if (IsCollision(newHeadPos))
        {
            GameOver();
            return; // не двигаем голову и не рисуем — она останется на прежнем месте
        }

        gridPosition = newHeadPos;

        bodyPositions.Insert(0, gridPosition);
        Vector2Int removedTail = bodyPositions[bodyPositions.Count - 1];
        bodyPositions.RemoveAt(bodyPositions.Count - 1);

        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);

        for (int i = 0; i < bodySegments.Count; i++)
        {
            Vector2Int pos = bodyPositions[i + 1];
            bodySegments[i].position = new Vector3(pos.x, pos.y, 0);
        }

        if (gridPosition == foodSpawner.FoodPosition)
        {
            Grow(removedTail);
            foodSpawner.SpawnFood();
            score++;
            UpdateScoreText();
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }

    bool IsCollision(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= gridSize || pos.y < 0 || pos.y >= gridSize)
            return true;

        // проверяем тело, кроме последнего сегмента (хвоста) — он в любом случае освободит эту клетку в этот же ход
        for (int i = 0; i < bodyPositions.Count - 1; i++)
        {
            if (bodyPositions[i] == pos)
                return true;
        }

        return false;
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over!");
        gameOverPanel.SetActive(true);
        finalScoreText.text = "Score: " + score;
    }

    public void Grow(Vector2Int newSegmentPos)
    {
        bodyPositions.Add(newSegmentPos);

        GameObject newSegment = Instantiate(segmentPrefab);
        newSegment.transform.position = new Vector3(newSegmentPos.x, newSegmentPos.y, 0);
        bodySegments.Add(newSegment.transform);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}