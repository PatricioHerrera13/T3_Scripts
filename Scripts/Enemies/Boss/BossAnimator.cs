using UnityEngine;

public class BossAnimator : EnemyAnimator
{
    [Header("Boss Specific")]
    [SerializeField] private BossCore bossCore;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    // Parámetros extra del Boss
    private static readonly int PhaseChangeHash = Animator.StringToHash("PhaseChange");
    private static readonly int PhaseHash       = Animator.StringToHash("Phase");

    // Para evitar la animación de PhaseChange en la fase inicial
    private bool hasInitializedPhase = false;

    protected override void Awake()
    {
        base.Awake();

        if (bossCore == null)
            bossCore = GetComponent<BossCore>();
        if (bossCore == null)
            bossCore = GetComponentInParent<BossCore>();

        // Por si el EnemyCore del base no encontró el BossCore
        if (enemyCore == null && bossCore != null)
            enemyCore = bossCore;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (bossCore != null)
            bossCore.OnPhaseChanged += HandlePhaseChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (bossCore != null)
            bossCore.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(int phaseIndex, BossPhase newPhase)
    {
        if (animator == null) return;

        // Siempre actualizamos el parámetro Phase
        animator.SetInteger(PhaseHash, phaseIndex);

        // Solo reproducimos la animación de cambio si NO es la fase inicial
        if (hasInitializedPhase)
        {
            animator.SetTrigger(PhaseChangeHash);
            animator.SetBool(IsMovingHash, false);

            if (showDebug)
                Debug.Log($"<color=magenta>[BossAnimator]</color> PhaseChange → Fase {phaseIndex} ({newPhase?.phaseName})");
        }
        else
        {
            // Primera vez: solo seteamos el estado sin animación de transición
            hasInitializedPhase = true;

            if (showDebug)
                Debug.Log($"<color=magenta>[BossAnimator]</color> Fase inicial seteada: {phaseIndex} (sin animación)");
        }
    }
}