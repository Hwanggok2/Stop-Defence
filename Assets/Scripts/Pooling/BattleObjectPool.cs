using System.Collections.Generic;
using UnityEngine;

public sealed class BattleObjectPool
{
    private readonly Transform container;
    private readonly Dictionary<GameObject, Queue<DataDrivenEnemy>> enemyPools = new();
    private readonly Dictionary<DataDrivenEnemy, GameObject> enemyPrefabs = new();
    private readonly Dictionary<Enemy.Projectile, Queue<Enemy.Projectile>> projectilePools = new();
    private readonly Dictionary<Enemy.Projectile, Enemy.Projectile> projectilePrefabs = new();

    public BattleObjectPool(Transform container)
    {
        this.container = container;
    }

    public void PrewarmEnemy(GameObject prefab, int count)
    {
        Queue<DataDrivenEnemy> pool = GetEnemyPool(prefab);
        while (pool.Count < count)
        {
            DataDrivenEnemy enemy = CreateEnemy(prefab);
            enemy.gameObject.SetActive(false);
            pool.Enqueue(enemy);
        }
    }

    public DataDrivenEnemy GetEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        Queue<DataDrivenEnemy> pool = GetEnemyPool(prefab);
        DataDrivenEnemy enemy = TakeAvailable(pool) ?? CreateEnemy(prefab);
        enemy.transform.SetPositionAndRotation(position, rotation);
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    public void ReleaseEnemy(DataDrivenEnemy enemy)
    {
        GameObject prefab = enemyPrefabs[enemy];
        enemy.gameObject.SetActive(false);
        GetEnemyPool(prefab).Enqueue(enemy);
    }

    public void PrewarmProjectile(Enemy.Projectile prefab, int count)
    {
        Queue<Enemy.Projectile> pool = GetProjectilePool(prefab);
        while (pool.Count < count)
        {
            Enemy.Projectile projectile = CreateProjectile(prefab);
            projectile.gameObject.SetActive(false);
            pool.Enqueue(projectile);
        }
    }

    public Enemy.Projectile GetProjectile(
        Enemy.Projectile prefab,
        Vector3 position,
        Quaternion rotation)
    {
        Queue<Enemy.Projectile> pool = GetProjectilePool(prefab);
        Enemy.Projectile projectile = TakeAvailable(pool) ?? CreateProjectile(prefab);
        projectile.transform.SetPositionAndRotation(position, rotation);
        projectile.gameObject.SetActive(true);
        return projectile;
    }

    public void ReleaseProjectile(Enemy.Projectile projectile)
    {
        Enemy.Projectile prefab = projectilePrefabs[projectile];
        projectile.gameObject.SetActive(false);
        GetProjectilePool(prefab).Enqueue(projectile);
    }

    private Queue<DataDrivenEnemy> GetEnemyPool(GameObject prefab)
    {
        if (!enemyPools.TryGetValue(prefab, out Queue<DataDrivenEnemy> pool))
        {
            pool = new Queue<DataDrivenEnemy>();
            enemyPools.Add(prefab, pool);
        }

        return pool;
    }

    private Queue<Enemy.Projectile> GetProjectilePool(Enemy.Projectile prefab)
    {
        if (!projectilePools.TryGetValue(prefab, out Queue<Enemy.Projectile> pool))
        {
            pool = new Queue<Enemy.Projectile>();
            projectilePools.Add(prefab, pool);
        }

        return pool;
    }

    private DataDrivenEnemy CreateEnemy(GameObject prefab)
    {
        GameObject instance = Object.Instantiate(prefab, container);
        DataDrivenEnemy enemy = instance.GetComponent<DataDrivenEnemy>();
        enemyPrefabs.Add(enemy, prefab);
        return enemy;
    }

    private Enemy.Projectile CreateProjectile(Enemy.Projectile prefab)
    {
        Enemy.Projectile projectile = Object.Instantiate(prefab, container);
        projectilePrefabs.Add(projectile, prefab);
        return projectile;
    }

    private static T TakeAvailable<T>(Queue<T> pool) where T : Component
    {
        while (pool.Count > 0)
        {
            T instance = pool.Dequeue();
            if (instance != null)
            {
                return instance;
            }
        }

        return null;
    }
}
