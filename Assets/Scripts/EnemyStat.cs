using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct EnemyStat
{
    public int level;
    public float hp;
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;
    public float moveSpeed;
    public int dropCoin;
}
