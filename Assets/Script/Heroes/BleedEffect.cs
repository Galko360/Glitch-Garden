using UnityEngine;

/// <summary>
/// Added to a combatant at runtime when hit by a bleed-applying projectile.
/// Ticks damage every interval for the duration, then removes itself.
/// </summary>
public class BleedEffect : MonoBehaviour
{
    private int   damagePerTick;
    private float tickInterval;
    private float lifeTimer;
    private float tickTimer;

    public BleedEffect Init(int dmg, float interval, float duration)
    {
        damagePerTick = dmg;
        tickInterval  = interval;
        lifeTimer     = duration;
        tickTimer     = interval;   // first tick after one interval, not immediately
        return this;
    }

    public void Refresh(float newDuration)
    {
        lifeTimer = newDuration;
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(this);
            return;
        }

        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            GetComponent<Combatant>()?.TakeDamage(damagePerTick);
            tickTimer = tickInterval;
        }
    }
}
