using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player projectile — damages enemies.
/// Supports area damage, piercing, and bleed via a BleedOnHit component.
/// </summary>
public class Bullet : Projectile
{
    [Header("AOE (0 = single target)")]
    public float explosionRadius = 0f;
    public LayerMask enemyLayer;

    [Header("Piercing")]
    public bool isPiercing = false;

    private readonly HashSet<Combatant> alreadyHit = new();

    // -------------------------------------------------

    protected override Combatant FindValidTarget(Collider2D other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null || alreadyHit.Contains(enemy)) return null;

        return enemy;
    }

    protected override void OnHitTarget(Combatant victim)
    {
        alreadyHit.Add(victim);

        if (explosionRadius > 0f)
        {
            Explode();
            Destroy(gameObject);
            return;
        }

        victim.TakeDamage(damage);
        GetComponent<BleedOnHit>()?.Apply(victim);

        if (!isPiercing)
            Destroy(gameObject);
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

        foreach (Collider2D col in hits)
            col.GetComponentInParent<Enemy>()?.TakeDamage(damage);
    }

    // -------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (explosionRadius <= 0f) return;

        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}
