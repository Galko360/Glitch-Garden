using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject tutorialPanel;

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

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
