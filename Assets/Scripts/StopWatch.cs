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

    void Start()
    {
        curTimer = maxTimer;
        prevTimer = -1;
        Time.fixedDeltaTime = 0.01f;

        castSkill = GetComponent<CastSkill>();
    }

    void Update()
    {
        // �����̽� �Է½� ��ų �ߵ�
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            prevTimer = (int)(curTimer * 100);

            // ��ų ���� ���� ���� Ȯ�� �� ��ų ����
            if (canSkillCast) {
                CastSkill();
            } else {
                Debug.Log("��ų ���� ���ٿ�� ��");
            }
        }

        // Ÿ�̸� UI ������Ʈ
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

        // Ÿ�̸� UI�� ��� �ð� ���
        timerSec = (int)curTimer;
        timerMil = (int)((curTimer - timerSec) * 100);
    }

    void CastSkill()
    {
        Debug.Log("��ų ���� �õ�");
        canSkillCast = false;

        // �����̽� Ű�� ������ �ʾ����� �н�
        if (prevTimer == -1) return;

        // ���� ���� ��ų Ȯ�� �� ����
        for (int i = 0; i < playerSkill.Count-1; i++) {
            playerSkill.TryGetValue(0, out string trigger);

            if (prevTimer.ToString().Contains(trigger))  {
                Debug.Log("��ų ����");
                canSkillCast = true;

                int key = playerSkill.FirstOrDefault(x => x.Value == trigger).Key;
                //��ų �ε����� ��ų ����
                castSkill.Cast(key);
            }
        }

        // ��ų�� ������� �ʾ����� 0.1�� ���� ��ų ���� �ȵǰ� �ϱ�
        if (!canSkillCast) {
            Debug.Log("���ٿ�� ����");
            Invoke("DelaySkillCast", 1f);
        }
    }
    void DelaySkillCast()
    {
        Debug.Log("���ٿ�� ����");
        canSkillCast = true;
    }
}
