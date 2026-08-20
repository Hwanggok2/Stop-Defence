using UnityEngine;

namespace Enemy
{
    public class Goblin : Enemy
    {
        [SerializeField] GameObject projectilePrefab;
        protected override void UpdateStat()
        {
            if (stat.level <= 0) return;
        
            stat.hp = stat.level * 2 + 70;
            stat.attackDamage = stat.level * 3 + 5;
            stat.attackSpeed = 0.5f + stat.level * stat.level * 0.0001f;
            stat.attackRange = 8;
            stat.moveSpeed = 2.8f + stat.level * 0.2f;
            stat.dropCoin = stat.level * 3 + 7;
        }

        protected override void Attack(Player player)
        {
            GameObject obj = Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity
            );

            if (!obj.TryGetComponent<Projectile>(out var projectile)) return;
            
            projectile.Init(stat.attackDamage, player.transform);
        }
    }
}
