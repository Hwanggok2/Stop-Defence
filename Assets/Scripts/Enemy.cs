using UnityEngine;

public class Enemy : Entity
{
    [SerializeField] private float moveSpeed = 3f;
    
    [SerializeField] private Transform _player;
    
    public void SetTarget(Transform target)
    {
        _player = target;
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
