using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class StopWatch : MonoBehaviour
{
    [Header("Runtime References")]
    [SerializeField] private Button touchButton;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private SkillInventory skillInventory;
    [SerializeField] private CastSkill castSkill;

    [Header("Timer")]
    [SerializeField, Min(1f)] private float maxTimer = 10f;
    [SerializeField, Min(0f)] private float inputCooldown = 0.1f;
    [SerializeField, Min(0f)] private float timerAdjustStep = 2f;

    [Header("Judgement Windows")]
    [SerializeField, Min(0f)] private float perfectWindow = 0.05f;
    [SerializeField, Min(0f)] private float greatWindow = 0.15f;
    [SerializeField, Min(0f)] private float goodWindow = 0.3f;
    [SerializeField, Min(0f)] private float badWindow = 0.5f;

    private readonly HashSet<string> castThisCycle = new();

    private float currentTimer;
    private float nextInputTime;

    public float CurrentTimer => currentTimer;

    private void Awake()
    {
        ResolveRuntimeReferences();
        currentTimer = maxTimer;
        UpdateTimerText();
    }

    private void OnEnable()
    {
        if (touchButton != null)
        {
            touchButton.onClick.AddListener(TryCastOwnedSkills);
        }
    }

    private void OnDisable()
    {
        if (touchButton != null)
        {
            touchButton.onClick.RemoveListener(TryCastOwnedSkills);
        }
    }

    private void Update()
    {
        UpdateTimer();

        ReadKeyboardInput();
        UpdateTimerText();
    }

    private void ReadKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            TryCastOwnedSkills();
        }

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            AdjustTimer(timerAdjustStep);
        }

        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            AdjustTimer(-timerAdjustStep);
        }
    }

    // Nudges the timer so the player can line a skill up with its target second.
    // Winding the timer up stops at the top of the cycle, while skipping ahead is
    // allowed to roll over into the next cycle: 1.2s minus 2s lands on 9.2s.
    public void AdjustTimer(float deltaSeconds)
    {
        float adjusted = currentTimer + deltaSeconds;
        currentTimer = deltaSeconds < 0f
            ? Mathf.Repeat(adjusted, maxTimer)
            : Mathf.Min(adjusted, maxTimer);
        UpdateTimerText();
    }

    public void TryCastOwnedSkills()
    {
        if (Time.unscaledTime < nextInputTime)
        {
            return;
        }

        nextInputTime = Time.unscaledTime + inputCooldown;
        ResolveRuntimeReferences();
        if (skillInventory == null || castSkill == null)
        {
            Debug.LogWarning("[StopWatch] SkillInventory or CastSkill is not available.", this);
            return;
        }

        IReadOnlyList<OwnedActiveSkill> ownedSkills = skillInventory.OwnedActiveSkills;
        foreach (OwnedActiveSkill skill in ownedSkills)
        {
            if (castThisCycle.Contains(skill.SkillId))
            {
                continue;
            }

            float error = Mathf.Abs(currentTimer - skill.TargetSecond);
            if (!TryEvaluateJudgement(error, out TimingJudgement judgement))
            {
                continue;
            }

            if (castSkill.Cast(skill.SkillId, judgement))
            {
                castThisCycle.Add(skill.SkillId);
            }
        }
    }

    public bool TryEvaluateJudgement(
        float absoluteError,
        out TimingJudgement judgement)
    {
        if (absoluteError <= perfectWindow)
        {
            judgement = TimingJudgement.Perfect;
            return true;
        }

        if (absoluteError <= greatWindow)
        {
            judgement = TimingJudgement.Great;
            return true;
        }

        if (absoluteError <= goodWindow)
        {
            judgement = TimingJudgement.Good;
            return true;
        }

        judgement = TimingJudgement.Bad;
        return absoluteError <= badWindow;
    }

    private void UpdateTimer()
    {
        currentTimer -= Time.deltaTime;
        while (currentTimer <= 0f)
        {
            currentTimer += maxTimer;
            castThisCycle.Clear();
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        float displayedTime = Mathf.Max(0f, currentTimer);
        int seconds = Mathf.FloorToInt(displayedTime);
        int hundredths = Mathf.FloorToInt((displayedTime - seconds) * 100f);
        timerText.SetText("{0:00}:{1:00}", seconds, hundredths);
    }

    private void ResolveRuntimeReferences()
    {
        if (castSkill == null)
        {
            castSkill = GetComponent<CastSkill>();
        }

        if (skillInventory == null)
        {
            skillInventory = Object.FindFirstObjectByType<SkillInventory>();
        }
    }
}
