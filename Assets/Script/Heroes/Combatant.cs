using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Shared base for anything that fights and dies — enemies and player units.
/// Owns health, the attack cooldown timer, and the death/fade sequence.
/// Subclasses add their own movement, targeting and attack logic.
/// </summary>
public abstract class Combatant : MonoBehaviour
{
    public event Action OnAttack;   // attack started (drives the attack animation)
    public event Action OnHit;      // took damage
    public event Action OnDeath;

    [Header("Health")]
    [StatBar(100)][SerializeField] protected int hp = 3;
    [StatBar(100)][SerializeField] protected int maxHp = 0;   // 0 = auto-fill from hp on Awake

    [Header("Attack")]
    [SerializeField] protected float attackCooldown = 1f;

    [Header("Death Settings")]
    [SerializeField] protected float fadeDuration = 1.5f;     // time to fade to 0 alpha
    [SerializeField] protected float delayBeforeFade = 0.5f;  // lets the death animation play

    public int HP => hp;
    public int MaxHP => maxHp;
    public float AttackCooldown => attackCooldown;
    public bool IsDead => isDead;

    protected bool isDead;
    protected float timer;

    private DamageFlash damageFlash;

    // -------------------------------------------------

    protected virtual void Awake()
    {
        if (maxHp <= 0) maxHp = hp;
        damageFlash = GetComponent<DamageFlash>();
    }

    // ----- Damage / Death -----

    public virtual void TakeDamage(int dmg)
    {
        if (isDead) return;

        hp -= dmg;
        damageFlash?.Flash();
        OnHit?.Invoke();

        if (hp <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        hp = Mathf.Min(hp + amount, maxHp);
    }

    protected virtual void Die()
    {
        isDead = true;

        OnDeath?.Invoke();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            foreach (SpriteRenderer sr in renderers)
            {
                if (sr == null) continue;

                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    // ----- Attack -----

    /// <summary>
    /// Raise the "attack started" event. Events can only be invoked from the class
    /// that declares them, so subclasses go through this instead.
    /// </summary>
    protected void RaiseAttack() => OnAttack?.Invoke();

    /// <summary>
    /// Called by an Animation Event (via AnimationEventRelay) on the frame the
    /// weapon connects. Subclasses apply their damage or spawn their projectile here.
    /// </summary>
    public abstract void OnAttackHit();
}
