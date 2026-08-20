using System.Collections.Generic;
using StopDefence.GameData;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class TimerPopupUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StopWatch stopWatch;
        [SerializeField] private SkillInventory skillInventory;
        [SerializeField] private SkillDatabase skillDatabase; // List<Sprite> 대체

        [Header("UI")]
        [SerializeField] private Image skillImage;
        [SerializeField] private TMP_Text skillText;
        [SerializeField] private TMP_Text pressedTimeText;

        [Header("Display")]
        [SerializeField, Min(0f)] private float displayWindow = 0.5f;
        [SerializeField, Min(0f)] private float skillTextDisplayDuration = 0.5f;
        [SerializeField, Min(0f)] private float pressedTimeDisplayDuration = 1f;

        private float skillTextTimer = 0f;
        private float pressedTimeTimer = 0f;

        [Header("Judgement Visual")]
        [SerializeField] private Color perfectColor = Color.yellow;
        [SerializeField] private Color greatColor = Color.green;
        [SerializeField] private Color goodColor = Color.white;
        [SerializeField] private Color badColor = Color.red;

        [SerializeField, Min(0f)] private float perfectScale = 1.5f;
        [SerializeField, Min(0f)] private float greatScale = 1.3f;
        [SerializeField, Min(0f)] private float goodScale = 1.15f;
        [SerializeField, Min(0f)] private float badScale = 1f;

        [SerializeField, Min(0f)] private float perfectShakeDuration = 0.65f;
        [SerializeField, Min(0f)] private float perfectShakeStrength = 0.3f;

        private Vector3 originalPressedTimeScale;

        private void Awake()
        {
            ResolveReferences();

            if (pressedTimeText != null)
                originalPressedTimeScale = pressedTimeText.transform.localScale;

            ClearUI();
        }

        private void Update()
        {
            ResolveReferences();

            if (stopWatch == null || skillInventory == null)
            {
                ClearUI();
                return;
            }

            if (skillTextTimer > 0f)   skillTextTimer   -= Time.deltaTime;
            if (pressedTimeTimer > 0f) pressedTimeTimer -= Time.deltaTime;

            UpdatePopup();

            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ShowPressedResult();
            }
        }

        private void UpdatePopup()
        {
            IReadOnlyList<OwnedActiveSkill> skills = skillInventory.OwnedActiveSkills;

            if (skills == null || skills.Count == 0)
            {
                ClearUI();
                return;
            }

            OwnedActiveSkill? closest = FindClosestSkill(skills, out _);

            if (closest.HasValue)
                ShowSkill(closest.Value, stopWatch.CurrentTimer);
            else
                ClearSkill();
        }

        // UpdatePopup / ShowPressedResult 중복 제거
        private OwnedActiveSkill? FindClosestSkill(
            IReadOnlyList<OwnedActiveSkill> skills,
            out float closestError)
        {
            float currentTimer = stopWatch.CurrentTimer;
            OwnedActiveSkill? result = null;
            closestError = float.MaxValue;

            foreach (OwnedActiveSkill skill in skills)
            {
                float error = Mathf.Abs(currentTimer - skill.TargetSecond);

                if (error <= displayWindow && error < closestError)
                {
                    closestError = error;
                    result = skill;
                }
            }

            return result;
        }

        private void ShowSkill(OwnedActiveSkill skill, float currentTimer)
        {
            float error = Mathf.Abs(currentTimer - skill.TargetSecond);

            if (skillText != null)
            {
                // DisplayName 조회, 없으면 SkillId 폴백
                string displayName = skill.SkillId;
                if (skillDatabase != null &&
                    skillDatabase.TryGetSkill(skill.SkillId, out SkillData skillData))
                {
                    displayName = skillData.DisplayName;
                }

                skillText.gameObject.SetActive(true);
                skillTextTimer = skillTextDisplayDuration;

                skillText.text = error <= 0.05f
                    ? $"{displayName}\n지금 사용!"
                    : $"{displayName}\n{skill.TargetSecond}초";
            }

            SetSkillImage(skill.SkillId);
            HidePressedTime();
        }

        // SkillDatabase에서 직접 Sprite 조회
        private void SetSkillImage(string skillId)
        {
            if (skillImage == null) return;

            if (skillDatabase != null &&
                skillDatabase.TryGetSkill(skillId, out SkillData skillData) &&
                skillData.Image != null)
            {
                skillImage.sprite = skillData.Image;
                skillImage.enabled = true;
                return;
            }

            Debug.LogWarning(
                $"[TimerPopupUI] SkillId '{skillId}'와 일치하는 Sprite를 찾을 수 없습니다.",
                this);

            skillImage.enabled = false;
        }

        private void ShowPressedResult()
        {
            IReadOnlyList<OwnedActiveSkill> skills = skillInventory.OwnedActiveSkills;

            if (skills == null || skills.Count == 0) return;

            OwnedActiveSkill? closest = FindClosestSkill(skills, out float closestError);

            if (!closest.HasValue) return;

            if (!stopWatch.TryEvaluateJudgement(closestError, out TimingJudgement judgement)) return;

            ShowJudgement(closest.Value, closestError, judgement);
        }

        private void ShowJudgement(
            OwnedActiveSkill skill,
            float error,
            TimingJudgement judgement)
        {
            if (pressedTimeText == null) return;

            pressedTimeText.text = $"{error:F2}초";
            pressedTimeText.gameObject.SetActive(true);
            pressedTimeTimer = pressedTimeDisplayDuration;
            ApplyJudgementVisual(judgement);

            if (judgement == TimingJudgement.Perfect)
            {
                CameraController.Instance?.Shake(
                    perfectShakeDuration,
                    perfectShakeStrength);
            }
        }

        private void ApplyJudgementVisual(TimingJudgement judgement)
        {
            switch (judgement)
            {
                case TimingJudgement.Perfect: SetPressedTimeVisual(perfectColor, perfectScale); break;
                case TimingJudgement.Great:   SetPressedTimeVisual(greatColor,   greatScale);   break;
                case TimingJudgement.Good:    SetPressedTimeVisual(goodColor,    goodScale);    break;
                case TimingJudgement.Bad:     SetPressedTimeVisual(badColor,     badScale);     break;
            }
        }

        private void SetPressedTimeVisual(Color color, float scale)
        {
            pressedTimeText.color = color;
            pressedTimeText.transform.localScale = originalPressedTimeScale * scale;
        }

        private void HidePressedTime()
        {
            if (pressedTimeText == null) return;
            if (pressedTimeTimer > 0f) return;

            pressedTimeText.gameObject.SetActive(false);
            pressedTimeText.transform.localScale = originalPressedTimeScale;
        }

        private void HideSkillText()
        {
            if (skillText == null) return;
            if (skillTextTimer > 0f) return;

            skillText.gameObject.SetActive(false);
            skillText.SetText(string.Empty);
        }

        private void ClearSkill()
        {
            HideSkillText();

            if (skillImage != null)
                skillImage.enabled = false;

            HidePressedTime();
        }

        private void ClearUI() => ClearSkill();

        private void ResolveReferences()
        {
            if (stopWatch == null)
                stopWatch = GetComponentInParent<StopWatch>();

            if (skillInventory == null)
                skillInventory = Object.FindFirstObjectByType<SkillInventory>();
        }
    }
}
