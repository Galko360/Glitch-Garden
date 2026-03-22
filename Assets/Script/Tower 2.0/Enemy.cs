using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Data (optional — overrides stats below if assigned)")]
    [SerializeField] private EnemyData data;

    [Header("Move")]
    [SerializeField] private float speed = 1f;

    [Header("Combat")]
    [SerializeField] private int hp = 3;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Gold")]
    [SerializeField] private int goldReward = 5;

    [Header("Raycast (Detect Defenders)")]
    [SerializeField] private float rayDistance = 0.8f;
    [SerializeField] private Vector2 rayBoxSize = new Vector2(0.2f, 0.8f);
    [SerializeField] private LayerMask defenderLayer;
    [SerializeField] private Transform sensorOrigin;

    private float timer;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();

        // If a data SO is assigned, let it override the serialized fields
        if (data != null)
        {
            hp              = data.hp;
            speed           = data.speed;
            attackDamage    = data.attackDamage;
            attackCooldown  = data.attackCooldown;
            goldReward      = data.goldReward;
        }
    }


    private void Update()
    {
        if (hp <= 0) return;

        timer -= Time.deltaTime;

        // Scan RIGHT for defenders (enemies move Left -> Right)
        UnitCombat defender = ScanForDefender();

        if (defender != null)
        {
            // Engage: stop moving and attack on cooldown
            if (timer <= 0f)
            {
                Debug.Log($"{name} attacks {defender.name}");
                defender.TakeDamage(attackDamage);
                timer = attackCooldown;
            }
            return;
        }

        // No defender ahead -> move forward
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
        hp -= dmg;
        GetComponent<DamageFlash>()?.Flash();

        if (hp <= 0)
        {
            GoldManager.Instance?.AddGold(goldReward);
            Destroy(gameObject);
        }
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
