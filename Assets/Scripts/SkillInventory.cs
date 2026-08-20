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
    private const int MinimumTargetSecond = 0;
    private const int MaximumTargetSecond = 10;

    private readonly List<OwnedActiveSkill> ownedActiveSkills = new List<OwnedActiveSkill>();
    private readonly HashSet<string> ownedActiveSkillIds =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<int> ownedTargetSeconds = new HashSet<int>();

    public event Action<OwnedActiveSkill> ActiveSkillAcquired;

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
            return -1;
        }

        int availableCount = MaximumTargetSecond - MinimumTargetSecond + 1 -
                             ownedTargetSeconds.Count;
        if (availableCount <= 0)
        {
            return -1;
        }

        int availableIndex = UnityEngine.Random.Range(0, availableCount);
        for (int second = MinimumTargetSecond; second <= MaximumTargetSecond; second++)
        {
            if (ownedTargetSeconds.Contains(second))
            {
                continue;
            }

            if (availableIndex-- == 0)
            {
                return second;
            }
        }

        return -1;
    }

    public bool Acquire(SkillData skill, int targetSecond)
    {
        if (skill == null || !skill.Enabled || string.IsNullOrWhiteSpace(skill.Id) ||
            skill.Category != SkillCategory.Active ||
            targetSecond < MinimumTargetSecond || targetSecond > MaximumTargetSecond ||
            ownedActiveSkillIds.Contains(skill.Id) ||
            ownedTargetSeconds.Contains(targetSecond))
        {
            return false;
        }

        ownedActiveSkillIds.Add(skill.Id);
        ownedTargetSeconds.Add(targetSecond);
        var ownedSkill = new OwnedActiveSkill(skill.Id, targetSecond);
        ownedActiveSkills.Add(ownedSkill);
        ActiveSkillAcquired?.Invoke(ownedSkill);
        return true;
    }
}
