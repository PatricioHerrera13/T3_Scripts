using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyCore enemyCore;

    // Nombres de los parámetros (tienen que coincidir exactamente con los del Animator Controller)
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DeathHash = Animator.StringToHash("Death");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (enemyCore == null)
            enemyCore = GetComponent<EnemyCore>();
        if (enemyCore == null)
            enemyCore = GetComponentInParent<EnemyCore>();
    }

    private void OnEnable()
    {
        if (enemyCore != null)
            enemyCore.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (enemyCore != null)
            enemyCore.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(EnemyState newState)
    {
        if (animator == null) return;

        switch (newState)
        {
            case EnemyState.Patrol:
            case EnemyState.Chase:
                animator.SetBool(IsMovingHash, true);
                break;

            case EnemyState.Attack:
                animator.SetBool(IsMovingHash, false);
                animator.SetTrigger(AttackHash);
                break;

            case EnemyState.Death:
                animator.SetTrigger(DeathHash);
                break;

            case EnemyState.Hurt:
            case EnemyState.Idle:
                // No hacemos nada especial (Hurt usa el flash rojo)
                animator.SetBool(IsMovingHash, false);
                break;
        }
    }
}