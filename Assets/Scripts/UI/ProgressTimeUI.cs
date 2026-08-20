using TMPro;
using UnityEngine;

public sealed class ProgressTimeUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text timeText;

    private float elapsedTime;
    private int displayedSecond = -1;

    private void Awake()
    {
        UpdateDisplay(0);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        int totalSeconds = Mathf.FloorToInt(elapsedTime);
        if (totalSeconds == displayedSecond) return;

        UpdateDisplay(totalSeconds);
    }

    private void UpdateDisplay(int totalSeconds)
    {
        displayedSecond = totalSeconds;

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timeText.SetText("진행 시간  {0:00}:{1:00}", minutes, seconds);
    }
}
