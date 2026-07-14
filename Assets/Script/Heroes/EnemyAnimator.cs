using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Parameter Names")]
    [SerializeField] private string isEngagedParam  = "IsEngaged";   // bool - stopped to fight
    [SerializeField] private string attackTrigger   = "Attack";       // trigger - plays attack swing

    private Enemy enemy;

    // -------------------------------------------------

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (enemy == null) return;
        enemy.OnStartAttacking += HandleEngage;
        enemy.OnStopAttacking  += HandleDisengage;
        enemy.OnAttack         += HandleAttack;
    }

    private void OnDisable()
    {
        if (enemy == null) return;
        enemy.OnStartAttacking -= HandleEngage;
        enemy.OnStopAttacking  -= HandleDisengage;
        enemy.OnAttack         -= HandleAttack;
    }

    // -------------------------------------------------

    private void HandleEngage()    => animator?.SetBool(isEngagedParam, true);
    private void HandleDisengage() => animator?.SetBool(isEngagedParam, false);
    private void HandleAttack()    => animator?.SetTrigger(attackTrigger);
}
