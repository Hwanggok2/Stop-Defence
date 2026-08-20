using System.Globalization;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Enemy
{
    public abstract class Enemy : MonoBehaviour
    {
        [SerializeField] protected EnemyStat stat;
        [SerializeField] private Player player;

        protected bool isInAttackRange;

        private float attackTimer;
        private float moveSpeedMultiplier = 1f;
        private Transform targetTransform;
        private bool isDead;
        private Coroutine slowRoutine;

        public bool IsDead => isDead;
        public float CurrentHp => stat.hp;

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
            StopAllCoroutines();
            attackTimer = 0f;
            moveSpeedMultiplier = 1f;
            slowRoutine = null;
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

            float previousHp = stat.hp;
            stat.hp = Mathf.Max(0f, stat.hp - amount);
            DamagePopup.Show(transform.position, previousHp - stat.hp);
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

        public void ApplyDamageOverTime(
            float damagePerTick,
            float duration,
            float tickInterval = 1f)
        {
            if (damagePerTick <= 0f || duration <= 0f || tickInterval <= 0f || isDead)
            {
                return;
            }

            StartCoroutine(DamageOverTimeRoutine(damagePerTick, duration, tickInterval));
        }

        public void ApplySlow(float slowRate, float duration)
        {
            if (slowRate <= 0f || duration <= 0f || isDead)
            {
                return;
            }

            if (slowRoutine != null)
            {
                StopCoroutine(slowRoutine);
            }

            slowRoutine = StartCoroutine(SlowRoutine(slowRate, duration));
        }

        public void Knockback(Vector3 direction, float distance)
        {
            if (distance <= 0f || isDead || direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            transform.position += direction.normalized * distance;
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
                stat.moveSpeed * moveSpeedMultiplier * Time.deltaTime
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

        private IEnumerator DamageOverTimeRoutine(
            float damagePerTick,
            float duration,
            float tickInterval)
        {
            float elapsed = 0f;
            var wait = new WaitForSeconds(tickInterval);

            while (elapsed < duration && !isDead)
            {
                TakeDamage(damagePerTick);
                if (isDead)
                {
                    yield break;
                }

                yield return wait;
                elapsed += tickInterval;
            }
        }

        private IEnumerator SlowRoutine(float slowRate, float duration)
        {
            moveSpeedMultiplier = 1f - Mathf.Clamp(slowRate, 0f, 0.95f);
            yield return new WaitForSeconds(duration);
            moveSpeedMultiplier = 1f;
            slowRoutine = null;
        }

    }

    internal sealed class DamagePopup : MonoBehaviour
    {
        private const float Lifetime = 0.75f;
        private const float RiseSpeed = 1.2f;

        private TMP_Text label;
        private float elapsed;

        public static void Show(Vector3 hitPosition, float damage)
        {
            if (damage <= 0f)
            {
                return;
            }

            var popupObject = new GameObject(
                "Damage Popup",
                typeof(TextMeshPro),
                typeof(DamagePopup));
            popupObject.transform.position = hitPosition + new Vector3(
                Random.Range(-0.15f, 0.15f),
                1f,
                -0.1f);

            DamagePopup popup = popupObject.GetComponent<DamagePopup>();
            popup.label = popupObject.GetComponent<TextMeshPro>();
            popup.label.text = damage.ToString("0.#", CultureInfo.InvariantCulture);
            popup.label.alignment = TextAlignmentOptions.Center;
            popup.label.color = new Color(1f, 0.35f, 0.1f, 1f);
            popup.label.fontSize = 3.5f;
            popup.label.fontStyle = FontStyles.Bold;
            popup.label.raycastTarget = false;

            MeshRenderer popupRenderer = popupObject.GetComponent<MeshRenderer>();
            if (popupRenderer != null)
            {
                popupRenderer.sortingOrder = 1000;
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * (RiseSpeed * Time.deltaTime);

            Color color = label.color;
            color.a = 1f - Mathf.Clamp01(elapsed / Lifetime);
            label.color = color;

            if (elapsed >= Lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
