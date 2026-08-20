using System;
using System.Collections.Generic;
using UnityEngine;

namespace StopDefence.GameData
{
    public enum EnemyType
    {
        Melee,
        Ranged,
        Boss
    }

    [Serializable]
    public sealed class EnemyLevelData
    {
        [SerializeField, Min(1)] private int level;
        [SerializeField, Min(0f)] private float hp;
        [SerializeField, Min(0f)] private float attack;
        [SerializeField, Min(0f)] private float attackSpeed;
        [SerializeField, Min(0f)] private float attackRange;
        [SerializeField, Min(0f)] private float moveSpeed;
        [SerializeField, Min(0)] private int dropCoin;

        public int Level => level;
        public float Hp => hp;
        public float Attack => attack;
        public float AttackSpeed => attackSpeed;
        public float AttackRange => attackRange;
        public float MoveSpeed => moveSpeed;
        public int DropCoin => dropCoin;

        public EnemyLevelData(
            int level,
            float hp,
            float attack,
            float attackSpeed,
            float attackRange,
            float moveSpeed,
            int dropCoin)
        {
            this.level = level;
            this.hp = hp;
            this.attack = attack;
            this.attackSpeed = attackSpeed;
            this.attackRange = attackRange;
            this.moveSpeed = moveSpeed;
            this.dropCoin = dropCoin;
        }
    }

    [Serializable]
    public sealed class EnemyData
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private EnemyType enemyType;
        [SerializeField] private GameObject prefab;
        [SerializeField] private List<EnemyLevelData> levels;

        public string Id => id;
        public string DisplayName => displayName;
        public EnemyType Type => enemyType;
        public GameObject Prefab => prefab;
        public IReadOnlyList<EnemyLevelData> Levels => levels;

        public EnemyData(
            string id,
            string displayName,
            EnemyType enemyType,
            GameObject prefab,
            IEnumerable<EnemyLevelData> levels)
        {
            this.id = id;
            this.displayName = displayName;
            this.enemyType = enemyType;
            this.prefab = prefab;
            this.levels = new List<EnemyLevelData>(levels);
        }

        public EnemyLevelData GetLevel(int level)
        {
            int index = level - 1;
            if (levels == null || index < 0 || index >= levels.Count)
            {
                return null;
            }

            EnemyLevelData levelData = levels[index];
            return levelData.Level == level ? levelData : null;
        }
    }

    [Serializable]
    public sealed class EnemySpawnData
    {
        [SerializeField, Min(0f)] private float time;
        [SerializeField] private string enemyId;
        [SerializeField, Min(1)] private int level;
        [SerializeField, Min(1)] private int spawnPoint;
        [SerializeField, Min(1)] private int count;
        [SerializeField, Min(0f)] private float interval;

        public float Time => time;
        public string EnemyId => enemyId;
        public int Level => level;
        public int SpawnPoint => spawnPoint;
        public int Count => count;
        public float Interval => interval;

        public EnemySpawnData(
            float time,
            string enemyId,
            int level,
            int spawnPoint,
            int count,
            float interval)
        {
            this.time = time;
            this.enemyId = enemyId;
            this.level = level;
            this.spawnPoint = spawnPoint;
            this.count = count;
            this.interval = interval;
        }
    }

    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Stop Defence/Game Data/Enemy Database")]
    public sealed class EnemyDatabase : ScriptableObject
    {
        [SerializeField] private List<EnemyData> enemies = new List<EnemyData>();
        [SerializeField] private List<EnemySpawnData> spawnSchedule = new List<EnemySpawnData>();
        [NonSerialized] private Dictionary<string, EnemyData> enemyById;

        public IReadOnlyList<EnemyData> Enemies => enemies;
        public IReadOnlyList<EnemySpawnData> SpawnSchedule => spawnSchedule;

        public bool TryGetEnemy(string id, out EnemyData enemy)
        {
            if (string.IsNullOrEmpty(id))
            {
                enemy = null;
                return false;
            }

            if (enemyById == null)
            {
                RebuildLookup();
            }

            return enemyById.TryGetValue(id, out enemy);
        }

        public void ReplaceData(
            IEnumerable<EnemyData> enemyValues,
            IEnumerable<EnemySpawnData> spawnValues)
        {
            enemies = new List<EnemyData>(enemyValues);
            spawnSchedule = new List<EnemySpawnData>(spawnValues);
            RebuildLookup();
        }

        private void OnEnable()
        {
            RebuildLookup();
        }

        private void RebuildLookup()
        {
            enemyById = new Dictionary<string, EnemyData>(
                enemies.Count,
                StringComparer.OrdinalIgnoreCase);

            foreach (EnemyData enemy in enemies)
            {
                enemyById[enemy.Id] = enemy;
            }
        }
    }
}
