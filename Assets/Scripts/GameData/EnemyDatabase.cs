using System;
using System.Collections.Generic;
using UnityEngine;

namespace StopDefence.GameData
{
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
        [SerializeField] private List<EnemyLevelData> levels;

        public string Id => id;
        public string DisplayName => displayName;
        public IReadOnlyList<EnemyLevelData> Levels => levels;

        public EnemyData(string id, string displayName, IEnumerable<EnemyLevelData> levels)
        {
            this.id = id;
            this.displayName = displayName;
            this.levels = new List<EnemyLevelData>(levels);
        }

        public EnemyLevelData GetLevel(int level)
        {
            return levels.Find(value => value.Level == level);
        }
    }

    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Stop Defence/Game Data/Enemy Database")]
    public sealed class EnemyDatabase : ScriptableObject
    {
        [SerializeField] private List<EnemyData> enemies = new List<EnemyData>();

        public IReadOnlyList<EnemyData> Enemies => enemies;

        public bool TryGetEnemy(string id, out EnemyData enemy)
        {
            enemy = enemies.Find(value =>
                string.Equals(value.Id, id, StringComparison.OrdinalIgnoreCase));
            return enemy != null;
        }

        public void ReplaceEnemies(IEnumerable<EnemyData> values)
        {
            enemies = new List<EnemyData>(values);
        }
    }
}
