using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public event Action OnStartAttacking;
    public event Action OnStopAttacking;
    public event Action OnAttack;
    public event Action OnDeath; // Added to properly bridge to EnemyAnimator

    [Header("Data (optional — overrides stats below if assigned)")]
    [SerializeField] private EnemyData data;

    [Header("Move")]
    [SerializeField] private float speed = 1f;

    [Header("Combat")]
    [StatBar(50)][SerializeField] private int hp = 3;
    [StatBar(20)][SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Gold")]
    [SerializeField] private int goldReward = 5;

    [Header("Raycast (Detect Defenders)")]
    [SerializeField] private float rayDistance = 0.8f;
    [SerializeField] private Vector2 rayBoxSize = new Vector2(0.2f, 0.8f);
    [SerializeField] private LayerMask defenderLayer;
    [SerializeField] private Transform sensorOrigin;

    [Header("Death Settings")]
    [SerializeField] private float fadeDuration = 1.5f;     // Time it takes to fade to 0 alpha
    [SerializeField] private float delayBeforeFade = 0.5f;  // Time to let the death animation play

    private float timer;
    private bool isAttacking;
    private bool isDead = false;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();

        if (data != null)
        {
            hp = data.hp;
            speed = data.speed;
            attackDamage = data.attackDamage;
            attackCooldown = data.attackCooldown;
            goldReward = data.goldReward;
        }
    }

    private void Update()
    {
        if (isDead || hp <= 0) return;

        timer -= Time.deltaTime;

        UnitCombat defender = ScanForDefender();

        if (defender != null)
        {
            if (!isAttacking)
            {
                isAttacking = true;
                OnStartAttacking?.Invoke();
            }

            if (timer <= 0f)
            {
                defender.TakeDamage(attackDamage);
                timer = attackCooldown;
                OnAttack?.Invoke();
            }
            return;
        }

        if (isAttacking)
        {
            isAttacking = false;
            OnStopAttacking?.Invoke();
        }

        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private UnitCombat ScanForDefender()
    {
        if (sensorOrigin == null) sensorOrigin = transform;

        RaycastHit2D hit = Physics2D.BoxCast(
            sensorOrigin.position,
            rayBoxSize,
            0f,
            Vector2.right,
            rayDistance,
            defenderLayer
        );

        if (!hit) return null;

        return hit.collider.GetComponentInParent<UnitCombat>();
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        hp -= dmg;
        GetComponent<DamageFlash>()?.Flash();

        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // 1. Immediately fire the death event to trigger the animation
        OnDeath?.Invoke();

        // 2. Award gold instantly
        GoldManager.Instance?.AddGold(goldReward);

        // 3. Disable physical interactions immediately
        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        // 4. Kick off the fading timeline
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

            foreach (var renderer in spriteRenderers)
            {
                if (renderer != null)
                {
                    Color c = renderer.color;
                    c.a = alpha;
                    renderer.color = c;
                }
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    private void FlashRed()
    {
        if (sr == null || isDead) return;
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform o = sensorOrigin != null ? sensorOrigin : transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            o.position + Vector3.right * rayDistance * 0.5f,
            new Vector3(rayBoxSize.x, rayBoxSize.y, 0f)
        );
    }
#endif
}