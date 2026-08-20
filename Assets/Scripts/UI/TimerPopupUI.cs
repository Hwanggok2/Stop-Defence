using System.Collections.Generic;
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

        [Header("UI")]
        [SerializeField] private Image skillImage;
        [SerializeField] private TMP_Text skillText;
        [SerializeField] private TMP_Text pressedTimeText;

        [Header("Skill Images")]
        [SerializeField] private List<Sprite> skillImages = new();

        [Header("Display")]
        [SerializeField, Min(0f)] private float displayWindow = 0.5f;
        [SerializeField, Min(0f)] private float skillTextDisplayDuration = 0.5f;   // 추가
        [SerializeField, Min(0f)] private float pressedTimeDisplayDuration = 1f;

        
        private float skillTextTimer = 0f;    // 추가
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

        private Vector3 originalPressedTimeScale;

        private void Awake()
        {
            ResolveReferences();

            if (pressedTimeText != null)
            {
                originalPressedTimeScale =
                    pressedTimeText.transform.localScale;
            }

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

            // 두 타이머 각각 차감
            if (skillTextTimer > 0f)    skillTextTimer    -= Time.deltaTime;
            if (pressedTimeTimer > 0f)  pressedTimeTimer  -= Time.deltaTime;

            UpdatePopup();

            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ShowPressedResult();
            }
        }

        private void UpdatePopup()
        {
            IReadOnlyList<OwnedActiveSkill> skills =
                skillInventory.OwnedActiveSkills;

            if (skills == null || skills.Count == 0)
            {
                ClearUI();
                return;
            }

            float currentTimer = stopWatch.CurrentTimer;

            OwnedActiveSkill? closestSkill = null;
            float closestError = float.MaxValue;

            foreach (OwnedActiveSkill skill in skills)
            {
                float error = Mathf.Abs(
                    currentTimer - skill.TargetSecond);

                if (error <= displayWindow &&
                    error < closestError)
                {
                    closestError = error;
                    closestSkill = skill;
                }
            }

            if (closestSkill.HasValue)
            {
                ShowSkill(
                    closestSkill.Value,
                    currentTimer);
            }
            else
            {
                ClearSkill();
            }
        }

        private void ShowSkill(OwnedActiveSkill skill, float currentTimer)
        {
            float error = Mathf.Abs(currentTimer - skill.TargetSecond);

            if (skillText != null)
            {
                skillText.gameObject.SetActive(true);
                skillTextTimer = skillTextDisplayDuration; // 타이머 리셋

                skillText.text = error <= 0.05f
                    ? $"{skill.SkillId}\n지금 사용!"
                    : $"{skill.SkillId}\n{skill.TargetSecond}초";
            }

            SetSkillImage(skill.SkillId);
            HidePressedTime();
        }
        
        private void SetSkillImage(string skillId)
        {
            if (skillImage == null)
            {
                return;
            }

            foreach (Sprite sprite in skillImages)
            {
                if (sprite == null)
                {
                    continue;
                }

                if (sprite.name == skillId)
                {
                    skillImage.sprite = sprite;
                    skillImage.enabled = true;
                    return;
                }
            }

            Debug.LogWarning(
                $"[TimerPopupUI] SkillId '{skillId}'와 일치하는 Sprite를 찾을 수 없습니다.",
                this);

            skillImage.enabled = false;
        }

        private void ShowPressedResult()
        {
            IReadOnlyList<OwnedActiveSkill> skills =
                skillInventory.OwnedActiveSkills;

            if (skills == null || skills.Count == 0)
            {
                return;
            }

            float currentTimer = stopWatch.CurrentTimer;

            OwnedActiveSkill? closestSkill = null;
            float closestError = float.MaxValue;

            foreach (OwnedActiveSkill skill in skills)
            {
                float error = Mathf.Abs(
                    currentTimer - skill.TargetSecond);

                if (error <= displayWindow &&
                    error < closestError)
                {
                    closestError = error;
                    closestSkill = skill;
                }
            }

            if (!closestSkill.HasValue)
            {
                return;
            }

            OwnedActiveSkill skillToShow = closestSkill.Value;

            if (!stopWatch.TryEvaluateJudgement(
                    closestError,
                    out TimingJudgement judgement))
            {
                return;
            }

            ShowJudgement(
                skillToShow,
                closestError,
                judgement);
        }

        private void ShowJudgement(
            OwnedActiveSkill skill,
            float error,
            TimingJudgement judgement)
        {
            if (pressedTimeText == null) return;

            pressedTimeText.text = $"{error:F2}초";
            pressedTimeText.gameObject.SetActive(true);
            pressedTimeTimer = pressedTimeDisplayDuration; // 타이머 시작
            ApplyJudgementVisual(judgement);
        }
        private void ApplyJudgementVisual(
            TimingJudgement judgement)
        {
            switch (judgement)
            {
                case TimingJudgement.Perfect:
                    SetPressedTimeVisual(
                        perfectColor,
                        perfectScale);
                    break;

                case TimingJudgement.Great:
                    SetPressedTimeVisual(
                        greatColor,
                        greatScale);
                    break;

                case TimingJudgement.Good:
                    SetPressedTimeVisual(
                        goodColor,
                        goodScale);
                    break;

                case TimingJudgement.Bad:
                    SetPressedTimeVisual(
                        badColor,
                        badScale);
                    break;
            }
        }

        private void SetPressedTimeVisual(
            Color color,
            float scale)
        {
            pressedTimeText.color = color;

            pressedTimeText.transform.localScale =
                originalPressedTimeScale * scale;
        }


        private void HidePressedTime()
        {
            if (pressedTimeText == null) return;

            // 타이머가 남아있으면 숨기지 않음
            if (pressedTimeTimer > 0f) return;

            pressedTimeText.gameObject.SetActive(false);
            pressedTimeText.transform.localScale = originalPressedTimeScale;
        }

        private void HideSkillText()
        {
            if (skillText == null) return;
            if (skillTextTimer > 0f) return; // 타이머 남아있으면 스킵

            skillText.gameObject.SetActive(false);
            skillText.SetText(string.Empty);
        }

        private void ClearSkill()
        {
            HideSkillText(); // SetText 직접 호출 대신 타이머 체크

            if (skillImage != null)
                skillImage.enabled = false;

            HidePressedTime();
        }

        private void ClearUI()
        {
            ClearSkill();
        }

        private void ResolveReferences()
        {
            if (stopWatch == null)
            {
                stopWatch = GetComponentInParent<StopWatch>();
            }

            if (skillInventory == null)
            {
                skillInventory =
                    Object.FindFirstObjectByType<SkillInventory>();
            }
        }
    }
}