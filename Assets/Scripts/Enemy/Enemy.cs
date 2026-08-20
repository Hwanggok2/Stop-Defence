using UnityEngine;

namespace Enemy
{
    public abstract class Enemy : MonoBehaviour
    {
        [SerializeField] protected EnemyStat stat;
        [SerializeField] private Player player;

        protected bool isInAttackRange;

        private float attackTimer;
        private Transform targetTransform;
        private bool isDead;

        public void SetLevel(int level)
        {
            stat.level = level;
            UpdateStat();
        }

        protected virtual void Start()
        {
            if (stat.level <= 0)
            {
                SetLevel(1);
            }
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
            targetTransform = target != null ? target.transform : null;
        }

        protected void PrepareForSpawn(Player target)
        {
            attackTimer = 0f;
            isDead = false;
            isInAttackRange = false;
            SetTarget(target);
            enabled = target != null;
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || isDead)
            {
                return;
            }

            stat.hp = Mathf.Max(0f, stat.hp - amount);
            if (stat.hp > 0f)
            {
                return;
            }

            isDead = true;
            enabled = false;
            Die();
        }

        public void HealHp(float amount)
        {
            stat.hp += amount;
        }

        protected virtual void Update()
        {
            if (targetTransform == null)
            {
                return;
            }

            Vector3 targetPosition = targetTransform.position;
            UpdateInAttackRange(targetPosition);

            if (isInAttackRange)
            {
                UpdateAttackTimer();
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                stat.moveSpeed * Time.deltaTime
            );
        }

        protected virtual void Die()
        {
            Destroy(gameObject);
        }

        private void UpdateInAttackRange(Vector3 targetPosition)
        {
            Vector3 offset = targetPosition - transform.position;
            float attackRange = stat.attackRange;
            isInAttackRange = offset.sqrMagnitude <= attackRange * attackRange;
        }

        private void UpdateAttackTimer()
        {
            if (stat.attackSpeed <= 0f)
            {
                return;
            }

            attackTimer += Time.deltaTime;
            if (attackTimer >= 1f / stat.attackSpeed)
            {
                attackTimer = 0f;
                Attack(player);
            }
        }
    }
}
