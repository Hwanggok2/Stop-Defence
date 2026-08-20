using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

        [Header("Judgement Visual")]
        [SerializeField] private Color perfectColor = Color.yellow;
        [SerializeField] private Color greatColor = Color.green;
        [SerializeField] private Color goodColor = Color.white;
        [SerializeField] private Color badColor = Color.red;

        [SerializeField, Min(0f)] private float perfectScale = 1.5f;
        [SerializeField, Min(0f)] private float greatScale = 1.3f;
        [SerializeField, Min(0f)] private float goodScale = 1.15f;
        [SerializeField, Min(0f)] private float badScale = 1f;

        [SerializeField, Min(0f)] private float pressedTimeDuration = 2f;
        [SerializeField, Min(0f)] private float perfectShakeDuration = 0.65f;
        [SerializeField, Min(0f)] private float perfectShakeStrength = 0.3f;

        private Vector3 originalPressedTimeScale;
        private Coroutine pressedTimeRoutine;

        private void Awake()
        {
            ResolveReferences();

            if (pressedTimeText != null)
            {
                originalPressedTimeScale =
                    pressedTimeText.transform.localScale;
            }

            ClearUI();
            HidePressedTime();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (stopWatch != null)
            {
                stopWatch.SpacePressed += ShowPressedResult;
            }
        }

        private void OnDisable()
        {
            if (stopWatch != null)
            {
                stopWatch.SpacePressed -= ShowPressedResult;
            }

            if (pressedTimeRoutine != null)
            {
                StopCoroutine(pressedTimeRoutine);
                pressedTimeRoutine = null;
            }

            HidePressedTime();
        }

        private void Update()
        {
            ResolveReferences();

            if (stopWatch == null || skillInventory == null)
            {
                ClearUI();
                return;
            }

            UpdatePopup();
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

        private void ShowSkill(
            OwnedActiveSkill skill,
            float currentTimer)
        {
            float error = Mathf.Abs(
                currentTimer - skill.TargetSecond);

            if (skillText != null)
            {
                if (error <= 0.05f)
                {
                    skillText.text =
                        $"{skill.SkillId}\n지금 사용!";
                }
                else
                {
                    skillText.text =
                        $"{skill.SkillId}\n{skill.TargetSecond}초";
                }
            }

            SetSkillImage(skill.SkillId);
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

        private void ShowPressedResult(float currentTimer)
        {
            IReadOnlyList<OwnedActiveSkill> skills =
                skillInventory.OwnedActiveSkills;

            if (skills == null || skills.Count == 0)
            {
                return;
            }

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

            if (!stopWatch.TryEvaluateJudgement(
                    closestError,
                    out TimingJudgement judgement))
            {
                return;
            }

            ShowJudgement(
                currentTimer,
                judgement);
        }

        private void ShowJudgement(
            float pressedTime,
            TimingJudgement judgement)
        {
            if (pressedTimeText == null)
            {
                return;
            }

            if (pressedTimeRoutine != null)
            {
                StopCoroutine(pressedTimeRoutine);
            }

            pressedTimeText.text = $"{pressedTime:F2}초";
            pressedTimeText.transform.localScale = originalPressedTimeScale;

            pressedTimeText.gameObject.SetActive(true);

            ApplyJudgementVisual(judgement);
            if (judgement == TimingJudgement.Perfect)
            {
                CameraController.Instance?.Shake(
                    perfectShakeDuration,
                    perfectShakeStrength);
            }

            pressedTimeRoutine = StartCoroutine(
                ShowPressedTimeRoutine());
        }

        private IEnumerator ShowPressedTimeRoutine()
        {
            float elapsed = 0f;

            while (elapsed < pressedTimeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            pressedTimeRoutine = null;
            HidePressedTime();
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
            if (pressedTimeText == null)
            {
                return;
            }

            pressedTimeText.gameObject.SetActive(false);

            pressedTimeText.transform.localScale =
                originalPressedTimeScale;
        }

        private void ClearSkill()
        {
            if (skillText != null)
            {
                skillText.SetText(string.Empty);
            }

            if (skillImage != null)
            {
                skillImage.enabled = false;
            }
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
