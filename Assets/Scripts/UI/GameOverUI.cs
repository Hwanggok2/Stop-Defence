using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameOverUI : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
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
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
