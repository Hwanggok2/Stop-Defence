using System;
using StopDefence.GameData;
using UnityEngine;

public sealed class Player : MonoBehaviour
{
    [SerializeField] private PlayerDatabase database;
    [SerializeField, Min(0)] private int startingLevel;
    [SerializeField, Min(1f)] private float fallbackMaxHp = 100f;

    public event Action StatusChanged;
    public event Action Died;

    public int Level { get; private set; }
    public int Experience { get; private set; }
    public int RequiredExperience { get; private set; }
    public float Hp { get; private set; }
    public float MaxHp { get; private set; }
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

        Hp = Mathf.Max(0f, Hp - amount);
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

        Hp = Mathf.Min(MaxHp, Hp + amount);
        StatusChanged?.Invoke();
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
        }

        StatusChanged?.Invoke();
    }

    private void ApplyLevel(PlayerLevelData levelData, bool preserveHealth)
    {
        float previousMaxHp = MaxHp;
        MaxHp = levelData.MaxHp;
        RequiredExperience = levelData.RequiredExperience;

        if (preserveHealth)
        {
            Hp = Mathf.Min(MaxHp, Hp + MaxHp - previousMaxHp);
        }
    }
}
