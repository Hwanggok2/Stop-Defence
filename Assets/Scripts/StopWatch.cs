using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;

public class StopWatch : MonoBehaviour
{
    [SerializeField]
    private CastSkill castSkill;

    [SerializeField]
    private TextMeshProUGUI timerText;

    float maxTimer = 60f;
    float curTimer;
    int timerSec;
    int timerMil;

    int prevTimer;

    bool canSkillCast = true;

    Dictionary<int, string> playerSkill = new Dictionary<int, string> {
        { 2, "1" }
    };

    void Start()
    {
        curTimer = maxTimer;
        prevTimer = -1;
        Time.fixedDeltaTime = 0.01f;

        castSkill = GetComponent<CastSkill>();
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            prevTimer = (int)(curTimer * 100);

            if (canSkillCast) {
                CastSkill();
            }
        }

        if (timerText != null) {
            timerText.text = string.Format("{0:D2}:{1:D2}", timerSec, timerMil);
        }
    }

    void FixedUpdate()
    {
        if (curTimer > 0) {
            curTimer -= Time.fixedDeltaTime;
        } else {
            curTimer = maxTimer;
        }

        timerSec = (int)curTimer;
        timerMil = (int)((curTimer - timerSec) * 100);
    }

    void CastSkill()
    {
        canSkillCast = false;

        if (prevTimer == -1) return;

        for (int i = 0; i < playerSkill.Count-1; i++) {
            playerSkill.TryGetValue(0, out string trigger);

            if (prevTimer.ToString().Contains(trigger))  {
                canSkillCast = true;
                int key = playerSkill.FirstOrDefault(x => x.Value == trigger).Key;
                castSkill.Cast(key);
            }
        }

        if (!canSkillCast) {
            Invoke("DelaySkillCast", 0.1f);
        }
    }
    void DelaySkillCast()
    {
        canSkillCast = true;
    }
}
