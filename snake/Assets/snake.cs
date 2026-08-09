using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class Snake : MonoBehaviour
{
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

private int score = 0;

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

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
            newDir = Vector2Int.up;
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
            newDir = Vector2Int.down;
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
            newDir = Vector2Int.left;
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
            newDir = Vector2Int.right;

        if (newDir.HasValue && inputQueue.Count < 2) // ограничиваем очередь двумя нажатиями
        {
            bool isOpposite = newDir.Value == -lastQueuedDirection;
            bool isSame = newDir.Value == lastQueuedDirection;

            if (!isOpposite && !isSame)
            {
                inputQueue.Enqueue(newDir.Value);
                lastQueuedDirection = newDir.Value;
            }
        }
    }

    void Update()
    {
        if (isGameOver) return;

        if (!gameStarted)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                gameStarted = true;
            }
            return; // пока не началось — дальше код не выполняем
        }

        HandleInput();

        moveTimer += Time.deltaTime;

        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Move();
        }
    }

    void Move()
    {
        if (inputQueue.Count > 0)
        {
            direction = inputQueue.Dequeue();
        }

        gridPosition += direction;

        bodyPositions.Insert(0, gridPosition);
        Vector2Int removedTail = bodyPositions[bodyPositions.Count - 1]; // запоминаем хвост ДО удаления
        bodyPositions.RemoveAt(bodyPositions.Count - 1);

        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);

        for (int i = 0; i < bodySegments.Count; i++)
        {
            Vector2Int pos = bodyPositions[i + 1];
            bodySegments[i].position = new Vector3(pos.x, pos.y, 0);
        }

        if (gridPosition == foodSpawner.FoodPosition)
        {
            Grow(removedTail); // передаём сохранённую позицию
            foodSpawner.SpawnFood();
            score++;                // ← добавили
            UpdateScoreText();
        }

        CheckCollisions();
    }

    void UpdateScoreText()
    {
        scoreText.text = "Счёт: " + score;
    }

    void CheckCollisions()
    {
        if (gridPosition.x < 0 || gridPosition.x >= gridSize ||
            gridPosition.y < 0 || gridPosition.y >= gridSize)
        {
            GameOver();
            return;
        }

        for (int i = 1; i < bodyPositions.Count; i++)
        {
            if (bodyPositions[i] == gridPosition)
            {
                GameOver();
                return;
            }
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over!");
        gameOverPanel.SetActive(true);
        finalScoreText.text = "Счёт: " + score;
    }

    public void Grow(Vector2Int newSegmentPos)
    {
        bodyPositions.Add(newSegmentPos);

        GameObject newSegment = Instantiate(segmentPrefab);
        newSegment.transform.position = new Vector3(newSegmentPos.x, newSegmentPos.y, 0);
        bodySegments.Add(newSegment.transform);
    }
}