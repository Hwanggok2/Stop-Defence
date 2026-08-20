using UnityEngine;

public sealed class BattleSettingsUI : MonoBehaviour
{
    public void ShowSettings()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
