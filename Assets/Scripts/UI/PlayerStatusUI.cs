using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerStatusUI : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider experienceSlider;

    private void OnEnable()
    {
        if (player == null)
        {
            return;
        }

        player.StatusChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.StatusChanged -= Refresh;
        }
    }

    private void Start()
    {
        if (player != null)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        healthSlider.SetValueWithoutNotify(player.HealthNormalized);
        experienceSlider.SetValueWithoutNotify(player.ExperienceNormalized);
    }
}
