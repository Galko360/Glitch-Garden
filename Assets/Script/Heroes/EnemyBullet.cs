using UnityEngine;

/// <summary>
/// Projectile fired by ranged enemies (e.g. DarkMage).
/// Mirrors Bullet.cs but damages UnitCombat (defenders) instead of Enemy.
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    public float lifeTime = 3f;

    [Header("Damage")]
    [HideInInspector] public int damage;
    [HideInInspector] public Vector2 direction = Vector2.left;

    private void Start() => Destroy(gameObject, lifeTime);

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 dir) => direction = dir.normalized;

    private void OnTriggerEnter2D(Collider2D other)
    {
        UnitCombat defender = other.GetComponentInParent<UnitCombat>();
        if (defender == null) return;

        defender.TakeDamage(damage);
        Destroy(gameObject);
    }
}
