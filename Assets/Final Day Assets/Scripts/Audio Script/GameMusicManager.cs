using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class GameMusicManager : MonoBehaviour
{
    public static GameMusicManager Instance { get; private set; }

    [Header("Music Tracks")]
    [SerializeField] private MusicTrackLibrary tracks = new();

    [Header("Audio")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

    [Header("Gameplay Scene")]
#if UNITY_EDITOR
    [Tooltip("The scene containing the base defense gameplay.")]
    [SerializeField] private SceneAsset gameplaySceneAsset;
#endif

    [SerializeField, HideInInspector] private string gameplaySceneName;

    private AudioSource audioSource;
    private IMusicPlayback playback;
    private IMusicState currentState;

    private WaveController currentWaveController;
    private BaseManager currentBaseManager;

    private bool hasStartedMusicLifecycle;

    // ---------------------------------------------------------
    // Unity Lifecycle
    // ---------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SetupAudioSource();

        playback = new UnityMusicPlayback(audioSource, this);

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        hasStartedMusicLifecycle = true;

        Scene currentScene = SceneManager.GetActiveScene();

        if (IsGameplayScene(currentScene))
        {
            HandleGameplaySceneLoaded();
        }
        else
        {
            ChangeState(CreateLaunchState());
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        UnbindGameplayEvents();

        playback?.Stop();

        if (Instance == this)
            Instance = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (gameplaySceneAsset != null)
            gameplaySceneName = gameplaySceneAsset.name;
    }
#endif

    // ---------------------------------------------------------
    // Audio Setup
    // ---------------------------------------------------------

    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = masterVolume;
        audioSource.spatialBlend = 0f;
    }

    // ---------------------------------------------------------
    // Scene Handling
    // ---------------------------------------------------------

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasStartedMusicLifecycle)
            return;

        if (IsGameplayScene(scene))
        {
            HandleGameplaySceneLoaded();
        }
        else
        {
            HandleNonGameplaySceneLoaded();
        }
    }

    private void HandleGameplaySceneLoaded()
    {
        BindGameplayEvents();

        ChangeToPrepMusic();
    }

    private void HandleNonGameplaySceneLoaded()
    {
        // The only non-gameplay scene currently expected is the main menu.
        // Game Launch is never replayed during scene changes.
        ChangeToMainMenuMusic();
    }

    private bool IsGameplayScene(Scene scene)
    {
        if (string.IsNullOrEmpty(gameplaySceneName))
            return false;

        return scene.name == gameplaySceneName;
    }

    // ---------------------------------------------------------
    // Gameplay Event Binding
    // ---------------------------------------------------------

    private void BindGameplayEvents()
    {
        UnbindGameplayEvents();

        currentWaveController = FindFirstObjectByType<WaveController>();
        currentBaseManager = FindFirstObjectByType<BaseManager>();

        if (currentWaveController != null)
        {
            currentWaveController.OnPreparationStarted += HandlePreparationStarted;
            currentWaveController.OnWaveStarted += HandleWaveStarted;
            currentWaveController.OnWaveFinished += HandleWaveFinished;
        }
        else
        {
            Debug.LogWarning(
                "[Music] No WaveController found in the gameplay scene."
            );
        }

        if (currentBaseManager != null)
        {
            currentBaseManager.OnBaseDied += HandleBaseDied;
        }
        else
        {
            Debug.LogWarning(
                "[Music] No BaseManager found in the gameplay scene."
            );
        }
    }

    private void UnbindGameplayEvents()
    {
        if (currentWaveController != null)
        {
            currentWaveController.OnPreparationStarted -= HandlePreparationStarted;
            currentWaveController.OnWaveStarted -= HandleWaveStarted;
            currentWaveController.OnWaveFinished -= HandleWaveFinished;
        }

        if (currentBaseManager != null)
        {
            currentBaseManager.OnBaseDied -= HandleBaseDied;
        }

        currentWaveController = null;
        currentBaseManager = null;
    }

    // ---------------------------------------------------------
    // Wave Events
    // ---------------------------------------------------------

    private void HandlePreparationStarted()
    {
        ChangeToPrepMusic();
    }

    private void HandleWaveStarted()
    {
        ChangeToWaveMusic();
    }

    private void HandleWaveFinished()
    {
        ChangeToPrepMusic();
    }

    // ---------------------------------------------------------
    // Base Death
    // ---------------------------------------------------------

    private void HandleBaseDied()
    {
        UnbindGameplayEvents();
        ChangeToGameOverMusic();
    }

    // ---------------------------------------------------------
    // Launch Sequence
    // ---------------------------------------------------------

    public void HandleLaunchFinished(LaunchMusicState state)
    {
        if (!IsCurrentState(state))
            return;

        Scene currentScene = SceneManager.GetActiveScene();

        if (IsGameplayScene(currentScene))
            ChangeToPrepMusic();
        else
            ChangeToMainMenuMusic();
    }

    // ---------------------------------------------------------
    // Wave Presentation
    // ---------------------------------------------------------

    public void HandleWaveStartPresentationFinished(WaveMusicState state)
    {
        if (!IsCurrentState(state))
            return;

        if (currentWaveController == null)
            return;

        state.StartGameLoop();
    }

    // ---------------------------------------------------------
    // State Creation
    // ---------------------------------------------------------

    private IMusicState CreateLaunchState()
    {
        return new LaunchMusicState(this, playback, tracks);
    }

    private IMusicState CreateMainMenuState()
    {
        return new MainMenuMusicState(this, playback, tracks);
    }

    private IMusicState CreatePrepState()
    {
        return new PrepMusicState(this, playback, tracks);
    }

    private WaveMusicState CreateWaveState()
    {
        return new WaveMusicState(this, playback, tracks);
    }

    private IMusicState CreateGameOverState()
    {
        return new GameOverMusicState(this, playback, tracks);
    }

    // ---------------------------------------------------------
    // State Transitions
    // ---------------------------------------------------------

    private void ChangeToMainMenuMusic()
    {
        ChangeState(CreateMainMenuState());
    }

    private void ChangeToPrepMusic()
    {
        ChangeState(CreatePrepState());
    }

    private void ChangeToWaveMusic()
    {
        ChangeState(CreateWaveState());
    }

    private void ChangeToGameOverMusic()
    {
        ChangeState(CreateGameOverState());
    }

    private void ChangeState(IMusicState newState)
    {
        if (newState == null)
            return;

        if (currentState != null &&
            currentState.GetType() == newState.GetType())
        {
            return;
        }

        currentState?.Exit();

        currentState = newState;

        currentState.Enter();
    }

    private bool IsCurrentState(IMusicState state)
    {
        return ReferenceEquals(currentState, state);
    }

    // ---------------------------------------------------------
    // Debugging
    // ---------------------------------------------------------

    public string CurrentMusicState =>
        currentState != null ? currentState.Name : "None";

    public bool IsMusicPlaying =>
        playback != null && playback.IsPlaying;
}