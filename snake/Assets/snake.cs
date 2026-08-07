using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Snake : MonoBehaviour
{
    private Vector2Int direction = Vector2Int.right; // куда движется змейка сейчас
    private Vector2Int gridPosition;                  // текущая позиция в клетках сетки
    private float moveTimer;
    public float moveInterval = 0.2f;                 // раз в сколько секунд двигаться
    public GameObject segmentPrefab; // сюда перетащим префаб в инспекторе
    private List<Vector2Int> bodyPositions = new List<Vector2Int>();
    private List<Transform> bodySegments = new List<Transform>();
    private FoodSpawner foodSpawner; // ← добавили сюда

    void Start()
    {
        gridPosition = Vector2Int.zero;
        bodyPositions.Add(gridPosition);
        foodSpawner = FindObjectOfType<FoodSpawner>(); // ← добавили сюда
    }

    void Update()
    {
        HandleInput();

        moveTimer += Time.deltaTime;

        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Move();
        }
    }

    void HandleInput()
    {
        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
        {
            if (direction != Vector2Int.down)
                direction = Vector2Int.up;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            if (direction != Vector2Int.up)
                direction = Vector2Int.down;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
        {
            if (direction != Vector2Int.right)
                direction = Vector2Int.left;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
        {
            if (direction != Vector2Int.left)
                direction = Vector2Int.right;
        }
    }
    void Move()
    {
        gridPosition += direction;

        bodyPositions.Insert(0, gridPosition); // новая позиция головы в начало списка
        bodyPositions.RemoveAt(bodyPositions.Count - 1); // убираем последний хвост

        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);

        for (int i = 0; i < bodySegments.Count; i++)
        {
            Vector2Int pos = bodyPositions[i + 1]; // +1 т.к. позиция [0] это голова
            bodySegments[i].position = new Vector3(pos.x, pos.y, 0);
        }
        
        if (gridPosition == foodSpawner.FoodPosition)
        {
            Grow();
            foodSpawner.SpawnFood();
        }
    }

    public void Grow()
    {
        Vector2Int newSegmentPos = bodyPositions[bodyPositions.Count - 1];
        bodyPositions.Add(newSegmentPos);

        GameObject newSegment = Instantiate(segmentPrefab);
        bodySegments.Add(newSegment.transform);
    }
}