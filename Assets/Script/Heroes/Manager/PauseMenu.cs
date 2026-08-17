using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject tutorialPanel;

    [Header("Scene Management")]
    [SerializeField] private UnityEngine.Object mainMenuSceneAsset; // Allows dragging a Scene file in the Inspector
    [SerializeField, HideInInspector] private string mainMenuSceneName; // Hidden string used at runtime

    private bool isPaused = false;

    // -------------------------------------------------

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // -------------------------------------------------

    public void TogglePause()
    {
        isPaused = !isPaused;

        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        // Close sub-panels when unpausing
        if (!isPaused)
        {
            settingsPanel?.SetActive(false);
            tutorialPanel?.SetActive(false);
        }
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        settingsPanel?.SetActive(false);
        tutorialPanel?.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel?.SetActive(true);
    }

    public void OpenTutorial()
    {
        pausePanel.SetActive(false);
        tutorialPanel?.SetActive(true);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Always reset time scale before loading scenes
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("Main Menu scene has not been assigned in the PauseMenu Inspector!", this);
        }
    }

    public void ExitGame()
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