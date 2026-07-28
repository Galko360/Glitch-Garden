using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public event Action OnStartAttacking;
    public event Action OnStopAttacking;
    public event Action OnAttack;
    public event Action OnDeath;

    [Header("Data (optional)")]
    [SerializeField] private EnemyData data;

    [Header("Move")]
    [SerializeField] private float speed = 1f;

    [Header("Combat")]
    [StatBar(50)][SerializeField] private int hp = 3;
    [StatBar(20)][SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Gold Settings")]
    [SerializeField] private int goldReward = 5;
    [SerializeField] private GameObject coinPrefab;

    [Header("Death Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float delayBeforeFade = 0.5f;

    [Header("Ranged Attack (leave empty for melee)")]
    [SerializeField] private EnemyBullet projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Raycast (Detect Defenders)")]
    [SerializeField] private float rayDistance = 0.8f;
    [SerializeField] private Vector2 rayBoxSize = new Vector2(0.2f, 0.8f);
    [SerializeField] private LayerMask defenderLayer;
    [SerializeField] private Transform sensorOrigin;

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
                if (projectilePrefab != null)
                    FireProjectile();
                else
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

    private void FireProjectile()
    {
        Transform spawn = firePoint != null ? firePoint : transform;
        EnemyBullet bullet = Instantiate(projectilePrefab, spawn.position, Quaternion.identity);
        bullet.damage = attackDamage;
        bullet.SetDirection(Vector2.right);
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

        OnDeath?.Invoke();

        SpawnCoin();

        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        StartCoroutine(DeathSequenceRoutine());
    }

    private void SpawnCoin()
    {
        if (coinPrefab == null)
        {
            GoldManager.Instance?.AddGold(goldReward);
            return;
        }

        GameObject spawnedCoin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
        if (spawnedCoin.TryGetComponent<Coin>(out var coinScript))
        {
            // Simply pass the gold amount; Coin handles its trajectory and target
            coinScript.Launch(goldReward);
        }
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Transform o = sensorOrigin != null ? sensorOrigin : transform;
        Gizmos.color = projectilePrefab != null ? Color.cyan : Color.yellow;
        Gizmos.DrawWireCube(
            o.position + Vector3.right * rayDistance * 0.5f,
            new Vector3(rayDistance, rayBoxSize.y, 0f)
        );
    }
#endif
}