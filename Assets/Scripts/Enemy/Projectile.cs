using UnityEngine;

namespace Enemy
{
    public class Projectile : MonoBehaviour
    {
        private float damage;
        private Player target;
        private float speed;
        private float remainingLifetime;
        private bool hasHit;
        private BattleObjectPool objectPool;

        public void Init(
            float damage,
            Player target,
            BattleObjectPool pool,
            float speed = 10f)
        {
            this.damage = damage;
            this.target = target;
            this.speed = speed;
            objectPool = pool;
            remainingLifetime = 5f;
            hasHit = false;

            Vector2 dir = target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private void Update()
        {
            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                Release();
                return;
            }

            if (target == null || target.IsDead)
            {
                Release();
                return;
            }

            Vector3 targetPosition = target.transform.position;
            Vector3 next = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime);
            next.z = transform.position.z;
            transform.position = next;

            if ((targetPosition - next).sqrMagnitude <= 0.0001f)
            {
                Hit(target);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Player>(out var player)) return;

            Hit(player);
        }

        private void Hit(Player player)
        {
            if (hasHit) return;

            hasHit = true;
            player.TakeDamage(damage);
            Release();
        }

        private void Release()
        {
            target = null;
            objectPool.ReleaseProjectile(this);
        }
    }
}
