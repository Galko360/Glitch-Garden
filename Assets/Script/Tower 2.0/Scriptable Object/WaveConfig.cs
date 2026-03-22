using UnityEngine;

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

    /// <summary>Returns a random EnemyData from the list, or null if none assigned.</summary>
    public EnemyData GetRandomEnemy()
    {
        if (possibleEnemies == null || possibleEnemies.Length == 0) return null;
        return possibleEnemies[Random.Range(0, possibleEnemies.Length)];
    }
}
