using UnityEngine;

/// <summary>
/// ArchMage - fires a spread of fireballs forward simultaneously.
/// Pair with a fireball bullet prefab (explosionRadius > 0).
/// </summary>
public class MultiFireballAttacker : MonoBehaviour, IAttackBehavior
{
    [Header("Damage")]
    [SerializeField] private int damage = 2;

    [Header("Projectile")]
    [SerializeField] private Bullet fireballPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Spread")]
    [SerializeField] private int fireballCount = 3;
    [SerializeField] private float spreadAngle = 30f;  // total spread in degrees

    [Header("Sensor (needs enemy in range to trigger)")]
    [SerializeField] private float range = 6f;
    [SerializeField] private LayerMask enemyLayer;

    public void Init(UnitCombat owner) { }

    public bool TryAttack()
    {
        if (fireballPrefab == null) return false;

        // Only fire if there's at least one enemy in range
        Collider2D hit = Physics2D.OverlapCircle(transform.position, range, enemyLayer);
        if (hit == null) return false;

        Transform spawnPoint = firePoint != null ? firePoint : transform;

        // Spread fireballs evenly across the spread angle
        float startAngle = -spreadAngle / 2f;
        float step = fireballCount > 1 ? spreadAngle / (fireballCount - 1) : 0f;

        for (int i = 0; i < fireballCount; i++)
        {
            float angle = startAngle + step * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector2.left;

            Bullet b = Instantiate(fireballPrefab, spawnPoint.position, Quaternion.identity);
            b.damage = damage;
            b.SetDirection(dir);
        }

        return true;
    }

    // -------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
#endif
}
