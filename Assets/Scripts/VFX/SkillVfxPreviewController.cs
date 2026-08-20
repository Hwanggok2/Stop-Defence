using UnityEngine;

namespace StopDefence.Vfx
{
    public sealed class SkillVfxPreviewController : MonoBehaviour
    {
        [SerializeField] private SkillParticleEffect fireballExplosion;
        [SerializeField] private SkillParticleEffect earthMagic;
        [SerializeField] private SkillParticleEffect nailDriving;
        [SerializeField] private SkillParticleEffect plagueMagic;
        [SerializeField] private SkillParticleEffect iceLance;
        [SerializeField] private SkillParticleEffect megaExplosion;
        [SerializeField] private SkillParticleEffect flashbang;

        private GUIStyle titleStyle;
        private GUIStyle helpStyle;

        private void Start()
        {
            PlayAll();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                fireballExplosion.Play();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                earthMagic.Play();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) && nailDriving != null)
            {
                nailDriving.Play();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4) && plagueMagic != null)
            {
                plagueMagic.Play();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5) && iceLance != null)
            {
                iceLance.Play();
            }

            if (Input.GetKeyDown(KeyCode.Alpha6) && flashbang != null)
            {
                flashbang.Play();
            }

            if (Input.GetKeyDown(KeyCode.Alpha7) && megaExplosion != null)
            {
                megaExplosion.Play();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayAll();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            int columnCount = flashbang != null
                ? 6
                : iceLance != null
                    ? 5
                    : plagueMagic != null
                    ? 4
                    : nailDriving != null
                        ? 3
                        : 2;
            float columnWidth = Screen.width / (float)columnCount;
            GUI.Label(new Rect(0f, 24f, columnWidth, 40f), "[1] 폭발 화염구", titleStyle);
            GUI.Label(new Rect(columnWidth, 24f, columnWidth, 40f), "[2] 대지 마법", titleStyle);
            if (nailDriving != null)
            {
                GUI.Label(
                    new Rect(columnWidth * 2f, 24f, columnWidth, 40f),
                    "[3] 대못 박기",
                    titleStyle);
            }

            if (plagueMagic != null)
            {
                GUI.Label(
                    new Rect(columnWidth * 3f, 24f, columnWidth, 40f),
                    "[4] 역병 마법",
                    titleStyle);
            }

            if (iceLance != null)
            {
                GUI.Label(
                    new Rect(columnWidth * 4f, 24f, columnWidth, 40f),
                    "[5] 얼음 창",
                    titleStyle);
            }

            if (flashbang != null)
            {
                GUI.Label(
                    new Rect(columnWidth * 5f, 24f, columnWidth, 40f),
                    "[6] 섬광탄",
                    titleStyle);
            }

            if (megaExplosion != null)
            {
                GUI.Label(
                    new Rect(Screen.width * 0.38f, Screen.height - 104f, Screen.width * 0.24f, 40f),
                    "[7] 대폭발",
                    titleStyle);
            }

            GUI.Label(
                new Rect(0f, Screen.height - 54f, Screen.width, 32f),
                megaExplosion != null
                    ? "1 / 2 / 3 / 4 / 5 / 6 / 7 : 개별 재생     Space : 전체 재생"
                    : flashbang != null
                    ? "1 / 2 / 3 / 4 / 5 / 6 : 개별 재생     Space : 전체 재생"
                    : iceLance != null
                        ? "1 / 2 / 3 / 4 / 5 : 개별 재생     Space : 전체 재생"
                        : plagueMagic != null
                        ? "1 / 2 / 3 / 4 : 개별 재생     Space : 전체 재생"
                    : nailDriving != null
                        ? "1 / 2 / 3 : 개별 재생     Space : 전체 재생"
                        : "1 / 2 : 개별 재생     Space : 전체 재생",
                helpStyle);
        }

        private void PlayAll()
        {
            fireballExplosion.Play();
            earthMagic.Play();
            if (nailDriving != null)
            {
                nailDriving.Play();
            }

            if (plagueMagic != null)
            {
                plagueMagic.Play();
            }

            if (iceLance != null)
            {
                iceLance.Play();
            }

            if (flashbang != null)
            {
                flashbang.Play();
            }

            if (megaExplosion != null)
            {
                megaExplosion.Play();
            }
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;

            helpStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };
            helpStyle.normal.textColor = new Color(0.85f, 0.9f, 1f);
        }
    }
}
