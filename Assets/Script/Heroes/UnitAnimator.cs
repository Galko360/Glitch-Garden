using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Trigger Names")]
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string deathTrigger = "Death";
    [SerializeField] private string attackSpeedParam = "AttackSpeed";

    [Header("Attack Speed Sync")]
    [Tooltip("Length of the attack animation clip in seconds")]
    [SerializeField] private float attackClipLength = 1f;

    private UnitCombat combat;
    private bool isDead = false;

    private void Awake()
    {
        combat = GetComponent<UnitCombat>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (combat == null || animator == null) return;
        animator.SetFloat(attackSpeedParam, attackClipLength / combat.AttackCooldown);
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