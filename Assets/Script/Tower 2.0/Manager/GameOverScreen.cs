using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text waveReachedText;

    // -------------------------------------------------

    private WaveController waveController;

    private void Awake()
    {
        gameOverPanel.SetActive(false);
        waveController = FindFirstObjectByType<WaveController>();
    }

    private void Start()
    {
        if (BaseManager.Instance != null)
            BaseManager.Instance.OnBaseDied += HandleBaseDied;
    }

    private void OnDestroy()
    {
        if (BaseManager.Instance != null)
            BaseManager.Instance.OnBaseDied -= HandleBaseDied;
    }

    private void HandleBaseDied()
    {
        int wave = waveController != null ? waveController.CurrentWave : 0;
        ShowGameOver(wave);
    }

    // -------------------------------------------------

    public void ShowGameOver(int waveReached)
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);

        if (waveReachedText != null)
            waveReachedText.text = $"You reached\nWave {waveReached}";
    }

    // -------------------------------------------------

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
