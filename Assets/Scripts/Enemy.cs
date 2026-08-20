using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] protected EnemyStat stat;
    [SerializeField] private Player player;

    protected bool isInAttackRange;
    
    private float attackTimer;
    
    public void SetLevel(int level)
    {
        stat.level = level;
        UpdateStat();
    }

    protected abstract void UpdateStat();

    protected abstract void Attack(Player player);
    
    private void OnValidate()
    {
        UpdateStat();
    }
    
    public void SetTarget(Player target)
    {
        player = target;
    }

    public void TakeDamage(float amount)
    {
        stat.hp -= amount;
    }

    public void HealHp(float amount)
    {
        stat.hp += amount;
    }

    protected virtual void Update()
    {
        if (player == null)
        {
            return;
        }

        Vector3 targetPosition = player.transform.position;
        UpdateInAttackRange(targetPosition);
        
        if (isInAttackRange)
        {
            UpdateAttackTimer();
            return;
        }
        MoveToTarget(targetPosition);
    }

    private void MoveToTarget(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            stat.moveSpeed * Time.deltaTime
        );
    }

    private void UpdateInAttackRange(Vector3 targetPosition)
    {
        var distance = Vector3.Distance(targetPosition, transform.position);
        isInAttackRange = distance <= stat.attackRange;
    }

    private void UpdateAttackTimer()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f / stat.attackSpeed)
        {
            attackTimer = 0f;
            Attack(player);
        }
    }
}
