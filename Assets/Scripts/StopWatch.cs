using TMPro;
using UnityEngine;

public class StopWatch : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timerText;

    float maxTimer = 60f;
    float curTimer;
    int timerSec;
    int timerMil;

    void Start()
    {
        curTimer = maxTimer;
        Time.fixedDeltaTime = 0.01f;
    }

    void Update() {
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
}
