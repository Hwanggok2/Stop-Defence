using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float hp;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float damage;
    [SerializeField] protected float damageRate;
    [SerializeField] protected float attackDistance;
    
    private Transform _player;

    public void SetStats(float hp, float moveSpeed,
        float damage, float damageRate, float attackDistance)
    {
        this.hp = hp;
        this.moveSpeed = moveSpeed;
        this.damage = damage;
        this.damageRate = damageRate;
        this.attackDistance = attackDistance;
    }

    public void SetTarget(Transform target)
    {
        _player = target;
    }
    
    public void TakeDamage(float amount)
    {
        this.hp -= amount;
    }

    public void HealHp(float amount)
    {
        this.hp += amount;
    }

    private void Update()
    {
        MoveToTarget();
    }

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            _player.position,
            moveSpeed * Time.deltaTime
        );
    }
}
