using System;
using UnityEngine;

/// <summary>
/// Attach to the Base GameObject alongside a BoxCollider2D (set Is Trigger = true) 
/// and a SpriteRenderer component.
/// The collider should span the full height of all lanes.
/// Any Enemy that enters the trigger deals damage to the base.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BaseManager : MonoBehaviour
{
    public static BaseManager Instance { get; private set; }

    [Header("HP")]
    [SerializeField] private int maxHp = 10;

    [Header("Visuals")]
    [Tooltip("Assign sprites in order: Index 0 = Max HP, Last Index = 1 HP (or vice versa depending on your setup).")]
    [SerializeField] private Sprite[] healthSprites;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField, Range(0f, 0.5f)] private float pitchVariance = 0.1f;
    [SerializeField, Range(0f, 0.5f)] private float volumeVariance = 0.1f;
    [SerializeField, Range(0.01f, 3f)] private float baseAudioVolume = 1f;
    [SerializeField, Range(1f, 5f)] private float volumeBoost = 1.5f;

    [Header("Audio Toggle")]
    [field: SerializeField] public bool PlayHitSoundEnabled { get; set; } = true;

    public int Hp { get; private set; }
    public int MaxHp => maxHp;

    public event Action<int, int> OnHpChanged;   // (currentHp, maxHp)
    public event Action OnBaseDied;

    private bool isDead;
    private AudioSource audioSource;

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

        SetupAudioSource();
        UpdateBaseSprite();
    }

    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for the base so it is always heard clearly by the player
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

        PlayHitSound();

        UpdateBaseSprite();

        if (Hp <= 0)
            Die();
    }

    private void PlayHitSound()
    {
        if (!PlayHitSoundEnabled || hitSound == null || audioSource == null) return;

        audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariance, pitchVariance);

        float finalVolume = baseAudioVolume * volumeBoost;
        float randomizedVolume = finalVolume + UnityEngine.Random.Range(-volumeVariance, volumeVariance);

        audioSource.PlayOneShot(hitSound, Mathf.Clamp01(randomizedVolume));
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