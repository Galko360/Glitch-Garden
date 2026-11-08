using UnityEngine;

/// <summary>
/// Enemy projectile (e.g. DarkMage) — damages placed player units.
/// </summary>
public class EnemyBullet : Projectile
{
    protected override Combatant FindValidTarget(Collider2D other)
        => other.GetComponentInParent<UnitCombat>();

    protected override void OnHitTarget(Combatant victim)
    {
        victim.TakeDamage(damage);
        Destroy(gameObject);
    }
}
