using UnityEngine;

/// <summary>
/// Shared movement and lifetime for every projectile in the game.
/// Subclasses decide who they are allowed to hit and what happens on impact.
/// </summary>
public abstract class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float lifeTime = 3f;

    [Header("Damage")]
    [HideInInspector] public int damage;
    [HideInInspector] public Vector2 direction = Vector2.left;

    // -------------------------------------------------

    private void Start() => Destroy(gameObject, lifeTime);

    private void Update() => transform.Translate(direction * speed * Time.deltaTime);

    public void SetDirection(Vector2 dir) => direction = dir.normalized;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Combatant victim = FindValidTarget(other);
        if (victim == null) return;

        OnHitTarget(victim);
    }

    // -------------------------------------------------

    /// <summary>Return the Combatant on this collider we are allowed to hit, or null.</summary>
    protected abstract Combatant FindValidTarget(Collider2D other);

    /// <summary>Apply damage/effects and decide whether to despawn.</summary>
    protected abstract void OnHitTarget(Combatant victim);
}
