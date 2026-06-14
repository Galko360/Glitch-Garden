using UnityEngine;

/// <summary>
/// Archer / Rogue - fires a bullet at the nearest enemy in a long range.
/// </summary>
public class RangedAttacker : MonoBehaviour, IAttackBehavior
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Projectile")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Sensor")]
    [SerializeField] private Transform sensorOrigin;
    [SerializeField] private Vector2 boxSize = new Vector2(0.2f, 0.8f);
    [SerializeField] private float range = 5f;
    [SerializeField] private LayerMask enemyLayer;

    public void Init(UnitCombat owner) { }

    public bool TryAttack()
    {
        if (bulletPrefab == null) return false;

        Enemy target = Scan();
        if (target == null) return false;

        Transform spawn = firePoint != null ? firePoint : transform;
        Bullet b = Instantiate(bulletPrefab, spawn.position, Quaternion.identity);
        b.damage = damage;
        b.SetDirection(Vector2.left);
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            origin.position + Vector3.left * range * 0.5f,
            new Vector3(range, boxSize.y, 0f)
        );
    }
#endif
}
