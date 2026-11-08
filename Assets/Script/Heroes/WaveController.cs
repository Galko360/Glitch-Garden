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
    [Tooltip("Cycle lanes in order instead of picking at random during endless waves.")]
    [SerializeField] private bool endlessRoundRobin = false;

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

            WaveSettings config = BuildWaveSettings(CurrentWave);

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

    private IEnumerator RunWave(WaveSettings config)
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
    /// Returns the hand-crafted wave if one exists for this wave number,
    /// otherwise procedurally generates one that scales with wave number.
    /// </summary>
    private WaveSettings BuildWaveSettings(int waveNumber)
    {
        int index = waveNumber - 1;

        if (index < waves.Count && waves[index] != null)
            return waves[index].ToSettings();

        // 0 for the first generated wave, so it uses the base values as-is
        int extraWaves = Mathf.Max(0, waveNumber - waves.Count - 1);

        return new WaveSettings
        {
            enemyCount = baseEnemyCount + (extraWaves * enemyCountIncreasePerWave),

            spawnInterval = Mathf.Max(
                minSpawnInterval,
                baseSpawnInterval - (extraWaves * spawnIntervalDecreasePerWave)
            ),

            roundRobin = endlessRoundRobin,
            possibleEnemies = InheritEnemyPool()
        };
    }

    /// <summary>
    /// Copy the enemy pool from the last hand-crafted wave.
    /// Cloned so a generated wave can never mutate the saved asset.
    /// </summary>
    private EnemyData[] InheritEnemyPool()
    {
        if (waves.Count == 0) return null;

        WaveConfig last = waves[waves.Count - 1];
        if (last == null || last.possibleEnemies == null) return null;

        return (EnemyData[])last.possibleEnemies.Clone();
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