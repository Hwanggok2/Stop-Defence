using UnityEngine;

public class Player : MonoBehaviour
{
    [field: SerializeField] public float Hp { get; private set; } = 100f;

    public void TakeDamage(float amount)
    {
        Hp -= amount;
    }

    public void HealHp(float amount)
    {
        Hp += amount;
    }
}
