using UnityEngine;

/// <summary>
/// Add this component to a bullet prefab to apply bleed on hit.
/// Works automatically with Bullet.cs — no extra setup needed.
/// </summary>
public class BleedOnHit : MonoBehaviour
{
    [SerializeField] private int damagePerTick  = 1;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float duration     = 3f;

    public void Apply(Enemy enemy)
    {
        if (enemy == null) return;

        // If already bleeding, just refresh the duration
        BleedEffect existing = enemy.GetComponent<BleedEffect>();
        if (existing != null)
            existing.Refresh(duration);
        else
        {
            BleedEffect bleed = enemy.gameObject.AddComponent<BleedEffect>();
            bleed.Init(damagePerTick, tickInterval, duration);
        }
    }
}
