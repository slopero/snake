using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public int gridSize = 20; // размер поля 20x20, как ты и делал

    private GameObject currentFood;
    public Vector2Int FoodPosition { get; private set; }

    void Start()
    {
        SpawnFood();
    }

    public void SpawnFood()
    {
        int x = Random.Range(0, gridSize);
        int y = Random.Range(0, gridSize);
        FoodPosition = new Vector2Int(x, y);

        if (currentFood == null)
        {
            currentFood = Instantiate(foodPrefab);
        }

        currentFood.transform.position = new Vector3(x, y, 0);
    }
}