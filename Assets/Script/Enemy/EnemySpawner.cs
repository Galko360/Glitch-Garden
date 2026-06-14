using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy defaultPrefab;

    private int rowIndex;

    public void SetRow(int row) => rowIndex = row;

    [ContextMenu("Spawn Enemy")]
    public Enemy SpawnEnemy(EnemyData data = null)
    {
        // Use the data's prefab if provided, otherwise fall back to the default
        Enemy prefabToUse = (data != null && data.prefab != null) ? data.prefab : defaultPrefab;

        if (prefabToUse == null)
        {
            Debug.LogError($"{name}: No enemy prefab assigned");
            return null;
        }

        Enemy enemy = Instantiate(prefabToUse, transform.position, Quaternion.identity);
        return enemy;
    }
}
