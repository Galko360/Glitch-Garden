using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Parameter Names")]
    [SerializeField] private string isEngagedParam = "IsEngaged";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string deathTrigger = "Death";
    [SerializeField] private string attackSpeedParam = "AttackSpeed";

    [Header("Attack Speed Sync")]
    [Tooltip("Length of the attack animation clip in seconds")]
    [SerializeField] private float attackClipLength = 1f;

    private Enemy enemy;
    private bool isDead = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (enemy == null || animator == null) return;
        animator.SetFloat(attackSpeedParam, attackClipLength / enemy.AttackCooldown);
    }

    private void OnEnable()
    {
        if (enemy == null) return;
        enemy.OnStartAttacking += HandleEngage;
        enemy.OnStopAttacking += HandleDisengage;
        enemy.OnAttack += HandleAttack;
        enemy.OnDeath += HandleDeath; // Listens to the newly added event
    }

    private void OnDisable()
    {
        if (enemy == null) return;
        enemy.OnStartAttacking -= HandleEngage;
        enemy.OnStopAttacking -= HandleDisengage;
        enemy.OnAttack -= HandleAttack;
        enemy.OnDeath -= HandleDeath;
    }

    private void HandleEngage()
    {
        if (isDead) return;
        animator?.SetBool(isEngagedParam, true);
    }

    private void HandleDisengage()
    {
        if (isDead) return;
        animator?.SetBool(isEngagedParam, false);
    }

    private void HandleAttack()
    {
        if (isDead) return;
        animator?.SetTrigger(attackTrigger);
    }

    private void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        // Instantly shut off walking/combat transitions, then smash the death trigger
        animator?.SetBool(isEngagedParam, false);
        animator?.SetTrigger(deathTrigger);
    }
}