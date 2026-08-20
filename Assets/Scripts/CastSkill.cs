using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using System.Collections.Generic;

public class CastSkill : MonoBehaviour
{
    GameObject player;
    Dictionary<string, int> skillDict = new Dictionary<string, int>
    {
        { "chain lightning", 2 }
    };

    int maxChainValue = 7;

    [SerializeField]
    float chainLightningDamage = 20f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }


    public void Cast(int ind)
    {
        switch (ind) {
            case 2:
                ChainLightning();
                break;
        }
    }

    void ChainLightning()
    {
        Debug.Log("cast");
        HashSet<GameObject> hitEnemies = new HashSet<GameObject>();
        GameObject origin = player;

        for (int i = 0; i < maxChainValue; i++)
        {
            GameObject target = FindEnemy(origin, hitEnemies);
            if (target == null)
            {
                break;
            }

            Enemy.Enemy enemy = target.GetComponent<Enemy.Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(chainLightningDamage);
            }

            hitEnemies.Add(target);
            origin = target;
        }
    }

    GameObject FindEnemy(GameObject obj, HashSet<GameObject> exclude = null)
    {
        if (obj == null) return null;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        GameObject nearestEnemy = null;
        float minSqrDistance = float.MaxValue;
        Vector3 originPos = obj.transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy == obj) continue;
            if (exclude != null && exclude.Contains(enemy)) continue;

            Vector3 diff = enemy.transform.position - originPos;
            float sqrDistance = diff.sqrMagnitude;

            if (sqrDistance < minSqrDistance) {
                minSqrDistance = sqrDistance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }
}
