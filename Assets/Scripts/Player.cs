using System;
using StopDefence.GameData;
using UnityEngine;

public sealed class Player : MonoBehaviour
{
    [SerializeField] private PlayerDatabase database;
    [SerializeField, Min(0)] private int startingLevel;
    [SerializeField, Min(1f)] private float fallbackMaxHp = 100f;

    private float maxHpBonus;

    public event Action StatusChanged;
    public event Action Died;
    public event Action<int> LevelGained;

    public int Level { get; private set; }
    public int Experience { get; private set; }
    public int RequiredExperience { get; private set; }
    public float Hp { get; private set; }
    public float MaxHp { get; private set; }
    public float DamageReduction { get; private set; }
    public float HealingReceivedBonus { get; private set; }
    public int AttackPowerLevel { get; private set; }
    public bool IsDead => Hp <= 0f;
    public float HealthNormalized => MaxHp > 0f ? Hp / MaxHp : 0f;
    public float ExperienceNormalized =>
        database == null
            ? 0f
            : (IsMaxLevel
                ? 1f
                : (RequiredExperience > 0 ? (float)Experience / RequiredExperience : 0f));

    private bool IsMaxLevel => database != null && Level >= database.MaxLevel;

    private void Awake()
    {
        if (database == null || database.GetLevel(startingLevel) == null)
        {
            Level = 0;
            MaxHp = fallbackMaxHp;
            Hp = MaxHp;
            return;
        }

        Level = startingLevel;
        ApplyLevel(database.GetLevel(Level), false);
        Hp = MaxHp;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || IsDead)
        {
            return;
        }

        float appliedDamage = amount * Mathf.Max(0f, 1f - DamageReduction);
        Hp = Mathf.Max(0f, Hp - appliedDamage);
        StatusChanged?.Invoke();

        if (IsDead)
        {
            Died?.Invoke();
        }
    }

    public void HealHp(float amount)
    {
        if (amount <= 0f || IsDead)
        {
            return;
        }

        float appliedHealing = amount * (1f + HealingReceivedBonus);
        Hp = Mathf.Min(MaxHp, Hp + appliedHealing);
        StatusChanged?.Invoke();
    }

    public bool CanApplyStatUpgrade(SkillData skill)
    {
        if (skill == null ||
            !skill.Enabled ||
            skill.Category != SkillCategory.StatUpgrade ||
            skill.StatType == PlayerStatType.None ||
            skill.StatValue <= 0f)
        {
            return false;
        }

        float currentValue = GetStatUpgradeValue(skill.StatType);
        return skill.StatCap <= 0f || currentValue < skill.StatCap;
    }

    public bool ApplyStatUpgrade(SkillData skill)
    {
        if (!CanApplyStatUpgrade(skill))
        {
            return false;
        }

        float currentValue = GetStatUpgradeValue(skill.StatType);
        float nextValue = skill.StatCap > 0f
            ? Mathf.Min(skill.StatCap, currentValue + skill.StatValue)
            : currentValue + skill.StatValue;
        float appliedValue = nextValue - currentValue;

        switch (skill.StatType)
        {
            case PlayerStatType.MaxHp:
                maxHpBonus = nextValue;
                MaxHp += appliedValue;
                Hp = Mathf.Min(MaxHp, Hp + appliedValue);
                break;
            case PlayerStatType.DamageReduction:
                DamageReduction = nextValue;
                break;
            case PlayerStatType.HealingReceived:
                HealingReceivedBonus = nextValue;
                break;
            case PlayerStatType.AttackPowerLevel:
                AttackPowerLevel = Mathf.RoundToInt(nextValue);
                break;
            default:
                return false;
        }

        StatusChanged?.Invoke();
        return true;
    }

    public void GainExperience(int amount)
    {
        if (amount <= 0 || IsDead || database == null || IsMaxLevel)
        {
            return;
        }

        Experience += amount;
        while (!IsMaxLevel && Experience >= RequiredExperience)
        {
            Experience -= RequiredExperience;
            Level++;
            ApplyLevel(database.GetLevel(Level), true);
            LevelGained?.Invoke(Level);
        }

        StatusChanged?.Invoke();
    }

    private void ApplyLevel(PlayerLevelData levelData, bool preserveHealth)
    {
        float previousMaxHp = MaxHp;
        MaxHp = levelData.MaxHp + maxHpBonus;
        RequiredExperience = levelData.RequiredExperience;

        if (preserveHealth)
        {
            Hp = Mathf.Min(MaxHp, Hp + MaxHp - previousMaxHp);
        }
    }

    private float GetStatUpgradeValue(PlayerStatType statType)
    {
        return statType switch
        {
            PlayerStatType.MaxHp => maxHpBonus,
            PlayerStatType.DamageReduction => DamageReduction,
            PlayerStatType.HealingReceived => HealingReceivedBonus,
            PlayerStatType.AttackPowerLevel => AttackPowerLevel,
            _ => 0f
        };
    }

}
