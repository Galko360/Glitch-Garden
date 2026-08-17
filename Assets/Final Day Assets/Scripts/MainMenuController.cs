#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("The credits panel.")]
    [SerializeField] private GameObject creditsPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button closeCreditsButton;
    [SerializeField] private Button quitButton;

    [Header("Scene Settings")]
#if UNITY_EDITOR
    [Tooltip("Drag and drop your target game scene asset here.")]
    [SerializeField] private SceneAsset gameSceneAsset;
#endif
    [SerializeField, HideInInspector] private string gameSceneName;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (gameSceneAsset != null)
        {
            gameSceneName = gameSceneAsset.name;
        }
    }
#endif

    private void Start()
    {
        // Ensure credits panel is closed when the game starts
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    /// <summary>
    /// Loads the main game scene assigned in the inspector.
    /// </summary>
    public void StartGame()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Game Scene is not assigned in the Inspector!");
        }
    }

    /// <summary>
    /// Opens the credits panel.
    /// </summary>
    public void OpenCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    /// <summary>
    /// Closes the credits panel.
    /// </summary>
    public void CloseCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit Game requested.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}