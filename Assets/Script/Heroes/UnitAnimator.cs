using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Trigger Names")]
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string deathTrigger = "Death";

    private UnitCombat combat;
    private bool isDead = false; // Prevents other animations from overriding death

    private void Awake()
    {
        combat = GetComponent<UnitCombat>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (combat == null) combat = GetComponent<UnitCombat>();
        if (combat == null) return;

        combat.OnAttack += PlayAttack;
        combat.OnHit += PlayHit;
        combat.OnDeath += PlayDeath;
    }

    private void OnDisable()
    {
        if (combat == null) return;
        combat.OnAttack -= PlayAttack;
        combat.OnHit -= PlayHit;
        combat.OnDeath -= PlayDeath;
    }

    private void PlayAttack()
    {
        if (isDead) return;
        animator?.SetTrigger(attackTrigger);
    }

    private void PlayHit()
    {
        if (isDead) return;
        animator?.SetTrigger(hitTrigger);
    }

    private void PlayDeath()
    {
        if (isDead) return;
        isDead = true; // Locks the animator down completely

        animator?.SetTrigger(deathTrigger);
    }
}