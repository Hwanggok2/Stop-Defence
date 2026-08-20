using System.Collections;
using StopDefence.GameData;
using UnityEngine;

public sealed class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyDatabase database;
    [SerializeField] private Transform spawnRoot;
    [SerializeField] private Player target;
    [SerializeField, Min(0)] private int projectilePrewarmCount = 12;

    private float elapsedTime;
    private int nextSpawnIndex;
    private Transform[] spawnPoints;
    private BattleObjectPool objectPool;

    private void Awake()
    {
        objectPool = new BattleObjectPool(transform);
        CacheSpawnPoints();
        PrewarmPools();
    }

    private void Update()
    {
        if (database == null || spawnRoot == null || target == null)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        while (nextSpawnIndex < database.SpawnSchedule.Count &&
               database.SpawnSchedule[nextSpawnIndex].Time <= elapsedTime)
        {
            StartCoroutine(SpawnGroup(database.SpawnSchedule[nextSpawnIndex]));
            nextSpawnIndex++;
        }
    }

    private IEnumerator SpawnGroup(EnemySpawnData spawnData)
    {
        WaitForSeconds interval = spawnData.Interval > 0f
            ? new WaitForSeconds(spawnData.Interval)
            : null;

        for (int index = 0; index < spawnData.Count; index++)
        {
            SpawnOne(spawnData);

            if (index + 1 < spawnData.Count && interval != null)
            {
                yield return interval;
            }
        }
    }

    private void SpawnOne(EnemySpawnData spawnData)
    {
        if (spawnPoints == null)
        {
            CacheSpawnPoints();
        }

        int childIndex = spawnData.SpawnPoint - 1;
        if (childIndex < 0 || childIndex >= spawnPoints.Length)
        {
            Debug.LogError(
                $"[EnemySpawner] SpawnPoint {spawnData.SpawnPoint} does not exist under '{spawnRoot.name}'.",
                this);
            return;
        }

        if (!database.TryGetEnemy(spawnData.EnemyId, out EnemyData enemy) ||
            enemy.Prefab == null)
        {
            Debug.LogError(
                $"[EnemySpawner] Enemy '{spawnData.EnemyId}' has no registered prefab.",
                this);
            return;
        }

        Transform spawnPoint = spawnPoints[childIndex];
        DataDrivenEnemy enemyController = objectPool.GetEnemy(
            enemy.Prefab,
            spawnPoint.position,
            spawnPoint.rotation);
        enemyController.name = $"{enemy.Id}_Lv{spawnData.Level}";
        enemyController.Initialize(database, enemy.Id, spawnData.Level, target, objectPool);
    }

    private void PrewarmPools()
    {
        if (database == null)
        {
            return;
        }

        foreach (EnemyData enemy in database.Enemies)
        {
            if (enemy.Prefab == null)
            {
                continue;
            }

            int enemyCount = GetLargestSpawnGroup(enemy.Id);
            objectPool.PrewarmEnemy(enemy.Prefab, enemyCount);

            DataDrivenEnemy controller = enemy.Prefab.GetComponent<DataDrivenEnemy>();
            if (controller != null && controller.ProjectilePrefab != null)
            {
                objectPool.PrewarmProjectile(
                    controller.ProjectilePrefab,
                    projectilePrewarmCount);
            }
        }
    }

    private int GetLargestSpawnGroup(string enemyId)
    {
        int largestCount = 1;
        foreach (EnemySpawnData spawnData in database.SpawnSchedule)
        {
            if (spawnData.EnemyId == enemyId)
            {
                largestCount = Mathf.Max(largestCount, spawnData.Count);
            }
        }

        return largestCount;
    }

    private void CacheSpawnPoints()
    {
        if (spawnRoot == null)
        {
            spawnPoints = new Transform[0];
            return;
        }

        spawnPoints = new Transform[spawnRoot.childCount];
        for (int index = 0; index < spawnPoints.Length; index++)
        {
            spawnPoints[index] = spawnRoot.GetChild(index);
        }
    }
}
