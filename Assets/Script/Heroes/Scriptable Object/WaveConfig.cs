using UnityEngine;

/// <summary>
/// Runtime wave data. Plain C# — unlike a ScriptableObject it is garbage collected
/// normally, so procedurally generated endless waves cannot leak.
/// </summary>
public class WaveSettings
{
    public int enemyCount;
    public float spawnInterval;
    public bool roundRobin;
    public EnemyData[] possibleEnemies;

    /// <summary>Returns a random EnemyData from the pool, or null if none assigned.</summary>
    public EnemyData GetRandomEnemy()
    {
        if (possibleEnemies == null || possibleEnemies.Length == 0) return null;
        return possibleEnemies[Random.Range(0, possibleEnemies.Length)];
    }
}

/// <summary>
/// Designer-authored wave asset. Converted to WaveSettings before being used at runtime.
/// </summary>
[CreateAssetMenu(menuName = "Tower/Wave Config", fileName = "WaveConfig")]
public class WaveConfig : ScriptableObject
{
    [Header("Wave")]
    public int enemyCount = 10;
    public float spawnInterval = 0.75f;

    [Header("Lane Distribution")]
    public bool roundRobin = true;

    [Header("Enemy Types (if empty, spawner uses its default prefab)")]
    public EnemyData[] possibleEnemies;

    /// <summary>Snapshot this asset as runtime data.</summary>
    public WaveSettings ToSettings() => new WaveSettings
    {
        enemyCount      = enemyCount,
        spawnInterval   = spawnInterval,
        roundRobin      = roundRobin,
        possibleEnemies = possibleEnemies
    };
}
