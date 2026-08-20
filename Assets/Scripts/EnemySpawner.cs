using System.Collections;
using StopDefence.GameData;
using UnityEngine;

public sealed class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyDatabase database;
    [SerializeField] private Transform spawnRoot;
    [SerializeField] private Player target;

    private float elapsedTime;
    private int nextSpawnIndex;
    private Transform[] spawnPoints;

    private void Awake()
    {
        CacheSpawnPoints();
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
        for (int index = 0; index < spawnData.Count; index++)
        {
            SpawnOne(spawnData);

            if (index + 1 < spawnData.Count && spawnData.Interval > 0f)
            {
                yield return new WaitForSeconds(spawnData.Interval);
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
        GameObject instance = Instantiate(
            enemy.Prefab,
            spawnPoint.position,
            spawnPoint.rotation);
        instance.name = $"{enemy.Id}_Lv{spawnData.Level}";

        DataDrivenEnemy enemyController = instance.GetComponent<DataDrivenEnemy>();
        if (enemyController == null)
        {
            Debug.LogError(
                $"[EnemySpawner] Prefab '{enemy.Prefab.name}' has no DataDrivenEnemy component.",
                instance);
            Destroy(instance);
            return;
        }

        enemyController.Initialize(database, enemy.Id, spawnData.Level, target);
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
