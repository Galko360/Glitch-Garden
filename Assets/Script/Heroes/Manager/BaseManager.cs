using System;
using UnityEngine;

/// <summary>
/// Attach to the Base GameObject alongside a BoxCollider2D (set Is Trigger = true).
/// The collider should span the full height of all lanes.
/// Any Enemy that enters the trigger deals damage to the base.
/// </summary>
public class BaseManager : MonoBehaviour
{
    public static BaseManager Instance { get; private set; }

    [Header("HP")]
    [SerializeField] private int maxHp = 10;

    public int Hp { get; private set; }
    public int MaxHp => maxHp;

    public event Action<int, int> OnHpChanged;   // (currentHp, maxHp)
    public event Action OnBaseDied;

    private bool isDead;

    // -------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Hp = maxHp;
    }

    // -------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        TakeDamage(1);
        Destroy(other.gameObject);          // remove the enemy that breached
    }

    // -------------------------------------------------

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        Hp = Mathf.Max(0, Hp - dmg);
        OnHpChanged?.Invoke(Hp, maxHp);

        Debug.Log($"[Base] Hit! HP = {Hp}/{maxHp}");

        if (Hp <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("[Base] Base destroyed — GAME OVER");
        OnBaseDied?.Invoke();
    }
}
