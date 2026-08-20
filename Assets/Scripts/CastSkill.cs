using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using System.Collections.Generic;

public class CastSkill : MonoBehaviour
{
    [SerializeField]
    GameObject player;
    Dictionary<string, int> skillDict = new Dictionary<string, int>
    {
        { "폭발 화염구", 0 },
        { "대지 마법", 1 },
        { "연쇄 번개", 2 },
        { "대못 박기", 3 },
        { "수리 마법", 4 },
    };

    int maxChainValue = 7;


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
        // 처음에는 플레이어 기준으로 적에게 발사
        // 이후 6번 반복하며 적에게 발사
        for (int i = 0; i < maxChainValue; i++)
        {
            GameObject target = FindEnemy(player);
            if (target != null)
            {
                // target의 hp 감소시키는 로직 추가
                
            }
        }
    }

    GameObject FindEnemy(GameObject obj)
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
