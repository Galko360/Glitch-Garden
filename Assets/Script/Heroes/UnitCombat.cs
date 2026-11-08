using UnityEngine;

/// <summary>
/// A placed player unit. Health and death are handled by Combatant;
/// this class only drives the attack cycle through its IAttackBehavior.
/// </summary>
public class UnitCombat : Combatant
{
    private IAttackBehavior attackBehavior;

    // -------------------------------------------------

    protected override void Awake()
    {
        base.Awake();

        attackBehavior = GetComponent<IAttackBehavior>();
        attackBehavior?.Init(this);
    }

    private void Update()
    {
        if (isDead) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (attackBehavior != null && attackBehavior.HasTarget())
        {
            timer = attackCooldown;
            RaiseAttack();
            // damage/projectile is dealt by Animation Event calling OnAttackHit()
        }
    }

    public override void OnAttackHit() => attackBehavior?.ExecuteAttack();

    // TODO: lane row is passed in by MergeManager but nothing consumes it yet.
    // Wire up when CombatRegistry goes live, or remove both sides.
    public void SetRow(int row) { }
}
