using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BossCore : EnemyCore
{
    [Header("Boss Phases")]
    [Tooltip("Ordenar de mayor a menor healthThreshold (1.0 = 100% → 0.0)")]
    [SerializeField] private List<BossPhase> phases = new List<BossPhase>();

    [Header("Debug")]
    [SerializeField] private bool showPhaseDebug = true;

    // Evento que se dispara cuando cambia de fase
    public event Action<int, BossPhase> OnPhaseChanged; // (nuevoIndex, fase)

    public int CurrentPhaseIndex { get; private set; } = -1;

    public BossPhase CurrentPhase =>
        (CurrentPhaseIndex >= 0 && CurrentPhaseIndex < phases.Count)
            ? phases[CurrentPhaseIndex]
            : null;

    // ========== Sistema de transición ==========
    private bool isTransitioning = false;
    public bool IsTransitioning => isTransitioning;

    private Coroutine transitionCoroutine;
    private EnemyHealth bossHealth;

    protected override void Awake()
    {
        base.Awake();

        bossHealth = GetComponent<EnemyHealth>();
        if (bossHealth == null)
            bossHealth = GetComponentInChildren<EnemyHealth>();

        // Ordenamos de mayor a menor threshold por seguridad
        if (phases != null && phases.Count > 1)
            phases.Sort((a, b) => b.healthThreshold.CompareTo(a.healthThreshold));
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (bossHealth != null)
            bossHealth.OnHealthChanged += HandleHealthChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (bossHealth != null)
            bossHealth.OnHealthChanged -= HandleHealthChanged;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
            isTransitioning = false;
        }
    }

    private void Start()
    {
        if (phases != null && phases.Count > 0)
            ForcePhase(0);
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (CurrentState == EnemyState.Death) return;
        if (phases == null || phases.Count == 0) return;

        float healthPercent = (float)current / max;
        CheckPhaseTransition(healthPercent);
    }

    private void CheckPhaseTransition(float healthPercent)
    {
        int newPhaseIndex = 0;

        for (int i = 0; i < phases.Count; i++)
        {
            if (healthPercent <= phases[i].healthThreshold)
                newPhaseIndex = i;
        }

        if (newPhaseIndex != CurrentPhaseIndex)
            ChangePhase(newPhaseIndex);
    }

    private void ChangePhase(int newIndex)
    {
        if (newIndex < 0 || newIndex >= phases.Count) return;
        if (newIndex == CurrentPhaseIndex) return;

        CurrentPhaseIndex = newIndex;
        BossPhase newPhase = phases[newIndex];

        if (showPhaseDebug)
        {
            Debug.Log($"<color=magenta>{gameObject.name}</color> → Fase " +
                      $"<color=yellow>{newPhase.phaseName}</color> " +
                      $"(Index {newIndex} | {newPhase.healthThreshold:P0})");
        }

        StartPhaseTransition(newPhase);
        OnPhaseChanged?.Invoke(newIndex, newPhase);
    }

    // ==================== SISTEMA DE TRANSICIÓN ====================

    private void StartPhaseTransition(BossPhase phase)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        float duration = phase.transitionDuration;
        if (duration <= 0f)
            duration = 1.4f;

        transitionCoroutine = StartCoroutine(PhaseTransitionRoutine(duration));
    }

    private IEnumerator PhaseTransitionRoutine(float duration)
    {
        isTransitioning = true;

        if (CurrentState == EnemyState.Attack)
        {
            FinishAttack();
        }

        ChangeState(EnemyState.Idle);

        if (showPhaseDebug)
            Debug.Log($"<color=magenta>{gameObject.name}</color> → Iniciando transición ({duration:F2}s). No puede atacar.");

        yield return new WaitForSeconds(duration);

        isTransitioning = false;
        transitionCoroutine = null;

        if (showPhaseDebug)
            Debug.Log($"<color=magenta>{gameObject.name}</color> → Transición terminada. Ya puede moverse y atacar.");
    }

    // ==================== BLOQUEO DE ATAQUE (ahora con override real) ====================

    public override void ChangeState(EnemyState newState)
    {
        // Mientras está en transición, no dejamos entrar a Attack
        if (isTransitioning && newState == EnemyState.Attack)
        {
            return;
        }

        base.ChangeState(newState);
    }

    // ==================== API PÚBLICA ====================

    public void ForcePhase(int index)
    {
        if (index < 0 || index >= phases.Count)
        {
            Debug.LogWarning($"{name}: ForcePhase recibió un index inválido ({index})");
            return;
        }

        ChangePhase(index);
    }

    public int GetPhaseCount() => phases != null ? phases.Count : 0;
}