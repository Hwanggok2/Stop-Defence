using System.Collections;
using System.Collections.Generic;
using StopDefence.GameData;
using UnityEngine;

public sealed class SkillSelectionUI : MonoBehaviour
{
    private const float InputLockSeconds = 1f;

    [SerializeField] private SkillDatabase database;
    [SerializeField] private Player player;
    [SerializeField] private SkillInventory inventory;
    [SerializeField] private GameObject selectionRoot;
    [SerializeField] private SkillCardUI[] cards;
    [SerializeField] private bool requestInitialSelection = true;

    private readonly Queue<bool> requests = new();
    private readonly List<SkillData> candidates = new();
    private readonly HashSet<string> candidateIds = new(System.StringComparer.OrdinalIgnoreCase);
    private Coroutine unlockRoutine;
    private bool initialRequestQueued;
    private bool selectionActive;
    private bool inputUnlocked;
    private bool currentActiveOnly;
    private bool ownsPause;
    private float previousTimeScale;

    private void Awake()
    {
        ResolveRuntimeReferences();
        selectionRoot.SetActive(false);
    }

    private void OnEnable()
    {
        ResolveRuntimeReferences();
        if (player != null)
        {
            player.LevelGained += HandleLevelGained;
        }
    }

    private void Start()
    {
        if (requestInitialSelection)
        {
            RequestInitialSelection();
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.LevelGained -= HandleLevelGained;
        }

        RestoreBattleTime();
    }

    public void RequestInitialSelection()
    {
        if (initialRequestQueued)
        {
            return;
        }

        initialRequestQueued = true;
        EnqueueSelection(true);
    }

    public void RequestLevelSelection()
    {
        EnqueueSelection(false);
    }

    private void HandleLevelGained(int _)
    {
        RequestLevelSelection();
    }

    private void EnqueueSelection(bool activeOnly)
    {
        requests.Enqueue(activeOnly);
        TryShowNextRequest();
    }

    private void TryShowNextRequest()
    {
        if (selectionActive)
        {
            return;
        }

        while (requests.Count > 0)
        {
            currentActiveOnly = requests.Dequeue();
            if (ShowSelection(currentActiveOnly))
            {
                return;
            }
        }

        RestoreBattleTime();
    }

    private bool ShowSelection(bool activeOnly)
    {
        if (database == null || inventory == null || cards == null || cards.Length == 0)
        {
            Debug.LogError("Skill selection references are not assigned.", this);
            return false;
        }

        CollectCandidates(activeOnly);
        if (candidates.Count == 0)
        {
            Debug.LogWarning("No available skills remain for this selection.", this);
            return false;
        }

        ShuffleCandidates();
        int offerCount = 0;
        for (int i = 0; i < candidates.Count && offerCount < cards.Length; i++)
        {
            SkillData skill = candidates[i];
            int targetSecond = inventory.CreateOfferTargetSecond(skill);
            if (skill.Category == SkillCategory.Active && targetSecond == 0)
            {
                continue;
            }

            SkillCardUI card = cards[offerCount++];
            card.gameObject.SetActive(true);
            card.Bind(skill, targetSecond, () => Select(skill, targetSecond));
        }

        if (offerCount == 0)
        {
            return false;
        }

        for (int i = offerCount; i < cards.Length; i++)
        {
            cards[i].gameObject.SetActive(false);
        }

        PauseBattleTime();
        selectionActive = true;
        inputUnlocked = false;
        selectionRoot.SetActive(true);

        for (int i = 0; i < offerCount; i++)
        {
            cards[i].PlayReveal();
        }

        if (unlockRoutine != null)
        {
            StopCoroutine(unlockRoutine);
        }

        unlockRoutine = StartCoroutine(UnlockCardsAfterDelay(offerCount));
        return true;
    }

    private void CollectCandidates(bool activeOnly)
    {
        candidates.Clear();
        candidateIds.Clear();

        IReadOnlyList<SkillData> skills = database.Skills;
        for (int i = 0; i < skills.Count; i++)
        {
            SkillData skill = skills[i];
            if (skill == null || !skill.Enabled || !candidateIds.Add(skill.Id))
            {
                continue;
            }

            if (skill.Category == SkillCategory.Active)
            {
                if (inventory.OwnsActiveSkill(skill.Id))
                {
                    continue;
                }
            }
            else if (activeOnly || player == null || !player.CanApplyStatUpgrade(skill))
            {
                continue;
            }

            candidates.Add(skill);
        }
    }

    private void ShuffleCandidates()
    {
        for (int i = 0; i < candidates.Count - 1; i++)
        {
            int swapIndex = Random.Range(i, candidates.Count);
            (candidates[i], candidates[swapIndex]) = (candidates[swapIndex], candidates[i]);
        }
    }

    private IEnumerator UnlockCardsAfterDelay(int offerCount)
    {
        yield return new WaitForSecondsRealtime(InputLockSeconds);

        inputUnlocked = true;
        unlockRoutine = null;
        for (int i = 0; i < offerCount; i++)
        {
            cards[i].SetInteractable(true);
        }
    }

    private void Select(SkillData skill, int targetSecond)
    {
        if (!selectionActive || !inputUnlocked)
        {
            return;
        }

        inputUnlocked = false;
        LockAllCards();

        bool acquired = skill.Category == SkillCategory.Active
            ? inventory.Acquire(skill, targetSecond)
            : player != null && player.ApplyStatUpgrade(skill);
        if (!acquired)
        {
            Debug.LogWarning($"Failed to acquire skill offer '{skill.Id}'. Regenerating cards.", this);
            ShowSelection(currentActiveOnly);
            return;
        }

        selectionRoot.SetActive(false);
        selectionActive = false;
        TryShowNextRequest();
    }

    private void LockAllCards()
    {
        if (unlockRoutine != null)
        {
            StopCoroutine(unlockRoutine);
            unlockRoutine = null;
        }

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].SetInteractable(false);
        }
    }

    private void PauseBattleTime()
    {
        if (ownsPause)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        ownsPause = true;
    }

    private void RestoreBattleTime()
    {
        if (!ownsPause)
        {
            return;
        }

        Time.timeScale = previousTimeScale;
        ownsPause = false;
    }

    private void ResolveRuntimeReferences()
    {
        if (player == null)
        {
            player = Object.FindFirstObjectByType<Player>();
        }

        if (inventory == null)
        {
            inventory = player != null
                ? player.GetComponent<SkillInventory>()
                : Object.FindFirstObjectByType<SkillInventory>();
        }
    }
}
