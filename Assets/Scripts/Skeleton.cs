using UnityEngine;

public class Skeleton : Enemy
{
    private void Start()
    {
        SetLevel(1);
    }

    protected override void UpdateStat()
    {
        if (stat.level <= 0) return;
        
        stat.hp = stat.level * stat.level * 2 + 18;
        stat.attackDamage = Mathf.Log(stat.level, 2) * 5 + 20;
        stat.attackSpeed = .8f + stat.level * .01f;
        stat.attackRange = 1;
        stat.moveSpeed = 4.9f + stat.level * .1f;
        stat.dropCoin = stat.level + 9;
    }

    protected override void Attack(Player player)
    {
        player.TakeDamage(stat.attackDamage);
    }
}
