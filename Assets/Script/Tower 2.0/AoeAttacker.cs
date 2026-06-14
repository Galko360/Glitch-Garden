using UnityEngine;

/// <summary>
/// Wizard / Reaper - damages ALL enemies inside a radius simultaneously.
/// Only attacks if at least one enemy is in range.
/// </summary>
public class AoeAttacker : MonoBehaviour, IAttackBehavior
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Sensor")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private LayerMask enemyLayer;

    public void Init(UnitCombat owner) { }

    public bool TryAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        if (hits.Length == 0) return false;

        foreach (var col in hits)
        {
            Enemy e = col.GetComponentInParent<Enemy>();
            e?.TakeDamage(damage);
        }

        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
