using UnityEngine;

// Attach to the child GameObject that has the Animator.
// Animation Events call methods here and relay up to Enemy / UnitCombat on the parent.
public class AnimationEventRelay : MonoBehaviour
{
    private Enemy enemy;
    private UnitCombat unitCombat;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        unitCombat = GetComponentInParent<UnitCombat>();
    }

    public void OnAttackHit()
    {
        enemy?.OnAttackHit();
        unitCombat?.OnAttackHit();
    }
}
