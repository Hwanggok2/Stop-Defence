using System;
using System.Collections.Generic;
using StopDefence.GameData;
using UnityEngine;

public readonly struct OwnedActiveSkill
{
    public string SkillId { get; }
    public int TargetSecond { get; }

    public OwnedActiveSkill(string skillId, int targetSecond)
    {
        SkillId = skillId;
        TargetSecond = targetSecond;
    }
}

public sealed class SkillInventory : MonoBehaviour
{
    private const int MinimumTargetSecond = 1;
    private const int MaximumTargetSecond = 10;

    private readonly List<OwnedActiveSkill> ownedActiveSkills = new List<OwnedActiveSkill>();
    private readonly HashSet<string> ownedActiveSkillIds =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<OwnedActiveSkill> OwnedActiveSkills => ownedActiveSkills;

    public bool OwnsActiveSkill(string skillId)
    {
        return !string.IsNullOrWhiteSpace(skillId) && ownedActiveSkillIds.Contains(skillId);
    }

    public int CreateOfferTargetSecond(SkillData skill)
    {
        if (skill == null || !skill.Enabled || skill.Category != SkillCategory.Active ||
            OwnsActiveSkill(skill.Id))
        {
            return 0;
        }

        return UnityEngine.Random.Range(MinimumTargetSecond, MaximumTargetSecond + 1);
    }

    public bool Acquire(SkillData skill, int targetSecond)
    {
        if (skill == null || !skill.Enabled || string.IsNullOrWhiteSpace(skill.Id) ||
            skill.Category != SkillCategory.Active ||
            targetSecond < MinimumTargetSecond || targetSecond > MaximumTargetSecond ||
            !ownedActiveSkillIds.Add(skill.Id))
        {
            return false;
        }

        ownedActiveSkills.Add(new OwnedActiveSkill(skill.Id, targetSecond));
        return true;
    }
}
