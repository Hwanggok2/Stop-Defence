using System.Collections.Generic;
using System.Text;
using StopDefence.GameData;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameOverUI : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private SkillDatabase skillDatabase;
    [SerializeField] private SkillInventory skillInventory;
    [SerializeField] private TMP_Text damageSummaryText;
    [SerializeField] private TMP_Text finalScoreText;

    private void Awake()
    {
        BattleStatistics.Reset();
        ResolveRuntimeReferences();
        gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        ResolveRuntimeReferences();
        if (player != null)
        {
            player.Died += Show;
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.Died -= Show;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void Show()
    {
        UpdateBattleResults();
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void UpdateBattleResults()
    {
        if (damageSummaryText != null)
        {
            var summary = new StringBuilder("스킬별 누적 피해\n");
            bool hasSkill = false;

            if (skillInventory != null)
            {
                foreach (OwnedActiveSkill ownedSkill in skillInventory.OwnedActiveSkills)
                {
                    summary.Append(GetSkillDisplayName(ownedSkill.SkillId))
                        .Append("  ")
                        .Append(BattleStatistics.GetSkillDamage(ownedSkill.SkillId).ToString("N0"))
                        .AppendLine();
                    hasSkill = true;
                }
            }

            if (!hasSkill)
            {
                summary.Append("피해 기록 없음");
            }

            damageSummaryText.text = summary.ToString().TrimEnd();
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"최종 점수\n{BattleStatistics.FinalScore:N0}";
        }
    }

    private string GetSkillDisplayName(string skillId)
    {
        return skillDatabase != null && skillDatabase.TryGetSkill(skillId, out SkillData skill)
            ? skill.DisplayName
            : skillId;
    }

    private void ResolveRuntimeReferences()
    {
        if (player == null)
        {
            player = UnityEngine.Object.FindFirstObjectByType<Player>();
        }

        if (skillInventory == null && player != null)
        {
            skillInventory = player.GetComponent<SkillInventory>();
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

public static class BattleStatistics
{
    private static readonly Dictionary<string, float> DamageBySkill =
        new(System.StringComparer.OrdinalIgnoreCase);

    public static float TotalSkillDamage { get; private set; }
    public static int FinalScore => Mathf.RoundToInt(TotalSkillDamage);

    public static void Reset()
    {
        DamageBySkill.Clear();
        TotalSkillDamage = 0f;
    }

    public static void RecordSkillDamage(string skillId, float amount)
    {
        if (string.IsNullOrWhiteSpace(skillId) || amount <= 0f)
        {
            return;
        }

        DamageBySkill.TryGetValue(skillId, out float currentDamage);
        DamageBySkill[skillId] = currentDamage + amount;
        TotalSkillDamage += amount;
    }

    public static float GetSkillDamage(string skillId)
    {
        return !string.IsNullOrWhiteSpace(skillId) &&
               DamageBySkill.TryGetValue(skillId, out float damage)
            ? damage
            : 0f;
    }
}
