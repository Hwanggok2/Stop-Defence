namespace Enemy
{
    public class Boss : Enemy
    {
        protected override void UpdateStat()
        {
            if (stat.level <= 0) return;

            stat.hp = stat.level * 400;
            stat.attackDamage = stat.level * 30;
            stat.attackSpeed = stat.level * 0.24f;
            stat.attackRange = 1;
            stat.moveSpeed = stat.level + 3;
            stat.dropCoin = stat.level * 100;
        }

        protected override void Attack(Player player)
        {
            player.TakeDamage(stat.attackDamage);
        }
    }
}
