using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Ranged")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Bullet bulletPrefab;

    [Header("Health")]
    [SerializeField] private int hp = 3;

    [Header("Attack Type")]
    [SerializeField] private AttackType attackType = AttackType.Melee;
    public enum AttackType { Melee, Ranged }

    [Header("Raycast Settings")]
    [SerializeField] private Transform sensorOrigin;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Vector2 rayBoxSize = new Vector2(0.2f, 0.8f);
    [SerializeField] private float rayDistance = 2f;                 // melee small, ranged big

    private float timer;
    private SpriteRenderer sr;


    // -------------------------------------------------

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        Enemy target = ScanForEnemy();
        if (target == null) return;

        if (attackType == AttackType.Melee)
        {
            target.TakeDamage(damage);
            Debug.Log($"{name} MELEE HIT");
        }
        else
        {
            if (bulletPrefab == null) return;

            Transform spawn = firePoint != null ? firePoint : transform;
            Bullet b = Instantiate(bulletPrefab, spawn.position, Quaternion.identity);
            b.damage = damage;
            b.SetTarget(target); // Enemy
            Debug.Log($"{name} SHOOTS");
        }

        timer = attackCooldown;
    }

    // -------------------------------------------------

    private Enemy ScanForEnemy()
    {
        if (sensorOrigin == null)
            sensorOrigin = transform;

        RaycastHit2D hit = Physics2D.BoxCast(
            sensorOrigin.position,
            rayBoxSize,
            0f,
            Vector2.left,      
            rayDistance,
            enemyLayer
        );

        if (!hit) return null;

        return hit.collider.GetComponentInParent<Enemy>();
    }

    // -------------------------------------------------

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        GetComponent<DamageFlash>()?.Flash();

        if (hp <= 0)
            Destroy(gameObject);
    }

    private void FlashRed()
    {
        if (sr == null) return;
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }


    // -------------------------------------------------
    // Dummy function so MergeManager doesn't break
    public void SetRow(int row) { }

    // -------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (sensorOrigin == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            sensorOrigin.position + Vector3.left * rayDistance * 0.5f,
            new Vector3(rayBoxSize.x, rayBoxSize.y, 0)
        );
    }
#endif
}
