using UnityEngine;

/// <summary>
/// Controla el Animator del Boss.
/// Escucha OnStateChanged y OnPhaseChanged de BossCore.
/// </summary>
[RequireComponent(typeof(Animator))]
public class BossAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossCore bossCore;
    [SerializeField] private Animator animator;

    [Header("Parameter Names (deben coincidir con el Animator Controller)")]
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string attackParam = "Attack";
    [SerializeField] private string phaseChangeParam = "PhaseChange";
    [SerializeField] private string deathParam = "Death";
    [SerializeField] private string phaseParam = "Phase";          // Int opcional (0 = Fase 1, 1 = Fase 2...)

    [Header("Settings")]
    [Tooltip("Si true, también setea el parámetro Phase (Int) al cambiar de fase")]
    [SerializeField] private bool usePhaseInt = true;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (bossCore == null)
            bossCore = GetComponent<BossCore>();

        if (bossCore == null)
            bossCore = GetComponentInParent<BossCore>();
    }

    private void OnEnable()
    {
        if (bossCore != null)
        {
            bossCore.OnStateChanged += HandleStateChanged;
            bossCore.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDisable()
    {
        if (bossCore != null)
        {
            bossCore.OnStateChanged -= HandleStateChanged;
            bossCore.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void Start()
    {
        // Sincronizamos fase inicial
        if (bossCore != null && usePhaseInt)
        {
            animator.SetInteger(phaseParam, bossCore.CurrentPhaseIndex);
        }
    }

    private void HandleStateChanged(EnemyState newState)
    {
        switch (newState)
        {
            case EnemyState.Idle:
            case EnemyState.Patrol:
            case EnemyState.Chase:
                animator.SetBool(isMovingParam, true);
                break;

            case EnemyState.Attack:
                animator.SetBool(isMovingParam, false);
                animator.SetTrigger(attackParam);
                break;

            case EnemyState.Hurt:
                // Por ahora solo flash de color (igual que enemigos normales)
                animator.SetBool(isMovingParam, false);
                break;

            case EnemyState.Death:
                animator.SetBool(isMovingParam, false);
                animator.SetTrigger(deathParam);
                break;
        }
    }

    private void HandlePhaseChanged(int phaseIndex, BossPhase phase)
    {
        // Disparamos la animación de cambio de fase
        animator.SetTrigger(phaseChangeParam);

        if (usePhaseInt)
        {
            animator.SetInteger(phaseParam, phaseIndex);
        }
    }

    // ============================================================
    // Métodos públicos por si querés forzar animaciones desde fuera
    // (útil para testing o Animation Events)
    // ============================================================

    public void TriggerAttack()
    {
        animator.SetTrigger(attackParam);
    }

    public void TriggerPhaseChange()
    {
        animator.SetTrigger(phaseChangeParam);
    }

    public void TriggerDeath()
    {
        animator.SetTrigger(deathParam);
    }

    public void SetMoving(bool moving)
    {
        animator.SetBool(isMovingParam, moving);
    }
}