using System;
using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int hp = 3;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1f;

    // Events for UnitAnimator (and anything else that wants to react)
    public event Action OnAttack;
    public event Action OnHit;
    public event Action OnDeath;

    private IAttackBehavior attackBehavior;
    private float timer;

    // -------------------------------------------------

    private void Awake()
    {
        attackBehavior = GetComponent<IAttackBehavior>();
        attackBehavior?.Init(this);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (attackBehavior != null && attackBehavior.TryAttack())
        {
            OnAttack?.Invoke();
            timer = attackCooldown;
        }
    }

    // -------------------------------------------------

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        GetComponent<DamageFlash>()?.Flash();
        OnHit?.Invoke();

        if (hp <= 0)
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------
    // Kept for MergeManager compatibility
    public void SetRow(int row) { }

    // -------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Each behavior draws its own gizmo now
    }
#endif
}
