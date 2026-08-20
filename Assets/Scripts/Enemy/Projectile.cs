using UnityEngine;

namespace Enemy
{
    public class Projectile : MonoBehaviour
    {
        private float damage;
        private Vector3 targetPos;
        private float speed;

        public void Init(float damage, Transform target, float speed = 10f)
        {
            this.damage = damage;
            this.targetPos = target.position;
            this.speed = speed;

            Vector2 dir = (targetPos - transform.position);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private void Update()
        {
            Vector3 next = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            next.z = transform.position.z;
            transform.position = next;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log(other.name);
            if (!other.TryGetComponent<Player>(out var player)) return;
            
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}