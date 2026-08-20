using StopDefence.GameData;
using UnityEngine;

public sealed class DataDrivenEnemy : Enemy
{
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");

    [SerializeField] private Animator animator;

    private EnemyDatabase database;
    private string enemyId;

    public void Initialize(
        EnemyDatabase enemyDatabase,
        string id,
        int level,
        Player target)
    {
        database = enemyDatabase;
        enemyId = id;
        SetLevel(level);

        if (target != null)
        {
            SetTarget(target);
        }

        enabled = target != null;
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
    }

    protected override void Attack(Player player)
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackTrigger);
        }

        player.TakeDamage(stat.attackDamage);
    }
}
