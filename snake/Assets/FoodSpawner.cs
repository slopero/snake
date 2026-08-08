using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;

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
            int x = Random.Range(0, snake.gridSize);
            int y = Random.Range(0, snake.gridSize);
            newPos = new Vector2Int(x, y);
        }
        while (snake.IsOccupied(newPos));

        FoodPosition = newPos;

        if (currentFood == null)
        {
            currentFood = Instantiate(foodPrefab);
        }

        currentFood.transform.position = new Vector3(newPos.x, newPos.y, 0);
    }
}