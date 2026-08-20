using System.Collections;
using System.Collections.Generic;
using StopDefence.GameData;
using StopDefence.Vfx;
using UnityEngine;

public sealed class CastSkill : MonoBehaviour
{
    private const string FireballId = "skill_001";
    private const string EarthMagicId = "skill_002";
    private const string ChainLightningId = "skill_003";
    private const string NailDrivingId = "skill_004";
    private const string RepairId = "skill_005";
    private const string PlagueMagicId = "skill_006";
    private const string IceLanceId = "skill_007";
    private const string MegaExplosionId = "skill_008";
    private const string RoundingThirtyId = "skill_009";
    private const string CaffeineId = "skill_011";
    private const float IceLanceTravelDistance = 40f;
    private const float IceLanceProjectileSpeed = 25f;

    [Header("Runtime References")]
    [SerializeField] private SkillDatabase database;
    [SerializeField] private Player player;
    [SerializeField] private Transform effectsRoot;

    [Header("Effect Prefabs")]
    [SerializeField] private SkillParticleEffect fireballEffectPrefab;
    [SerializeField] private SkillParticleEffect earthMagicEffectPrefab;
    [SerializeField] private SkillParticleEffect nailDrivingEffectPrefab;
    [SerializeField] private SkillParticleEffect plagueMagicEffectPrefab;
    [SerializeField] private SkillParticleEffect iceLanceEffectPrefab;
    [SerializeField] private SkillParticleEffect megaExplosionEffectPrefab;
    [SerializeField] private ChainLightningBolt chainLightningEffectPrefab;

    [Header("Skill Balance (Database Fallbacks)")]
    [SerializeField, Min(0f)] private float fireballDamage = 10f;
    [SerializeField, Min(0f)] private float fireballRange = 5f;
    [SerializeField, Min(0f)] private float burnDamagePerSecond = 2f;
    [SerializeField, Min(0f)] private float burnDuration = 3f;
    [SerializeField, Min(0f)] private float burnDurationPerAttackLevel = 0.2f;
    [SerializeField, Min(0f)] private float earthMagicDamage = 15f;
    [SerializeField, Min(0f)] private float earthKnockbackDistance = 3f;
    [SerializeField, Min(0f)] private float earthKnockbackPerAttackLevel = 0.2f;
    [SerializeField, Min(0f)] private float chainLightningDamage = 3f;
    [SerializeField, Min(1)] private int maxChainTargets = 7;
    [SerializeField, Min(0f)] private float nailDrivingDamage = 20f;
    [SerializeField, Min(0f)] private float repairAmount = 20f;
    [SerializeField, Min(0f)] private float repairPerAttackLevel = 2f;
    [SerializeField, Min(0f)] private float plagueDamagePerSecond = 5f;
    [SerializeField, Min(0f)] private float plagueDuration = 3f;
    [SerializeField, Min(0f)] private float plagueDurationPerAttackLevel = 0.2f;
    [SerializeField, Min(0f)] private float plagueRadius = 3f;
    [SerializeField, Min(0f)] private float iceLanceDamage = 5f;
    [SerializeField, Min(0f)] private float iceLanceHitRadius = 1.25f;
    [SerializeField, Range(0f, 0.95f)] private float iceSlowRate = 0.3f;
    [SerializeField, Min(0f)] private float iceSlowPerAttackLevel = 0.01f;
    [SerializeField, Min(0f)] private float iceSlowDuration = 1f;
    [SerializeField, Min(0f)] private float iceSlowDurationPerAttackLevel = 0.1f;
    [SerializeField, Min(0f)] private float megaExplosionDamage = 100f;
    [SerializeField, Min(0f)] private float roundingThirtyDamage = 30f;
    [SerializeField, Min(1f)] private float caffeineDamageMultiplier = 1.3f;
    [SerializeField, Min(0f)] private float caffeineDuration = 5f;

    private float activeDamageBuff = 1f;
    private Coroutine caffeineRoutine;

    private void Awake()
    {
        ResolveRuntimeReferences();
    }

    public void Cast(int index)
    {
        Cast($"skill_{index:000}", TimingJudgement.Perfect);
    }

    public bool Cast(string skillId, TimingJudgement judgement)
    {
        ResolveRuntimeReferences();
        float damageMultiplier = GetDamageMultiplier(judgement) * activeDamageBuff;

        bool castSucceeded = skillId switch
        {
            FireballId => CastFireball(damageMultiplier),
            EarthMagicId => CastEarthMagic(damageMultiplier),
            ChainLightningId => CastChainLightning(damageMultiplier),
            NailDrivingId => CastNailDriving(damageMultiplier),
            RepairId => CastRepair(),
            PlagueMagicId => CastPlagueMagic(damageMultiplier),
            IceLanceId => CastIceLance(damageMultiplier),
            MegaExplosionId => CastMegaExplosion(damageMultiplier),
            RoundingThirtyId => CastRoundingThirty(damageMultiplier),
            CaffeineId => CastCaffeine(),
            _ => false
        };

        if (castSucceeded)
        {
            Debug.Log(
                $"[CastSkill] {skillId} cast: {judgement}, damage x{damageMultiplier:0.##}",
                this);
        }
        else if (!string.IsNullOrWhiteSpace(skillId) &&
                 (database == null || !database.TryGetSkill(skillId, out _)))
        {
            Debug.LogWarning($"[CastSkill] Unknown skill id '{skillId}'.", this);
        }

        return castSucceeded;
    }

    public float GetDamageMultiplier(TimingJudgement judgement)
    {
        return database != null &&
               database.TryGetDamageMultiplier(judgement, out float multiplier)
            ? multiplier
            : 1f;
    }

    private float GetDamage(string skillId, float fallback)
    {
        return database != null && database.TryGetSkill(skillId, out SkillData skill)
            ? skill.CalculateDamage(GetAttackPowerLevel())
            : fallback;
    }

    private float GetDotDamage(string skillId, float fallback)
    {
        return database != null && database.TryGetSkill(skillId, out SkillData skill)
            ? skill.CalculateDotDamage(GetAttackPowerLevel())
            : fallback;
    }

    private int GetAttackPowerLevel()
    {
        return player != null ? player.AttackPowerLevel : 0;
    }

    private float ScaleByAttackPowerLevel(float baseValue, float valuePerLevel)
    {
        return baseValue + valuePerLevel * GetAttackPowerLevel();
    }

    private bool CastFireball(float multiplier)
    {
        Enemy.Enemy target = FindNearestEnemy(GetOriginPosition());
        if (target == null)
        {
            return false;
        }

        Vector3 impactPosition = target.transform.position;
        SpawnEffect(fireballEffectPrefab, impactPosition);
        StartCoroutine(ApplyFireballImpact(impactPosition, multiplier));
        return true;
    }

    private IEnumerator ApplyFireballImpact(Vector3 position, float multiplier)
    {
        yield return new WaitForSeconds(0.48f);

        foreach (Enemy.Enemy enemy in FindEnemiesInRange(position, fireballRange))
        {
            enemy.TakeDamage(
                GetDamage(FireballId, fireballDamage) * multiplier,
                FireballId);
            enemy.ApplyDamageOverTime(
                GetDotDamage(FireballId, burnDamagePerSecond) * multiplier,
                ScaleByAttackPowerLevel(burnDuration, burnDurationPerAttackLevel),
                FireballId);
        }
    }

    private bool CastEarthMagic(float multiplier)
    {
        Enemy.Enemy target = FindNearestEnemy(GetOriginPosition());
        if (target == null)
        {
            return false;
        }

        Vector3 impactPosition = target.transform.position;
        SpawnEffect(earthMagicEffectPrefab, impactPosition);
        StartCoroutine(ApplyEarthMagicImpact(impactPosition, multiplier));
        return true;
    }

    private IEnumerator ApplyEarthMagicImpact(Vector3 origin, float multiplier)
    {
        yield return new WaitForSeconds(0.04f);

        foreach (Enemy.Enemy enemy in GetActiveEnemies())
        {
            Vector3 direction = enemy.transform.position - origin;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = Vector3.right;
            }

            enemy.TakeDamage(
                GetDamage(EarthMagicId, earthMagicDamage) * multiplier,
                EarthMagicId);
            enemy.Knockback(
                direction,
                ScaleByAttackPowerLevel(
                    earthKnockbackDistance,
                    earthKnockbackPerAttackLevel));
        }
    }

    private bool CastChainLightning(float multiplier)
    {
        var hitEnemies = new HashSet<Enemy.Enemy>();
        Vector3 origin = GetOriginPosition();

        for (int index = 0; index < maxChainTargets; index++)
        {
            Enemy.Enemy target = FindNearestEnemy(origin, hitEnemies);
            if (target == null)
            {
                break;
            }

            Vector3 targetPosition = target.transform.position;
            if (chainLightningEffectPrefab != null)
            {
                ChainLightningBolt bolt = Instantiate(
                    chainLightningEffectPrefab,
                    effectsRoot);
                bolt.Play(origin, targetPosition);
            }

            target.TakeDamage(
                GetDamage(ChainLightningId, chainLightningDamage) * multiplier,
                ChainLightningId);
            hitEnemies.Add(target);
            origin = targetPosition;
        }

        return hitEnemies.Count > 0;
    }

    private bool CastNailDriving(float multiplier)
    {
        Enemy.Enemy target = FindNearestEnemy(GetOriginPosition());
        if (target == null)
        {
            return false;
        }

        SpawnEffect(nailDrivingEffectPrefab, target.transform.position);
        StartCoroutine(ApplySingleTargetDamage(
            target,
            GetDamage(NailDrivingId, nailDrivingDamage) * multiplier,
            0.45f,
            NailDrivingId));
        return true;
    }

    private bool CastRepair()
    {
        if (player == null || player.IsDead)
        {
            return false;
        }

        player.HealHp(ScaleByAttackPowerLevel(repairAmount, repairPerAttackLevel));
        return true;
    }

    private bool CastPlagueMagic(float multiplier)
    {
        Enemy.Enemy target = FindNearestEnemy(GetOriginPosition());
        if (target == null)
        {
            return false;
        }

        Vector3 impactPosition = target.transform.position;
        SpawnEffect(plagueMagicEffectPrefab, impactPosition);
        StartCoroutine(ApplyPlagueImpact(impactPosition, multiplier));
        return true;
    }

    private IEnumerator ApplyPlagueImpact(Vector3 position, float multiplier)
    {
        yield return new WaitForSeconds(0.54f);

        foreach (Enemy.Enemy enemy in FindEnemiesInRange(position, plagueRadius))
        {
            enemy.ApplyDamageOverTime(
                GetDotDamage(PlagueMagicId, plagueDamagePerSecond) * multiplier,
                ScaleByAttackPowerLevel(
                    plagueDuration,
                    plagueDurationPerAttackLevel),
                PlagueMagicId);
        }
    }

    private bool CastIceLance(float multiplier)
    {
        Vector3 origin = GetOriginPosition();
        List<Enemy.Enemy> targets = GetActiveEnemies();
        Enemy.Enemy target = FindNearestEnemy(origin);
        if (target == null)
        {
            return false;
        }

        Vector3 direction = target.transform.position - origin;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            direction = Vector3.right;
        }
        else
        {
            direction.Normalize();
        }

        if (iceLanceEffectPrefab != null)
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);
            Vector3 effectPosition = origin + direction * IceLanceTravelDistance;
            Instantiate(iceLanceEffectPrefab, effectPosition, rotation, effectsRoot);
        }

        StartCoroutine(ApplyIceLancePiercing(targets, origin, direction, multiplier));
        return true;
    }

    private IEnumerator ApplyIceLancePiercing(
        List<Enemy.Enemy> targets,
        Vector3 origin,
        Vector3 direction,
        float multiplier)
    {
        var hitEnemies = new HashSet<Enemy.Enemy>();
        float damage = GetDamage(IceLanceId, iceLanceDamage) * multiplier;
        float sqrHitRadius = iceLanceHitRadius * iceLanceHitRadius;
        float traveledDistance = 0f;

        while (traveledDistance < IceLanceTravelDistance)
        {
            float nextDistance = Mathf.Min(
                traveledDistance + IceLanceProjectileSpeed * Time.deltaTime,
                IceLanceTravelDistance);
            Vector3 segmentStart = origin + direction * traveledDistance;
            Vector3 segmentEnd = origin + direction * nextDistance;

            foreach (Enemy.Enemy enemy in targets)
            {
                if (enemy == null || enemy.IsDead ||
                    !enemy.gameObject.activeInHierarchy || hitEnemies.Contains(enemy) ||
                    SqrDistanceToSegment(enemy.transform.position, segmentStart, segmentEnd) >
                    sqrHitRadius)
                {
                    continue;
                }

                hitEnemies.Add(enemy);
                enemy.TakeDamage(damage, IceLanceId);
                enemy.ApplySlow(
                    ScaleByAttackPowerLevel(iceSlowRate, iceSlowPerAttackLevel),
                    ScaleByAttackPowerLevel(
                        iceSlowDuration,
                        iceSlowDurationPerAttackLevel));
            }

            traveledDistance = nextDistance;
            yield return null;
        }
    }

    private static float SqrDistanceToSegment(
        Vector3 point,
        Vector3 segmentStart,
        Vector3 segmentEnd)
    {
        Vector3 segment = segmentEnd - segmentStart;
        float sqrLength = segment.sqrMagnitude;
        if (sqrLength <= Mathf.Epsilon)
        {
            return (point - segmentStart).sqrMagnitude;
        }

        float position = Mathf.Clamp01(
            Vector3.Dot(point - segmentStart, segment) / sqrLength);
        Vector3 nearestPoint = segmentStart + segment * position;
        return (point - nearestPoint).sqrMagnitude;
    }

    private bool CastMegaExplosion(float multiplier)
    {
        List<Enemy.Enemy> enemies = GetActiveEnemies();
        if (enemies.Count == 0)
        {
            return false;
        }

        Enemy.Enemy target = FindNearestEnemy(GetOriginPosition());
        if (target != null)
        {
            SpawnEffect(megaExplosionEffectPrefab, target.transform.position);
        }

        foreach (Enemy.Enemy enemy in enemies)
        {
            enemy.TakeDamage(
                GetDamage(MegaExplosionId, megaExplosionDamage) * multiplier,
                MegaExplosionId);
        }

        return true;
    }

    private bool CastRoundingThirty(float multiplier)
    {
        Enemy.Enemy target = FindNearestEnemy(GetOriginPosition());
        if (target == null)
        {
            return false;
        }

        target.TakeDamage(
            GetDamage(RoundingThirtyId, roundingThirtyDamage) * multiplier,
            RoundingThirtyId);
        return true;
    }

    private bool CastCaffeine()
    {
        if (player == null || player.IsDead)
        {
            return false;
        }

        if (caffeineRoutine != null)
        {
            StopCoroutine(caffeineRoutine);
        }

        caffeineRoutine = StartCoroutine(CaffeineRoutine());
        return true;
    }

    private IEnumerator CaffeineRoutine()
    {
        activeDamageBuff = caffeineDamageMultiplier;
        yield return new WaitForSeconds(caffeineDuration);
        activeDamageBuff = 1f;
        caffeineRoutine = null;
    }

    private static IEnumerator ApplySingleTargetDamage(
        Enemy.Enemy target,
        float damage,
        float delay,
        string skillId)
    {
        yield return new WaitForSeconds(delay);

        if (target != null && !target.IsDead && target.gameObject.activeInHierarchy)
        {
            target.TakeDamage(damage, skillId);
        }
    }

    private void SpawnEffect(SkillParticleEffect prefab, Vector3 position)
    {
        if (prefab != null)
        {
            Instantiate(prefab, position, Quaternion.identity, effectsRoot);
        }
    }

    private Enemy.Enemy FindNearestEnemy(
        Vector3 origin,
        ISet<Enemy.Enemy> excluded = null)
    {
        Enemy.Enemy nearest = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (Enemy.Enemy enemy in GetActiveEnemies())
        {
            if (excluded != null && excluded.Contains(enemy))
            {
                continue;
            }

            float sqrDistance = (enemy.transform.position - origin).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearest = enemy;
                nearestSqrDistance = sqrDistance;
            }
        }

        return nearest;
    }

    private static List<Enemy.Enemy> FindEnemiesInRange(Vector3 origin, float range)
    {
        float sqrRange = range * range;
        return GetActiveEnemies().FindAll(
            enemy => (enemy.transform.position - origin).sqrMagnitude <= sqrRange);
    }

    private static List<Enemy.Enemy> GetActiveEnemies()
    {
        var activeEnemies = new List<Enemy.Enemy>();
        foreach (Enemy.Enemy enemy in Object.FindObjectsByType<Enemy.Enemy>())
        {
            if (enemy != null &&
                enemy.gameObject.activeInHierarchy &&
                !enemy.IsDead)
            {
                activeEnemies.Add(enemy);
            }
        }

        return activeEnemies;
    }

    private Vector3 GetOriginPosition()
    {
        return player != null ? player.transform.position : transform.position;
    }

    private void ResolveRuntimeReferences()
    {
        if (player == null)
        {
            player = Object.FindFirstObjectByType<Player>();
        }
    }
}
