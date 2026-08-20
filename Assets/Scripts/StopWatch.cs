using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Collections;
using System.Collections.Generic;

public class StopWatch : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timerText;

    List<string> skills = new List<string> {"1","2","3","4"};

    float maxTimer = 60f;
    float curTimer;
    int timerSec;
    int timerMil;

    int prevTimer;

    void Start()
    {
        // 타이머 관련 데이터 초기화
        curTimer = maxTimer;
        prevTimer = 0;
        Time.fixedDeltaTime = 0.01f;
    }

    void Update() {
        // 스페이스 입력시 타이머 초기화
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            prevTimer = (int)(curTimer * 100);
            curTimer = 0f;
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

        timerSec = (int)curTimer;
        timerMil = (int)((curTimer - timerSec) * 100);

        Debug.Log("저장된 시간 : " + prevTimer);

    }
}