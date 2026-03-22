using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float lifeTime = 3f;

    [Header("Damage")]
    [HideInInspector] public int damage;
    [HideInInspector] public Vector2 direction = Vector2.left;

    [Header("AOE (0 = single target)")]
    public float explosionRadius = 0f;
    public LayerMask enemyLayer;

    [Header("Piercing")]
    public bool isPiercing = false;

    private readonly HashSet<Enemy> alreadyHit = new();

    // -------------------------------------------------

    private void Start() => Destroy(gameObject, lifeTime);

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 dir) => direction = dir.normalized;

    // -------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy e = other.GetComponentInParent<Enemy>();
        if (e == null || alreadyHit.Contains(e)) return;

        alreadyHit.Add(e);

        if (explosionRadius > 0f)
        {
            Explode();
            Destroy(gameObject);
        }
        else
        {
            e.TakeDamage(damage);
            if (!isPiercing)
                Destroy(gameObject);
        }
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (var col in hits)
        {
            Enemy e = col.GetComponentInParent<Enemy>();
            e?.TakeDamage(damage);
        }
    }

    // -------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (explosionRadius <= 0f) return;
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}
