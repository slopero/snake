using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public int gridSize = 20;

    private GameObject currentFood;
    public Vector2Int FoodPosition { get; private set; }

    private Snake snake;

    void Start()
    {
        snake = FindObjectOfType<Snake>();
        SpawnFood();
    }

    public void SpawnFood()
    {
        Vector2Int newPos;

        do
        {
            int x = Random.Range(0, gridSize);
            int y = Random.Range(0, gridSize);
            newPos = new Vector2Int(x, y);
        }
        while (snake.IsOccupied(newPos)); // пока клетка занята телом — выбираем заново

        FoodPosition = newPos;

        if (currentFood == null)
        {
            currentFood = Instantiate(foodPrefab);
        }

        currentFood.transform.position = new Vector3(newPos.x, newPos.y, 0);
    }
}