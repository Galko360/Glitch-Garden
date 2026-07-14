public interface IAttackBehavior
{
    /// <summary>
    /// Called once after the unit is created. Use to cache references.
    /// </summary>
    void Init(UnitCombat owner);

    /// <summary>
    /// Called by UnitCombat when the attack timer is ready.
    /// Return true if an attack was actually performed (resets the timer).
    /// Return false if no valid target was found (timer stays ready).
    /// </summary>
    bool TryAttack();
}
