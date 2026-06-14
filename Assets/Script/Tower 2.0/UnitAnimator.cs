using UnityEngine;

/// <summary>
/// Bridges UnitCombat events → Animator triggers.
/// Add this to any unit prefab that has an Animator.
/// </summary>
public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // Animator trigger names - change these to match your Animator Controller
    [Header("Trigger Names")]
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string hitTrigger    = "Hit";
    [SerializeField] private string deathTrigger  = "Death";

    private UnitCombat combat;

    private void Awake()
    {
        combat = GetComponent<UnitCombat>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (combat == null) return;
        combat.OnAttack += PlayAttack;
        combat.OnHit    += PlayHit;
        combat.OnDeath  += PlayDeath;
    }

    private void OnDisable()
    {
        if (combat == null) return;
        combat.OnAttack -= PlayAttack;
        combat.OnHit    -= PlayHit;
        combat.OnDeath  -= PlayDeath;
    }

    private void PlayAttack() => animator?.SetTrigger(attackTrigger);
    private void PlayHit()    => animator?.SetTrigger(hitTrigger);
    private void PlayDeath()  => animator?.SetTrigger(deathTrigger);
}
