using System;
using UnityEngine;

/// <summary>
/// Attach to the Base GameObject alongside a BoxCollider2D (set Is Trigger = true) 
/// and a SpriteRenderer component.
/// The collider should span the full height of all lanes.
/// Any Enemy that enters the trigger deals damage to the base.
/// </summary>
public class BaseManager : MonoBehaviour
{
    public static BaseManager Instance { get; private set; }

    [Header("HP")]
    [SerializeField] private int maxHp = 10;

    [Header("Visuals")]
    [Tooltip("Assign sprites in order: Index 0 = Max HP, Last Index = 1 HP (or vice versa depending on your setup).")]
    [SerializeField] private Sprite[] healthSprites;
    [SerializeField] private SpriteRenderer spriteRenderer;

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

        // Fallback to finding SpriteRenderer automatically if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateBaseSprite();
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

        UpdateBaseSprite();

        if (Hp <= 0)
            Die();
    }

    private void UpdateBaseSprite()
    {
        if (spriteRenderer == null || healthSprites == null || healthSprites.Length == 0)
            return;

        // Map HP to an array index. 
        // Assuming your array has 10 elements where index 0 is full HP (10) and index 9 is 1 HP:
        int spriteIndex = Mathf.Clamp(maxHp - Hp, 0, healthSprites.Length - 1);

        if (healthSprites[spriteIndex] != null)
        {
            spriteRenderer.sprite = healthSprites[spriteIndex];
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("[Base] Base destroyed — GAME OVER");
        OnBaseDied?.Invoke();
    }
}