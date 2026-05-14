using UnityEngine;

public class ObstacleGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnableObject
    {
        public GameObject prefab;

        [Range(0f, 1f)]
        public float spawnChance;
    }

    [System.Serializable]
    public struct SpawnableItem
    {
        public GameObject prefab;
        public ItemType itemType;

        [Range(0f, 1f)]
        public float spawnChance;
    }

    [Header("Obstacles")]
    public SpawnableObject[] objects;

    [Header("Items")]
    public SpawnableItem[] items;

    [Header("Item Spawn Settings")]
    public float itemSpawnInterval = 40f;

    [Tooltip("Khoảng cách tối thiểu sau obstacle")]
    public float minDistanceAfterObstacle = 5f;

    [Tooltip("Khoảng cách tối đa sau obstacle")]
    public float maxDistanceAfterObstacle = 8f;

    [Tooltip("Độ cao thấp nhất player có thể nhặt")]
    public float minItemHeight = 1.0f;

    [Tooltip("Độ cao cao nhất player có thể nhặt")]
    public float maxItemHeight = 2.0f;

    [Header("Item Collision Check")]
    public float itemCheckRadius = 1f;

    public Transform itemParent;

    // =====================================================
    // NEW SPAWN SYSTEM
    // =====================================================

    [Header("Dynamic Difficulty")]

    [Tooltip("Khoảng cách obstacle khi game dễ")]
    public float easySpawnDistance = 14f;

    [Tooltip("Khoảng cách obstacle khi game khó")]
    public float hardSpawnDistance = 7f;

    [Tooltip("Game speed bắt đầu tăng difficulty")]
    public float minDifficultySpeed = 5f;

    [Tooltip("Game speed đạt max difficulty")]
    public float maxDifficultySpeed = 20f;

    [Tooltip("Khoảng cách an toàn tối thiểu")]
    public float absoluteMinDistance = 6f;

    [Tooltip("Stress mode giảm khoảng cách")]
    public float stressDistanceMultiplier = 0.8f;

    // Runtime
    private float distanceCounter;
    private float nextSpawnDistance;

    private GameObject lastSpawnedObject;
    private Vector3 lastObstaclePosition;
    private bool hasSpawnedObstacle;

    // =====================================================
    // UNITY
    // =====================================================

    private void OnEnable()
    {
        distanceCounter = 0f;

        GenerateNextSpawnDistance();

        InvokeRepeating(
            nameof(SpawnRandomItem),
            itemSpawnInterval,
            itemSpawnInterval
        );
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.isGameOver) return;

        float speed = GameManager.Instance.GetFinalGameSpeed();

        // =====================================================
        // DISTANCE BASED SPAWN
        // =====================================================

        distanceCounter += speed * Time.deltaTime;

        if (distanceCounter >= nextSpawnDistance)
        {
            SpawnObstacle();

            distanceCounter = 0f;

            GenerateNextSpawnDistance();
        }
    }

    // =====================================================
    // GENERATE NEXT DISTANCE
    // =====================================================

    private void GenerateNextSpawnDistance()
    {
        if (GameManager.Instance == null)
        {
            nextSpawnDistance = easySpawnDistance;
            return;
        }

        float speed = GameManager.Instance.GetFinalGameSpeed();

        // Difficulty 0 -> 1
        float difficulty = Mathf.InverseLerp(
            minDifficultySpeed,
            maxDifficultySpeed,
            speed
        );

        // Speed càng cao -> khoảng cách càng thấp
        float distance = Mathf.Lerp(
            easySpawnDistance,
            hardSpawnDistance,
            difficulty
        );

        // Random nhẹ để gameplay tự nhiên hơn
        distance += Random.Range(-1.5f, 1.5f);

        // =====================================================
        // STRESS MODE
        // =====================================================

        if (GameManager.Instance.spawnRateMultiplier > 1f)
        {
            distance *= stressDistanceMultiplier;
        }

        // =====================================================
        // SAFE DISTANCE
        // =====================================================

        distance = Mathf.Max(distance, absoluteMinDistance);

        nextSpawnDistance = distance;
    }

    // =====================================================
    // SPAWN OBSTACLE
    // =====================================================

    private void SpawnObstacle()
    {
        GameObject selectedPrefab = SelectObstacle();

        if (selectedPrefab == null) return;

        GameObject obstacle = Instantiate(selectedPrefab);

        obstacle.transform.position += transform.position;

        lastSpawnedObject = selectedPrefab;
        lastObstaclePosition = obstacle.transform.position;

        hasSpawnedObstacle = true;
    }

    // =====================================================
    // SELECT OBSTACLE
    // =====================================================

    private GameObject SelectObstacle()
    {
        if (objects == null || objects.Length == 0)
        {
            return null;
        }

        float totalChance = 0f;

        foreach (SpawnableObject obj in objects)
        {
            totalChance += obj.spawnChance;
        }

        if (totalChance <= 0f)
        {
            return null;
        }

        for (int attempt = 0; attempt < 10; attempt++)
        {
            float random = Random.Range(0f, totalChance);

            foreach (SpawnableObject obj in objects)
            {
                if (random < obj.spawnChance)
                {
                    // Không spawn liên tục cùng 1 obstacle
                    if (objects.Length > 1 &&
                        obj.prefab == lastSpawnedObject)
                    {
                        break;
                    }

                    return obj.prefab;
                }

                random -= obj.spawnChance;
            }
        }

        return objects[Random.Range(0, objects.Length)].prefab;
    }

    // =====================================================
    // SPAWN ITEM
    // =====================================================

    private void SpawnRandomItem()
    {
        if (!hasSpawnedObstacle) return;

        GameObject selectedItem = SelectItem();

        if (selectedItem == null) return;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            float randomDistance = Random.Range(
                minDistanceAfterObstacle,
                maxDistanceAfterObstacle
            );

            float randomHeight = Random.Range(
                minItemHeight,
                maxItemHeight
            );

            Vector3 spawnPosition = lastObstaclePosition;

            spawnPosition.x += randomDistance;
            spawnPosition.y = randomHeight;
            spawnPosition.z = 0f;

            // =========================================
            // CHECK COLLISION
            // =========================================

            bool blocked = Physics2D.OverlapCircle(
                spawnPosition,
                itemCheckRadius,
                LayerMask.GetMask("Obstacle")
            );

            // =========================================
            // SAFE POSITION FOUND
            // =========================================

            if (!blocked)
            {
                Instantiate(
                    selectedItem,
                    spawnPosition,
                    Quaternion.identity,
                    itemParent
                );

                return;
            }
        }
    }

    // =====================================================
    // SELECT ITEM
    // =====================================================

    private GameObject SelectItem()
    {
        if (items == null || items.Length == 0)
        {
            return null;
        }

        float totalChance = 0f;

        foreach (SpawnableItem item in items)
        {
            totalChance += item.spawnChance;
        }

        if (totalChance <= 0f)
        {
            return null;
        }

        float random = Random.Range(0f, totalChance);

        foreach (SpawnableItem item in items)
        {
            if (random < item.spawnChance)
            {
                return item.prefab;
            }

            random -= item.spawnChance;
        }

        return null;
    }
}