using UnityEngine;

namespace Enemy
{
    public abstract class Enemy : MonoBehaviour
    {
        [SerializeField] protected EnemyStat stat;
        [SerializeField] private Player player;

        protected bool isInAttackRange;
        protected bool isTarget;

        private float attackTimer;

        public void SetLevel(int level)
        {
            stat.level = level;
            UpdateStat();
        }

        protected virtual void Start()
        {
            SetLevel(1);
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
            UpdateInAttackRange();

            if (isInAttackRange)
            {
                UpdateAttackTimer();
                return;
            }
            
            if (isTarget)
                MoveToTarget();
            else
                Move();
        }

        private void Move()
        {
            transform.position += Vector3.left * stat.moveSpeed * Time.deltaTime;
        }
        private void MoveToTarget()
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.transform.position,
                stat.moveSpeed * Time.deltaTime
            );
        }

        private void UpdateInAttackRange()
        {
            var distance = Vector3.Distance(player.transform.position, transform.position);
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
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == GameManager.Instance.TargetArea)
                isTarget = true;
        }
    }
}
