using UnityEngine;
using System;
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

    private void OnEnable()
    {
        if (bossHealth != null)
            bossHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (bossHealth != null)
            bossHealth.OnHealthChanged -= HandleHealthChanged;
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

        OnPhaseChanged?.Invoke(newIndex, newPhase);
    }

    /// <summary>
    /// Fuerza el cambio a una fase específica (útil para testing y secuencias especiales).
    /// </summary>
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