using UnityEngine;

namespace StopDefence.Vfx
{
    public sealed class SkillParticleEffect : MonoBehaviour
    {
        [SerializeField] private string skillId;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool destroyWhenFinished = true;
        [SerializeField, Min(0.01f)] private float playbackSpeed = 1.5f;

        private ParticleSystem[] particleSystems;
        private float[] baseSimulationSpeeds;
        private bool hasPlayed;

        public string SkillId => skillId;
        public float PlaybackSpeed => Mathf.Max(0.01f, playbackSpeed);

        private void OnEnable()
        {
            if (Application.isPlaying && playOnEnable)
            {
                Play();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || !destroyWhenFinished || !hasPlayed)
            {
                return;
            }

            if (!IsAlive())
            {
                Destroy(gameObject);
            }
        }

        [ContextMenu("Play Effect")]
        public void Play()
        {
            CacheParticleSystems();

            for (int index = 0; index < particleSystems.Length; index++)
            {
                ParticleSystem particleSystem = particleSystems[index];
                ParticleSystem.MainModule main = particleSystem.main;
                main.simulationSpeed = baseSimulationSpeeds[index] * PlaybackSpeed;
                particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.Play(false);
            }

            hasPlayed = true;
        }

        [ContextMenu("Stop Effect")]
        public void Stop()
        {
            CacheParticleSystems();

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            hasPlayed = false;
        }

        private bool IsAlive()
        {
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (particleSystem.IsAlive(false))
                {
                    return true;
                }
            }

            return false;
        }

        private void CacheParticleSystems()
        {
            if (particleSystems != null)
            {
                return;
            }

            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            baseSimulationSpeeds = new float[particleSystems.Length];
            for (int index = 0; index < particleSystems.Length; index++)
            {
                baseSimulationSpeeds[index] = particleSystems[index].main.simulationSpeed;
            }
        }
    }
}
