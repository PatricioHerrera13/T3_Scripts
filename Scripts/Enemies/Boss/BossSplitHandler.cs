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

    [Tooltip("Offset de posición de los dos minions respecto al Boss")]
    [SerializeField] private Vector2 spawnOffset = new Vector2(1.5f, 0f);

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
        // Solo nos interesa la fase del split y que no se haya hecho todavía
        if (hasSplit) return;
        if (phaseIndex != splitPhaseIndex) return;

        if (showDebug)
            Debug.Log($"<color=magenta>[BossSplitHandler]</color> Detectado cambio a Fase {phaseIndex}. Esperando fin de transición...");

        // Esperamos a que termine la transición (usa el transitionDuration de la fase)
        float waitTime = newPhase.transitionDuration;
        if (waitTime <= 0f) waitTime = 2f; // fallback de seguridad

        StartCoroutine(WaitAndSplit(waitTime));
    }

    private IEnumerator WaitAndSplit(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (hasSplit) yield break; // por si acaso

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

        // 1. Calculamos la vida de cada minion
        int healthPerMinion = fixedMinionHealth;

        if (divideRemainingHealth && bossHealth != null)
        {
            int remaining = bossHealth.GetCurrentHealth();
            healthPerMinion = Mathf.Max(1, remaining / 2);

            if (showDebug)
                Debug.Log($"<color=magenta>[BossSplitHandler]</color> Vida restante del Boss: {remaining} → {healthPerMinion} por minion");
        }

        // 2. Spawneamos los dos minions
        Vector3 bossPos = transform.position;

        // Minion izquierda
        GameObject minionLeft = SpawnMinion(bossPos + (Vector3)(-spawnOffset), healthPerMinion, "BossMinion_L");
        // Minion derecha
        GameObject minionRight = SpawnMinion(bossPos + (Vector3)spawnOffset, healthPerMinion, "BossMinion_R");

        activeMinions.Add(minionLeft);
        activeMinions.Add(minionRight);
        aliveMinions = 2;

        // 3. Desactivamos el Boss original y su LevelComplete
        DisableOriginalBoss();

        if (showDebug)
            Debug.Log($"<color=magenta>[BossSplitHandler]</color> Split completado. 2 minions activos.");
    }

    private GameObject SpawnMinion(Vector3 position, int health, string name)
    {
        GameObject minion = Instantiate(minionPrefab, position, Quaternion.identity);
        minion.name = name;

        // Seteamos la vida
        EnemyHealth minionHealth = minion.GetComponent<EnemyHealth>();
        if (minionHealth != null)
        {
            // Como EnemyHealth no tiene un SetMaxHealth público todavía,
            // usamos una forma segura (la mejoramos después si hace falta)
            // Por ahora asumimos que el prefab ya tiene la vida configurada
            // o que vamos a agregar un método SetHealth más adelante.
        }

        // Nos suscribimos a la muerte del minion
        if (minionHealth != null)
        {
            minionHealth.OnDeath += OnMinionDeath;
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
            // Victoria
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
        // Desactivamos el LevelComplete del Boss original para que no dispare antes
        if (levelCompleteOnDeath != null)
            levelCompleteOnDeath.enabled = false;

        // Desactivamos el GameObject del Boss (o solo los componentes si preferís)
        // Opción A (recomendada): desactivar todo el objeto
        gameObject.SetActive(false);

        // Si preferís solo desactivar comportamientos y dejar el sprite visible un momento,
        // se puede hacer más elaborado después.
    }
}