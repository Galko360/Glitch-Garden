using UnityEngine;

/// <summary>
/// Paladin support component — runs independently from the attack cycle.
/// Heals the most wounded nearby defender on its own timer.
/// Pair with MeleeAttacker so Paladin can fight AND heal simultaneously.
/// </summary>
public class HealerAura : MonoBehaviour
{
    [Header("Healing")]
    [SerializeField] private int healAmount      = 2;
    [SerializeField] private float healInterval  = 3f;

    [Header("Range")]
    [SerializeField] private float radius        = 3f;
    [SerializeField] private LayerMask defenderLayer;

    private float timer;

    // -------------------------------------------------

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        UnitCombat target = FindMostWounded();
        if (target != null)
            target.Heal(healAmount);

        timer = healInterval;
    }

    // -------------------------------------------------

    private UnitCombat FindMostWounded()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, defenderLayer);

        UnitCombat mostWounded = null;
        float lowestHpRatio    = float.MaxValue;

        foreach (var col in hits)
        {
            UnitCombat unit = col.GetComponentInParent<UnitCombat>();

            // Skip self and anyone already at full HP
            if (unit == null || unit.gameObject == gameObject) continue;
            if (unit.HP >= unit.MaxHP) continue;

            float ratio = (float)unit.HP / unit.MaxHP;
            if (ratio < lowestHpRatio)
            {
                lowestHpRatio = ratio;
                mostWounded   = unit;
            }
        }

        return mostWounded;
    }

    // -------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
