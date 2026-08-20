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

    Dictionary<int, string> playerSkill = new Dictionary<int, string>
    { /* skill index, skill macro*/
        { 2, "1" }
    };

    float maxTimer = 20f;
    float curTimer;
    int timerSec;
    int timerMil;

    int prevTimer;

    bool canSkillCast = true;

    void Start()
    {
        // 타이머 관련 데이터 초기화
        curTimer = maxTimer;
        prevTimer = -1;
        Time.fixedDeltaTime = 0.01f;

        castSkill = GetComponent<CastSkill>();
    }

    void Update()
    {
        // 스페이스 입력시 스킬 발동
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            prevTimer = (int)(curTimer * 100);

            // 스킬 시전 가능 여부 확인 후 스킬 시전
            if (canSkillCast) {
                CastSkill();
            } else {
                Debug.Log("스킬 시전 리바운드 중");
            }
        }

        // 타이머 UI 업데이트
        if (timerText != null) {
            timerText.text = string.Format("{0:D2}:{1:D2}", timerSec, timerMil);
        }
    }

    void FixedUpdate()
    {
        // 타이머 시간 감소
        if (curTimer > 0) {
            curTimer -= Time.fixedDeltaTime;
        } else {
            curTimer = maxTimer;
        }

        // 타이머 UI에 띄울 시간 계산
        timerSec = (int)curTimer;
        timerMil = (int)((curTimer - timerSec) * 100);
    }

    void CastSkill()
    {
        Debug.Log("스킬 시전 시도");
        canSkillCast = false;

        // 스페이스 키를 누르지 않았으면 패스
        if (prevTimer == -1) return;

        // 시전 가능 스킬 확인 후 시전
        for (int i = 0; i < playerSkill.Count-1; i++) {
            playerSkill.TryGetValue(0, out string trigger);

            if (prevTimer.ToString().Contains(trigger))  {
                Debug.Log("스킬 시전");
                canSkillCast = true;

                int key = playerSkill.FirstOrDefault(x => x.Value == trigger).Key;
                //스킬 인덱스로 스킬 실행
                castSkill.Cast(key);
            }
        }

        // 스킬이 실행되지 않았으면 0.1초 동안 스킬 시전 안되게 하기
        if (!canSkillCast) {
            Debug.Log("리바운드 시작");
            Invoke("DelaySkillCast", 1f);
        }
    }
    void DelaySkillCast()
    {
        Debug.Log("리바운드 해제");
        canSkillCast = true;
    }
}