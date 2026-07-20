using System;
using System.Collections;
using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [Header("Health")]
    [StatBar(100)][SerializeField] private int hp = 3;
    [StatBar(100)][SerializeField] private int maxHp = 3;

    public int HP => hp;
    public int MaxHP => maxHp;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1f;

    [Header("Death Settings")]
    [SerializeField] private float fadeDuration = 1.5f;     // Time it takes to fade to 0 alpha
    [SerializeField] private float delayBeforeFade = 0.5f;  // Time to let the death animation play

    public event Action OnAttack;
    public event Action OnHit;
    public event Action OnDeath;

    private IAttackBehavior attackBehavior;
    private float timer;
    private bool isDead = false;

    private void Awake()
    {
        if (maxHp <= 0) maxHp = hp;
        attackBehavior = GetComponent<IAttackBehavior>();
        attackBehavior?.Init(this);
    }

    private void Update()
    {
        if (isDead) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (attackBehavior != null && attackBehavior.TryAttack())
        {
            OnAttack?.Invoke();
            timer = attackCooldown;
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        hp -= dmg;
        GetComponent<DamageFlash>()?.Flash();
        OnHit?.Invoke();

        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // 1. Immediately fire the death event to start the animation
        OnDeath?.Invoke();

        // 2. Disable physical interactions immediately
        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        // 3. Kick off the fading timeline
        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            foreach (var sr in spriteRenderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        hp = Mathf.Min(hp + amount, maxHp);
    }

    public void SetRow(int row) { }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() { }
#endif
}