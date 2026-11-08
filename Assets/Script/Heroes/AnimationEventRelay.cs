using UnityEngine;

// Attach to the child GameObject that has the Animator.
// Animation Events call methods here and relay up to the Combatant on the parent.
public class AnimationEventRelay : MonoBehaviour
{
    private Combatant combatant;

    private void Awake()
    {
        combatant = GetComponentInParent<Combatant>();
    }

    public void OnAttackHit()
    {
        combatant?.OnAttackHit();
    }
}
