using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameOverScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text waveReachedText;

    [Header("Scene Management")]
    [SerializeField] private UnityEngine.Object mainMenuSceneAsset; // Allows dragging a Scene file in the Inspector
    [SerializeField, HideInInspector] private string mainMenuSceneName; // Hidden string used at runtime

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

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("Main Menu scene has not been assigned in the GameOverScreen Inspector!", this);
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // This automatically runs in the editor whenever you change a value in the inspector
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (mainMenuSceneAsset != null)
        {
            string path = AssetDatabase.GetAssetPath(mainMenuSceneAsset);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            mainMenuSceneName = name;
        }
        else
        {
            mainMenuSceneName = string.Empty;
        }
    }
#endif
}