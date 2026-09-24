using UnityEngine;
using System;
using System.Collections.Generic;

public class BossCore : EnemyCore
{
    [Header("Boss Phases")]
    [Tooltip("Ordenar de mayor a menor healthThreshold (100% → 0%)")]
    [SerializeField] private List<BossPhase> phases = new List<BossPhase>();

    [Header("Debug")]
    [SerializeField] private bool showPhaseDebug = true;

    // Evento que se dispara cuando cambia de fase
    public event Action<int, BossPhase> OnPhaseChanged;   // (nuevoIndex, fase)

    // Estado actual
    public int CurrentPhaseIndex { get; private set; } = -1;
    public BossPhase CurrentPhase => (CurrentPhaseIndex >= 0 && CurrentPhaseIndex < phases.Count)
        ? phases[CurrentPhaseIndex]
        : null;

    private EnemyHealth bossHealth;

    protected override void Awake()
    {
        base.Awake();   // Importante: llama al Awake de EnemyCore

        bossHealth = GetComponent<EnemyHealth>();
        if (bossHealth == null)
            bossHealth = GetComponentInChildren<EnemyHealth>();

        // Ordenamos las fases de mayor a menor threshold por seguridad
        phases.Sort((a, b) => b.healthThreshold.CompareTo(a.healthThreshold));
    }

    private void Start()
    {
        // Inicializamos en la primera fase (la de mayor vida)
        if (phases.Count > 0)
        {
            ForcePhase(0);
        }
    }

    private void Update()
    {
        // EnemyCore ya tiene su propio Update. 
        // Aquí solo revisamos el cambio de fase por vida.
        CheckPhaseTransition();
    }

    private void CheckPhaseTransition()
    {
        if (bossHealth == null || phases.Count == 0) return;
        if (CurrentState == EnemyState.Death) return;

        float healthPercent = (float)bossHealth.GetCurrentHealth() / bossHealth.GetMaxHealth();

        // Buscamos la fase más baja que todavía cumpla el threshold
        int newPhaseIndex = 0;
        for (int i = 0; i < phases.Count; i++)
        {
            if (healthPercent <= phases[i].healthThreshold)
            {
                newPhaseIndex = i;
            }
        }

        if (newPhaseIndex != CurrentPhaseIndex)
        {
            ChangePhase(newPhaseIndex);
        }
    }

    private void ChangePhase(int newIndex)
    {
        if (newIndex < 0 || newIndex >= phases.Count) return;
        if (newIndex == CurrentPhaseIndex) return;

        CurrentPhaseIndex = newIndex;
        BossPhase newPhase = phases[newIndex];

        if (showPhaseDebug)
        {
            Debug.Log($"<color=magenta>{gameObject.name}</color> cambió a fase " +
                      $"<color=yellow>{newPhase.phaseName}</color> " +
                      $"(Index {newIndex} | Threshold {newPhase.healthThreshold:P0})");
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

    /// <summary>
    /// Devuelve la cantidad total de fases configuradas.
    /// </summary>
    public int GetPhaseCount() => phases.Count;
}