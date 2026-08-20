using System;
using System.Collections.Generic;
using UnityEngine;

namespace StopDefence.GameData
{
    [Serializable]
    public sealed class PlayerLevelData
    {
        [SerializeField, Min(0)] private int level;
        [SerializeField, Min(1)] private int requiredExperience;
        [SerializeField, Min(1f)] private float maxHp;

        public int Level => level;
        public int RequiredExperience => requiredExperience;
        public float MaxHp => maxHp;

        public PlayerLevelData(int level, int requiredExperience, float maxHp)
        {
            this.level = level;
            this.requiredExperience = requiredExperience;
            this.maxHp = maxHp;
        }
    }

    [CreateAssetMenu(fileName = "PlayerDatabase", menuName = "Stop Defence/Game Data/Player Database")]
    public sealed class PlayerDatabase : ScriptableObject
    {
        [SerializeField] private List<PlayerLevelData> levels = new List<PlayerLevelData>();

        public int MaxLevel => levels.Count - 1;

        public PlayerLevelData GetLevel(int level)
        {
            if (level < 0 || level >= levels.Count)
            {
                return null;
            }

            PlayerLevelData levelData = levels[level];
            return levelData.Level == level ? levelData : null;
        }

        public void ReplaceLevels(IEnumerable<PlayerLevelData> values)
        {
            levels = new List<PlayerLevelData>(values);
        }
    }
}
