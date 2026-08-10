using UnityEngine;

/// <summary>
/// Big Brute - wide melee swing that hits ALL enemies in front at once.
/// </summary>
public class CleaveAttacker : MonoBehaviour, IAttackBehavior
{
    [Header("Damage")]
    [SerializeField] private int damage = 3;

    [Header("Sensor")]
    [SerializeField] private Transform sensorOrigin;
    [SerializeField] private float range = 1.2f;
    [SerializeField] private Vector2 boxSize = new Vector2(0.2f, 2.5f);   // wide on Y to catch full lane
    [SerializeField] private LayerMask enemyLayer;

    public void Init(UnitCombat owner) { }

    public bool HasTarget()
    {
        Transform origin = sensorOrigin != null ? sensorOrigin : transform;
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            origin.position + Vector3.left * (range * 0.5f),
            new Vector2(range, boxSize.y),
            0f,
            enemyLayer
        );
        return hits.Length > 0;
    }

    public void ExecuteAttack()
    {
        Transform origin = sensorOrigin != null ? sensorOrigin : transform;
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            origin.position + Vector3.left * (range * 0.5f),
            new Vector2(range, boxSize.y),
            0f,
            enemyLayer
        );
        foreach (var col in hits)
        {
            Enemy e = col.GetComponentInParent<Enemy>();
            e?.TakeDamage(damage);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform origin = sensorOrigin != null ? sensorOrigin : transform;
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.5f);
        Gizmos.DrawWireCube(
            origin.position + Vector3.left * (range * 0.5f),
            new Vector3(range, boxSize.y, 0f)
        );
    }
#endif
}
