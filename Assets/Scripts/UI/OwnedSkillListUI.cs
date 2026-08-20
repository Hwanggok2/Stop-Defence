using System;
using System.Collections.Generic;
using StopDefence.GameData;
using UnityEngine;

public sealed class OwnedSkillListUI : MonoBehaviour
{
    [SerializeField] private SkillDatabase database;
    [SerializeField] private SkillInventory inventory;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private OwnedSkillItemUI itemPrefab;

    private readonly List<OwnedActiveSkill> sortedSkills = new();
    private readonly List<OwnedSkillItemUI> spawnedItems = new();

    private void OnEnable()
    {
        ResolveInventory();
        if (inventory != null)
        {
            inventory.ActiveSkillAcquired += HandleActiveSkillAcquired;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.ActiveSkillAcquired -= HandleActiveSkillAcquired;
        }
    }

    private void HandleActiveSkillAcquired(OwnedActiveSkill _)
    {
        Refresh();
    }

    private void Refresh()
    {
        ClearItems();
        if (database == null || inventory == null || contentRoot == null || itemPrefab == null)
        {
            return;
        }

        sortedSkills.Clear();
        sortedSkills.AddRange(inventory.OwnedActiveSkills);
        sortedSkills.Sort(CompareSkills);

        foreach (OwnedActiveSkill ownedSkill in sortedSkills)
        {
            if (!database.TryGetSkill(ownedSkill.SkillId, out SkillData skill))
            {
                continue;
            }

            OwnedSkillItemUI item = Instantiate(itemPrefab, contentRoot);
            item.Bind(skill, ownedSkill.TargetSecond);
            spawnedItems.Add(item);
        }
    }

    private void ClearItems()
    {
        foreach (OwnedSkillItemUI item in spawnedItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        spawnedItems.Clear();
    }

    private void ResolveInventory()
    {
        if (inventory == null)
        {
            inventory = GetComponentInParent<SkillInventory>();
        }
    }

    private static int CompareSkills(OwnedActiveSkill left, OwnedActiveSkill right)
    {
        int secondComparison = left.TargetSecond.CompareTo(right.TargetSecond);
        return secondComparison != 0
            ? secondComparison
            : StringComparer.OrdinalIgnoreCase.Compare(left.SkillId, right.SkillId);
    }
}
