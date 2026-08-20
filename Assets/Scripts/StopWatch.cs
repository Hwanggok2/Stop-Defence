using System;
using TMPro;
using UnityEngine;

public class StopWatch : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    public float Seconds { get; private set; } = 0.0f;
    private void Update()
    {
        if (Seconds >= 60.0f)
        {
            Seconds = 0.0f;
        }
        
        var sec = Time.deltaTime;
        Seconds += sec;
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        int seconds = Mathf.FloorToInt(Seconds % 60);
        int milliseconds = Mathf.FloorToInt((Seconds - seconds) * 100);
        timeText.text = $"{seconds:00} : {milliseconds:00}";
    }
}
