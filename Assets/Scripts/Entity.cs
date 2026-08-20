using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public float Hp { get; private set; }

    public void TakeDamage(float damage)
    {
        Hp -= damage;
    }

    public void HealHp(float heal)
    {
        Hp += heal;
    }
}
