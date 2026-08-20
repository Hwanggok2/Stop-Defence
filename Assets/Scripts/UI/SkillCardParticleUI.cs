using UnityEngine;
using UnityEngine.UI;

public sealed class SkillCardParticleUI : MonoBehaviour
{
    [SerializeField] private ParticleSystem source;
    [SerializeField] private RectTransform[] particleRects;
    [SerializeField] private Image[] particleImages;

    private ParticleSystem.Particle[] particles;

    private void Awake()
    {
        EnsureBuffer();
        HideParticles();
        enabled = false;
    }

    private void OnDisable()
    {
        HideParticles();
    }

    public void Begin()
    {
        EnsureBuffer();
        HideParticles();
        enabled = source != null && particles.Length > 0;
    }

    private void LateUpdate()
    {
        int count = source.GetParticles(particles, particles.Length);
        for (int i = 0; i < count; i++)
        {
            ParticleSystem.Particle particle = particles[i];
            RectTransform particleRect = particleRects[i];
            Image particleImage = particleImages[i];

            if (!particleImage.gameObject.activeSelf)
            {
                particleImage.gameObject.SetActive(true);
            }
            particleImage.color = particle.GetCurrentColor(source);
            particleRect.anchoredPosition = particle.position;
            float size = Mathf.Max(1f, particle.GetCurrentSize(source));
            particleRect.sizeDelta = new Vector2(size, size);
            particleRect.localRotation = Quaternion.Euler(0f, 0f, -particle.rotation);
        }

        for (int i = count; i < particleImages.Length; i++)
        {
            if (particleImages[i].gameObject.activeSelf)
            {
                particleImages[i].gameObject.SetActive(false);
            }
        }

        if (count == 0 && !source.IsAlive(true))
        {
            enabled = false;
        }
    }

    private void EnsureBuffer()
    {
        int count = Mathf.Min(
            particleRects?.Length ?? 0,
            particleImages?.Length ?? 0);
        if (particles == null || particles.Length != count)
        {
            particles = new ParticleSystem.Particle[count];
        }
    }

    private void HideParticles()
    {
        if (particleImages == null)
        {
            return;
        }

        for (int i = 0; i < particleImages.Length; i++)
        {
            if (particleImages[i] != null)
            {
                if (particleImages[i].gameObject.activeSelf)
                {
                    particleImages[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
