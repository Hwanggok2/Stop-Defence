using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using System.Collections.Generic;

public class CastSkill : MonoBehaviour
{
    Dictionary<string, int> skillDict = new Dictionary<string, int>
    {
        { "폭발 화염구", 0 },
        { "대지 마법", 1 },
        { "연쇄 번개", 2 },
        { "대못 박기", 3 },
        { "수리 마법", 4 },
    };


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
        
    }

    void FindEnemy()
    {

    }
}
