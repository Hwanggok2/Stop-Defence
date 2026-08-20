using UnityEngine;

namespace StopDefence.Vfx
{
    public sealed class SkillParticleEffect : MonoBehaviour
    {
        [SerializeField] private string skillId;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool destroyWhenFinished = true;

        private ParticleSystem[] particleSystems;
        private bool hasPlayed;

        public string SkillId => skillId;

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

            foreach (ParticleSystem particleSystem in particleSystems)
            {
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
            particleSystems ??= GetComponentsInChildren<ParticleSystem>(true);
        }
    }
}
