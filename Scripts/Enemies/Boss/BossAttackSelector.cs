using UnityEngine;
using System.Collections.Generic;

public class BossAttackSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossCore bossCore;

    [Header("Settings")]
    [Tooltip("Si está activo, elige un ataque aleatorio de los disponibles en la fase")]
    [SerializeField] private bool randomSelection = true;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private List<MonoBehaviour> currentlyEnabledAttacks = new List<MonoBehaviour>();

    private void Awake()
    {
        if (bossCore == null)
            bossCore = GetComponent<BossCore>();
    }

    private void OnEnable()
    {
        if (bossCore != null)
        {
            bossCore.OnPhaseChanged += HandlePhaseChanged;
            bossCore.OnAttackRequested += HandleAttackRequested;
        }
    }

    private void OnDisable()
    {
        if (bossCore != null)
        {
            bossCore.OnPhaseChanged -= HandlePhaseChanged;
            bossCore.OnAttackRequested -= HandleAttackRequested;
        }
    }

    private void Start()
    {
        // Al empezar aplicamos la fase actual
        if (bossCore != null && bossCore.CurrentPhase != null)
        {
            ApplyPhaseAttacks(bossCore.CurrentPhase);
        }
    }

    private void HandlePhaseChanged(int phaseIndex, BossPhase newPhase)
    {
        ApplyPhaseAttacks(newPhase);
    }

    private void ApplyPhaseAttacks(BossPhase phase)
    {
        // 1. Desactivamos TODOS los ataques del boss
        DisableAllAttacks();

        currentlyEnabledAttacks.Clear();

        if (phase == null || phase.allowedAttacks == null) return;

        // 2. Activamos solo los de esta fase
        foreach (var attack in phase.allowedAttacks)
        {
            if (attack == null) continue;

            attack.enabled = true;
            currentlyEnabledAttacks.Add(attack);

            if (showDebug)
                Debug.Log($"<color=cyan>[BossAttackSelector]</color> Activado: {attack.GetType().Name}");
        }

        if (showDebug)
            Debug.Log($"<color=cyan>[BossAttackSelector]</color> Fase \"{phase.phaseName}\" → {currentlyEnabledAttacks.Count} ataques activos");
    }

    private void DisableAllAttacks()
    {
        // Buscamos todos los posibles ataques bajo el Boss
        // (ChargeAttack, ProjectileAttack, MeleeAttack, etc.)
        var allAttacks = GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var comp in allAttacks)
        {
            if (comp == null) continue;

            // Solo desactivamos componentes que sean ataques conocidos
            if (comp is ChargeAttack || comp is ProjectileAttack || comp is MeleeAttack)
            {
                comp.enabled = false;
            }
        }
    }

    private void HandleAttackRequested()
    {
        if (currentlyEnabledAttacks.Count == 0)
        {
            if (showDebug)
                Debug.LogWarning("[BossAttackSelector] No hay ataques activos en esta fase");
            bossCore.FinishAttack();
            return;
        }

        MonoBehaviour chosenAttack = null;

        if (randomSelection)
        {
            int index = Random.Range(0, currentlyEnabledAttacks.Count);
            chosenAttack = currentlyEnabledAttacks[index];
        }
        else
        {
            // Por ahora toma el primero
            chosenAttack = currentlyEnabledAttacks[0];
        }

        if (chosenAttack == null)
        {
            bossCore.FinishAttack();
            return;
        }

        // Disparamos el ataque elegido
        TriggerAttack(chosenAttack);
    }

    private void TriggerAttack(MonoBehaviour attack)
    {
        // Llamamos a un método público común que vamos a agregar a los ataques
        if (attack is ChargeAttack charge)
        {
            charge.TriggerAttack();
        }
        else if (attack is ProjectileAttack projectile)
        {
            projectile.TriggerAttack();
        }
        else if (attack is MeleeAttack melee)
        {
            melee.TriggerAttack();
        }
        else
        {
            Debug.LogWarning($"[BossAttackSelector] El ataque {attack.GetType().Name} no tiene método TriggerAttack()");
            bossCore.FinishAttack();
        }
    }
}