using System;
using StopDefence.GameData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SkillCardUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image skillImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text digitText;
    [SerializeField] private ParticleSystem revealParticles;
    [SerializeField] private SkillCardParticleUI particleUI;
    [SerializeField] private Color placeholderColor = new(0.45f, 0.45f, 0.45f, 1f);

    private Action selected;

    private void Awake()
    {
        button.onClick.AddListener(HandleClicked);
        SetInteractable(false);

        if (revealParticles != null)
        {
            ParticleSystem.MainModule main = revealParticles.main;
            main.useUnscaledTime = true;
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }
    }

    public void Bind(SkillData skill, int targetSecond, Action onSelected)
    {
        selected = onSelected;
        nameText.text = skill.DisplayName;
        descriptionText.text = skill.Description;
        digitText.text = skill.Category == SkillCategory.Active
            ? $"목표 시간: {targetSecond:00}.00"
            : "획득 즉시 적용";

        Sprite sprite = skill.Image;
        skillImage.sprite = sprite;
        skillImage.color = sprite != null ? Color.white : placeholderColor;
        SetInteractable(false);
    }

    public void SetInteractable(bool value)
    {
        button.interactable = value;
    }

    public void PlayReveal()
    {
        if (revealParticles == null)
        {
            return;
        }

        revealParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        revealParticles.Play(true);
        particleUI?.Begin();
    }

    private void HandleClicked()
    {
        selected?.Invoke();
    }
}
