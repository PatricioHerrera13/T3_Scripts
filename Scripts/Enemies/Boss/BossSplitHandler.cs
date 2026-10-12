using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossSplitHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossCore bossCore;
    [SerializeField] private EnemyHealth bossHealth;
    [SerializeField] private BossLevelCompleteOnDeath levelCompleteOnDeath;

    [Header("Split Settings")]
    [Tooltip("Índice de la fase en la que se hace el split (normalmente 1 = Phase 2)")]
    [SerializeField] private int splitPhaseIndex = 1;

    [Tooltip("Prefab del minion que se va a spawnear (tiene que tener EnemyCore + EnemyHealth)")]
    [SerializeField] private GameObject minionPrefab;

    [Tooltip("Offset horizontal de los dos minions respecto al punto de spawn")]
    [SerializeField] private float horizontalOffset = 1.5f;

    [Tooltip("Offset vertical extra (positivo = más arriba). Usalo si aparecen bajo el piso.")]
    [SerializeField] private float verticalOffset = 0.5f;

    [Tooltip("Si está asignado, usa esta Transform como origen del spawn en vez de this.transform")]
    [SerializeField] private Transform spawnOrigin;

    [Header("Minion Health")]
    [Tooltip("Si está activo, divide la vida restante del Boss entre los dos minions")]
    [SerializeField] private bool divideRemainingHealth = true;

    [Tooltip("Vida fija por minion (solo se usa si divideRemainingHealth = false)")]
    [SerializeField] private int fixedMinionHealth = 3;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    // Internals
    private bool hasSplit = false;
    private List<GameObject> activeMinions = new List<GameObject>();
    private int aliveMinions = 0;

    private void Awake()
    {
        if (bossCore == null) bossCore = GetComponent<BossCore>();
        if (bossHealth == null) bossHealth = GetComponent<EnemyHealth>();
        if (levelCompleteOnDeath == null) levelCompleteOnDeath = GetComponent<BossLevelCompleteOnDeath>();
    }

    private void OnEnable()
    {
        if (bossCore != null)
            bossCore.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        if (bossCore != null)
            bossCore.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(int phaseIndex, BossPhase newPhase)
    {
        if (hasSplit) return;
        if (phaseIndex != splitPhaseIndex) return;

        if (showDebug)
            Debug.Log($"<color=magenta>[BossSplitHandler]</color> Detectado cambio a Fase {phaseIndex}. Esperando fin de transición...");

        float waitTime = newPhase.transitionDuration;
        if (waitTime <= 0f) waitTime = 2f;

        StartCoroutine(WaitAndSplit(waitTime));
    }

    private IEnumerator WaitAndSplit(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (hasSplit) yield break;

        PerformSplit();
    }

    private void PerformSplit()
    {
        hasSplit = true;

        if (minionPrefab == null)
        {
            Debug.LogError("[BossSplitHandler] No hay Minion Prefab asignado!", this);
            return;
        }

        // 1. Vida por minion
        int healthPerMinion = fixedMinionHealth;

        if (divideRemainingHealth && bossHealth != null)
        {
            int remaining = bossHealth.GetCurrentHealth();
            healthPerMinion = Mathf.Max(1, remaining / 2);

            if (showDebug)
                Debug.Log($"<color=magenta>[BossSplitHandler]</color> Vida restante del Boss: {remaining} → {healthPerMinion} por minion");
        }

        // 2. Punto de origen del spawn
        Vector3 origin = spawnOrigin != null ? spawnOrigin.position : transform.position;
        origin.y += verticalOffset; // ← corrige el “bajo el piso”

        Vector3 leftPos  = origin + new Vector3(-horizontalOffset, 0f, 0f);
        Vector3 rightPos = origin + new Vector3( horizontalOffset, 0f, 0f);

        GameObject minionLeft  = SpawnMinion(leftPos,  healthPerMinion, "BossMinion_L");
        GameObject minionRight = SpawnMinion(rightPos, healthPerMinion, "BossMinion_R");

        activeMinions.Add(minionLeft);
        activeMinions.Add(minionRight);
        aliveMinions = 2;

        // 3. Desactivar Boss original
        DisableOriginalBoss();

        if (showDebug)
            Debug.Log($"<color=magenta>[BossSplitHandler]</color> Split completado. 2 minions activos en Y={origin.y:F2}");
    }

    private GameObject SpawnMinion(Vector3 position, int health, string name)
    {
        GameObject minion = Instantiate(minionPrefab, position, Quaternion.identity);
        minion.name = name;

        EnemyHealth minionHealth = minion.GetComponent<EnemyHealth>();
        if (minionHealth != null)
        {
            minionHealth.SetMaxHealth(health, true);
            minionHealth.OnDeath += OnMinionDeath;
        }
        else
        {
            Debug.LogWarning($"[BossSplitHandler] El minion {name} no tiene EnemyHealth!", minion);
        }

        return minion;
    }

    private void OnMinionDeath()
    {
        aliveMinions--;

        if (showDebug)
            Debug.Log($"<color=magenta>[BossSplitHandler]</color> Minion muerto. Quedan vivos: {aliveMinions}");

        if (aliveMinions <= 0)
        {
            if (showDebug)
                Debug.Log($"<color=magenta>[BossSplitHandler]</color> Ambos minions derrotados → Level Complete");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteCurrentLevel();
            }
        }
    }

    private void DisableOriginalBoss()
    {
        if (levelCompleteOnDeath != null)
            levelCompleteOnDeath.enabled = false;

        gameObject.SetActive(false);
    }
}