using StopDefence.GameData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class OwnedSkillItemUI : MonoBehaviour
{
    [SerializeField] private Image skillImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text targetSecondText;
    [SerializeField] private Color placeholderColor = new(0.45f, 0.45f, 0.45f, 1f);

    public void Bind(SkillData skill, int targetSecond)
    {
        nameText.text = skill.DisplayName;
        targetSecondText.text = $"{targetSecond:00}.00초";

        Sprite sprite = skill.Image;
        skillImage.sprite = sprite;
        skillImage.color = sprite != null ? Color.white : placeholderColor;
    }
}
