public interface IAttackBehavior
{
    void Init(UnitCombat owner);

    // Detection only — returns true if a valid target is in range.
    // Resets the cooldown timer and starts the attack animation.
    bool HasTarget();

    // Damage/projectile — called by Animation Event at the hit frame.
    void ExecuteAttack();
}
