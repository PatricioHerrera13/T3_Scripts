using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected EnemyCore enemyCore;

    // Hashes protegidos para que BossAnimator los pueda usar
    protected static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    protected static readonly int AttackHash   = Animator.StringToHash("Attack");
    protected static readonly int DeathHash    = Animator.StringToHash("Death");

    protected virtual void Awake()
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

    protected virtual void OnEnable()
    {
        if (enemyCore != null)
            enemyCore.OnStateChanged += HandleStateChanged;
    }

    protected virtual void OnDisable()
    {
        if (enemyCore != null)
            enemyCore.OnStateChanged -= HandleStateChanged;
    }

    protected virtual void HandleStateChanged(EnemyState newState)
    {
        if (animator == null) return;

        switch (newState)
        {
            case EnemyState.Patrol:
            case EnemyState.Chase:
                animator.SetBool(IsMovingHash, true);
                break;

            case EnemyState.Attack:
                // Solo detenemos el movimiento. El trigger lo disparan los Attack Behaviors.
                animator.SetBool(IsMovingHash, false);
                break;

            case EnemyState.Death:
                animator.SetTrigger(DeathHash);
                break;

            case EnemyState.Hurt:
            case EnemyState.Idle:
                animator.SetBool(IsMovingHash, false);
                break;
        }
    }

    /// <summary>
    /// Llamar desde los Attack Behaviors SOLO cuando realmente empiezan a atacar.
    /// </summary>
    public virtual void TriggerAttackAnimation()
    {
        if (animator != null)
            animator.SetTrigger(AttackHash);
    }
}