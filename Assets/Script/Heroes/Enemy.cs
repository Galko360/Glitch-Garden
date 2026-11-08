using System;
using UnityEngine;

/// <summary>
/// Walks right until a defender blocks the way, then attacks it.
/// Melee by default; assign a projectilePrefab to make it ranged.
/// </summary>
public class Enemy : Combatant
{
    public event Action OnStartAttacking;
    public event Action OnStopAttacking;

    [Header("Data (optional)")]
    [SerializeField] private EnemyData data;

    [Header("Move")]
    [SerializeField] private float speed = 1f;

    [Header("Combat")]
    [StatBar(20)][SerializeField] private int attackDamage = 1;

    [Header("Gold Settings")]
    [SerializeField] private int goldReward = 5;
    [SerializeField] private GameObject coinPrefab;

    [Header("Ranged Attack (leave empty for melee)")]
    [SerializeField] private EnemyBullet projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Raycast (Detect Defenders)")]
    [SerializeField] private float rayDistance = 0.8f;
    [SerializeField] private Vector2 rayBoxSize = new Vector2(0.2f, 0.8f);
    [SerializeField] private LayerMask defenderLayer;
    [SerializeField] private Transform sensorOrigin;

    private bool isAttacking;

    // -------------------------------------------------

    protected override void Awake()
    {
        base.Awake();

        if (data != null)
        {
            hp = data.hp;
            speed = data.speed;
            attackDamage = data.attackDamage;
            attackCooldown = data.attackCooldown;
            goldReward = data.goldReward;
            maxHp = hp;                 // re-sync after the data override
        }
    }

    private void Update()
    {
        if (isDead) return;

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
                timer = attackCooldown;
                RaiseAttack();
                // damage/projectile is dealt by Animation Event calling OnAttackHit()
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

    // ----- Targeting -----

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

    // ----- Attack -----

    public override void OnAttackHit()
    {
        if (isDead) return;

        if (projectilePrefab != null)
            FireProjectile();
        else
            ScanForDefender()?.TakeDamage(attackDamage);
    }

    private void FireProjectile()
    {
        Transform spawn = firePoint != null ? firePoint : transform;

        EnemyBullet bullet = Instantiate(projectilePrefab, spawn.position, Quaternion.identity);
        bullet.damage = attackDamage;
        bullet.SetDirection(Vector2.right);
    }

    // ----- Death -----

    protected override void Die()
    {
        base.Die();
        SpawnCoin();
    }

    private void SpawnCoin()
    {
        if (coinPrefab == null)
        {
            GoldManager.Instance?.AddGold(goldReward);
            return;
        }

        GameObject spawnedCoin = Instantiate(coinPrefab, transform.position, Quaternion.identity);

        if (spawnedCoin.TryGetComponent<Coin>(out Coin coinScript))
            coinScript.Launch(goldReward);   // Coin handles its own trajectory and target
    }

    // ----- Gizmos -----

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Transform o = sensorOrigin != null ? sensorOrigin : transform;
        Gizmos.color = projectilePrefab != null ? Color.cyan : Color.yellow;

        // Width = rayBoxSize.x + rayDistance to match the actual BoxCast sweep area
        Gizmos.DrawWireCube(
            o.position + Vector3.right * rayDistance * 0.5f,
            new Vector3(rayDistance + rayBoxSize.x, rayBoxSize.y, 0f)
        );
    }
#endif
}
