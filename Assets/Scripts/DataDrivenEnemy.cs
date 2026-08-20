using System.Collections;
using StopDefence.GameData;
using UnityEngine;

public sealed class DataDrivenEnemy : Enemy.Enemy
{
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int DieTrigger = Animator.StringToHash("Die");
    private static readonly WaitForSeconds DeathAnimationDelay = new(1.5f);

    [SerializeField] private Animator animator;
    [SerializeField] private Enemy.Projectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    private EnemyDatabase database;
    private string enemyId;
    private Player rewardTarget;
    private BattleObjectPool objectPool;
    private Collider2D[] enemyColliders;

    public Enemy.Projectile ProjectilePrefab => projectilePrefab;

    private void Awake()
    {
        enemyColliders = GetComponentsInChildren<Collider2D>(true);
    }

    public void Initialize(
        EnemyDatabase enemyDatabase,
        string id,
        int level,
        Player target,
        BattleObjectPool pool)
    {
        database = enemyDatabase;
        enemyId = id;
        rewardTarget = target;
        objectPool = pool;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            enemyCollider.enabled = true;
        }

        SetLevel(level);
        PrepareForSpawn(target);
    }

    protected override void UpdateStat()
    {
        if (database == null || !database.TryGetEnemy(enemyId, out EnemyData enemy))
        {
            return;
        }

        EnemyLevelData levelData = enemy.GetLevel(stat.level);
        if (levelData == null)
        {
            return;
        }

        stat.hp = levelData.Hp;
        stat.attackDamage = levelData.Attack;
        stat.attackSpeed = levelData.AttackSpeed;
        stat.attackRange = levelData.AttackRange;
        stat.moveSpeed = levelData.MoveSpeed;
        stat.dropCoin = levelData.DropCoin;
        stat.experience = levelData.Experience;
    }

    protected override void Attack(Player player)
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackTrigger);
        }

        if (projectilePrefab == null)
        {
            player.TakeDamage(stat.attackDamage);
            return;
        }

        Enemy.Projectile projectile = objectPool.GetProjectile(
            projectilePrefab,
            projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position,
            Quaternion.identity);
        projectile.Init(stat.attackDamage, player, objectPool);
    }

    protected override void Die()
    {
        if (rewardTarget != null)
        {
            rewardTarget.GainExperience(stat.experience);
        }

        if (animator == null)
        {
            objectPool.ReleaseEnemy(this);
            return;
        }

        animator.SetTrigger(DieTrigger);
        foreach (Collider2D enemyCollider in enemyColliders)
        {
            enemyCollider.enabled = false;
        }

        StartCoroutine(ReleaseAfterDeathAnimation());
    }

    private IEnumerator ReleaseAfterDeathAnimation()
    {
        yield return DeathAnimationDelay;
        objectPool.ReleaseEnemy(this);
    }
}
