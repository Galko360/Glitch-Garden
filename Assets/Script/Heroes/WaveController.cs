using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaveController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<EnemySpawner> spawners = new();

    [Header("Hand-crafted Waves")]
    [SerializeField] private List<WaveConfig> waves = new();
    [SerializeField] private bool autoStart = true;

    [Header("Prep Phase (before wave 1)")]
    [SerializeField] private float prepDuration = 10f;

    [Header("Break Between Waves")]
    [SerializeField] private float breakDuration = 8f;

    [Header("Endless Scaling (kicks in after hand-crafted waves)")]
    [SerializeField] private int baseEnemyCount = 10;
    [SerializeField] private int enemyCountIncreasePerWave = 3;
    [SerializeField] private float baseSpawnInterval = 0.75f;
    [SerializeField] private float minSpawnInterval = 0.2f;
    [SerializeField] private float spawnIntervalDecreasePerWave = 0.05f;

    public int CurrentWave { get; private set; } = 0;
    public bool IsBreak { get; private set; } = false;
    public bool IsPrepPhase { get; private set; } = false;
    public float BreakTimeRemaining { get; private set; } = 0f;

    public event Action OnPreparationStarted;
    public event Action OnWaveStarted;
    public event Action OnWaveFinished;

    private Coroutine running;
    private int spawnIndex = 0;

    // -------------------------------------------------

    private void Awake()
    {
        if (spawners.Count == 0)
        {
            spawners.AddRange(
                FindObjectsByType<EnemySpawner>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
            );
        }
    }

    private void Start()
    {
        if (autoStart)
            running = StartCoroutine(RunEndless());
    }

    // -------------------------------------------------

    private IEnumerator RunEndless()
    {
        if (spawners.Count == 0)
        {
            Debug.LogError("[Wave] No spawners found.");
            yield break;
        }

        // ────────────────────────────────────────────────────────
        // Initial preparation
        // ────────────────────────────────────────────────────────

        IsPrepPhase = true;
        IsBreak = true;
        BreakTimeRemaining = prepDuration;

        OnPreparationStarted?.Invoke();

        while (BreakTimeRemaining > 0f)
        {
            BreakTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        IsPrepPhase = false;
        IsBreak = false;

        // ────────────────────────────────────────────────────────
        // Endless wave cycle
        // ────────────────────────────────────────────────────────

        while (true)
        {
            CurrentWave++;

            WaveConfig config = BuildWaveConfig(CurrentWave);

            Debug.Log(
                $"[Wave] Starting wave {CurrentWave} | " +
                $"enemies={config.enemyCount} " +
                $"interval={config.spawnInterval:F2}s"
            );

            OnWaveStarted?.Invoke();

            yield return StartCoroutine(RunWave(config));

            OnWaveFinished?.Invoke();

            Debug.Log(
                $"[Wave] Wave {CurrentWave} finished. " +
                $"Break for {breakDuration}s"
            );

            // ────────────────────────────────────────────────────
            // Break / preparation for next wave
            // ────────────────────────────────────────────────────

            IsBreak = true;
            BreakTimeRemaining = breakDuration;

            // The break is the presentation/preparation period.
            OnPreparationStarted?.Invoke();

            while (BreakTimeRemaining > 0f)
            {
                BreakTimeRemaining -= Time.deltaTime;
                yield return null;
            }

            IsBreak = false;
        }
    }

    // -------------------------------------------------

    private IEnumerator RunWave(WaveConfig config)
    {
        for (int i = 0; i < config.enemyCount; i++)
        {
            EnemySpawner spawner = PickSpawner(config.roundRobin);

            spawner.SpawnEnemy(config.GetRandomEnemy());

            yield return new WaitForSeconds(config.spawnInterval);
        }
    }

    // -------------------------------------------------

    /// <summary>
    /// Returns the hand-crafted WaveConfig if one exists for this wave number,
    /// otherwise procedurally generates one that scales with wave number.
    /// </summary>
    private WaveConfig BuildWaveConfig(int waveNumber)
    {
        int index = waveNumber - 1;

        if (index < waves.Count && waves[index] != null)
            return waves[index];

        WaveConfig generated = ScriptableObject.CreateInstance<WaveConfig>();

        int extraWaves = waveNumber - waves.Count;

        generated.enemyCount =
            baseEnemyCount +
            (extraWaves * enemyCountIncreasePerWave);

        generated.spawnInterval =
            Mathf.Max(
                minSpawnInterval,
                baseSpawnInterval -
                (extraWaves * spawnIntervalDecreasePerWave)
            );

        generated.roundRobin = false;

        return generated;
    }

    // -------------------------------------------------

    private EnemySpawner PickSpawner(bool roundRobin)
    {
        if (!roundRobin)
            return spawners[UnityEngine.Random.Range(0, spawners.Count)];

        return spawners[spawnIndex++ % spawners.Count];
    }

    // -------------------------------------------------

    [ContextMenu("Skip Break")]
    public void SkipBreak()
    {
        BreakTimeRemaining = 0f;
    }
}