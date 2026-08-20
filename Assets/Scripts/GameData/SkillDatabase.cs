using System;
using System.Collections.Generic;
using UnityEngine;

namespace StopDefence.GameData
{
    public enum SkillCategory
    {
        Active,
        StatUpgrade
    }

    public enum PlayerStatType
    {
        None,
        MaxHp,
        DamageReduction,
        HealingReceived
    }

    [Serializable]
    public sealed class JudgementBalanceData
    {
        [SerializeField] private global::TimingJudgement judgement;
        [SerializeField, Min(0f)] private float damageMultiplier;

        public global::TimingJudgement Judgement => judgement;
        public float DamageMultiplier => damageMultiplier;

        public JudgementBalanceData(
            global::TimingJudgement judgement,
            float damageMultiplier)
        {
            this.judgement = judgement;
            this.damageMultiplier = damageMultiplier;
        }
    }

    [Serializable]
    public sealed class SkillData
    {
        [SerializeField] private string skillId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private SkillCategory category;
        [SerializeField, Range(0, 10)]
        [Tooltip("원본 스킬 Grade 메타데이터입니다. 현재 초 단위 발동 판정에는 사용하지 않습니다.")]
        private int grade;
        [SerializeField, Min(0f)]
        [Tooltip("판정 배율 적용 전 기본 데미지입니다. 지속 피해 스킬은 틱당 데미지입니다.")]
        private float baseDamage;
        [SerializeField] private string imagePath;
        [SerializeField] private Sprite image;
        [SerializeField] private bool enabled;
        [SerializeField] private PlayerStatType statType;
        [SerializeField, Min(0f)] private float statValue;
        [SerializeField, Min(0f)] private float statCap;

        public string Id => skillId;
        public string SkillId => skillId;
        public string DisplayName => displayName;
        public string Description => description;
        public SkillCategory Category => category;
        public int Grade => grade;
        public float BaseDamage => baseDamage;
        public string ImagePath => imagePath;
        public Sprite Image => image;
        public bool Enabled => enabled;
        public PlayerStatType StatType => statType;
        public float StatValue => statValue;
        public float StatCap => statCap;

        public SkillData(
            string skillId,
            string displayName,
            string description,
            SkillCategory category,
            int grade,
            float baseDamage,
            string imagePath,
            Sprite image,
            bool enabled,
            PlayerStatType statType,
            float statValue,
            float statCap)
        {
            this.skillId = skillId;
            this.displayName = displayName;
            this.description = description;
            this.category = category;
            this.grade = grade;
            this.baseDamage = baseDamage;
            this.imagePath = imagePath;
            this.image = image;
            this.enabled = enabled;
            this.statType = statType;
            this.statValue = statValue;
            this.statCap = statCap;
        }
    }

    [CreateAssetMenu(fileName = "SkillDatabase", menuName = "Stop Defence/Game Data/Skill Database")]
    public sealed class SkillDatabase : ScriptableObject
    {
        [SerializeField] private List<SkillData> skills = new List<SkillData>();
        [SerializeField] private List<JudgementBalanceData> judgementBalances =
            new List<JudgementBalanceData>();
        [NonSerialized] private Dictionary<string, SkillData> skillById;
        [NonSerialized] private List<SkillData> enabledSkills;
        [NonSerialized] private Dictionary<global::TimingJudgement, float>
            damageMultiplierByJudgement;

        public IReadOnlyList<SkillData> Skills => skills;
        public IReadOnlyList<JudgementBalanceData> JudgementBalances => judgementBalances;

        public IReadOnlyList<SkillData> EnabledSkills
        {
            get
            {
                EnsureLookup();
                return enabledSkills;
            }
        }

        public bool TryGetSkill(string id, out SkillData skill)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                skill = null;
                return false;
            }

            EnsureLookup();
            return skillById.TryGetValue(id, out skill);
        }

        public bool TryGetDamageMultiplier(
            global::TimingJudgement judgement,
            out float multiplier)
        {
            EnsureLookup();
            return damageMultiplierByJudgement.TryGetValue(judgement, out multiplier);
        }

        public void ReplaceSkills(IEnumerable<SkillData> values)
        {
            skills = new List<SkillData>(values);
            RebuildLookup();
        }

        public void ReplaceData(
            IEnumerable<SkillData> skillValues,
            IEnumerable<JudgementBalanceData> judgementValues)
        {
            skills = new List<SkillData>(skillValues);
            judgementBalances = new List<JudgementBalanceData>(judgementValues);
            RebuildLookup();
        }

        private void OnEnable()
        {
            RebuildLookup();
        }

        private void EnsureLookup()
        {
            if (skillById == null ||
                enabledSkills == null ||
                damageMultiplierByJudgement == null)
            {
                RebuildLookup();
            }
        }

        private void RebuildLookup()
        {
            skills ??= new List<SkillData>();
            judgementBalances ??= new List<JudgementBalanceData>();
            skillById = new Dictionary<string, SkillData>(
                skills.Count,
                StringComparer.OrdinalIgnoreCase);
            enabledSkills = new List<SkillData>(skills.Count);
            damageMultiplierByJudgement =
                new Dictionary<global::TimingJudgement, float>(judgementBalances.Count);

            foreach (SkillData skill in skills)
            {
                if (skill == null || string.IsNullOrWhiteSpace(skill.Id))
                {
                    continue;
                }

                if (!skillById.ContainsKey(skill.Id))
                {
                    skillById.Add(skill.Id, skill);
                }

                if (skill.Enabled)
                {
                    enabledSkills.Add(skill);
                }
            }

            foreach (JudgementBalanceData balance in judgementBalances)
            {
                if (balance == null || balance.DamageMultiplier <= 0f)
                {
                    continue;
                }

                damageMultiplierByJudgement.TryAdd(
                    balance.Judgement,
                    balance.DamageMultiplier);
            }
        }
    }
}
