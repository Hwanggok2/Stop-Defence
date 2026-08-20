using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TimerPopupUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StopWatch stopWatch;
        [SerializeField] private SkillInventory skillInventory;

        [Header("UI")]
        [SerializeField] private TMP_Text skillText;

        [Header("Display")]
        [SerializeField, Min(0f)] private float displayWindow = 0.5f;

        private void Awake()
        {
            ResolveReferences();
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
                float error = Mathf.Abs(currentTimer - skill.TargetSecond);

                if (error <= displayWindow && error < closestError)
                {
                    closestError = error;
                    closestSkill = skill;
                }
            }

            if (closestSkill.HasValue)
            {
                ShowSkill(closestSkill.Value, currentTimer);
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
            if (skillText == null)
            {
                return;
            }

            if (currentTimer >= skill.TargetSecond)
            {
                skillText.text =
                    $"{skill.SkillId}\n{skill.TargetSecond}초에 사용";
            }
            else
            {
                skillText.text =
                    $"{skill.SkillId}\n지금 사용 가능";
            }
        }

        private void ClearSkill()
        {
            if (skillText != null)
            {
                skillText.SetText(string.Empty);
            }
        }

        private void ClearUI()
        {
            if (skillText != null)
            {
                skillText.SetText(string.Empty);
            }
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