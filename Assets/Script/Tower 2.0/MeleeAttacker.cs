using UnityEngine;

/// <summary>
/// Warrior / Paladin - hits the nearest enemy in a short forward range.
/// </summary>
public class MeleeAttacker : MonoBehaviour, IAttackBehavior
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Sensor")]
    [SerializeField] private Transform sensorOrigin;
    [SerializeField] private Vector2 boxSize = new Vector2(0.2f, 0.8f);
    [SerializeField] private float range = 1.5f;
    [SerializeField] private LayerMask enemyLayer;

    public void Init(UnitCombat owner) { }

    public bool TryAttack()
    {
        Enemy target = Scan();
        if (target == null) return false;

        target.TakeDamage(damage);
        return true;
    }

    private Enemy Scan()
    {
        Transform origin = sensorOrigin != null ? sensorOrigin : transform;

        RaycastHit2D hit = Physics2D.BoxCast(
            origin.position,
            boxSize,
            0f,
            Vector2.left,
            range,
            enemyLayer
        );

        if (!hit) return null;
        return hit.collider.GetComponentInParent<Enemy>();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform origin = sensorOrigin != null ? sensorOrigin : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            origin.position + Vector3.left * range * 0.5f,
            new Vector3(range, boxSize.y, 0f)
        );
    }
#endif
}
