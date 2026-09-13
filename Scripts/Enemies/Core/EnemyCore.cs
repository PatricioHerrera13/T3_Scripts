using UnityEngine;
using System;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Hurt,
    Death
}

public class EnemyCore : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth health;
    [SerializeField] private MovementZone movementZone;
    [SerializeField] private DetectionZone detectionZone;
    [SerializeField] private AttackZone attackZone;

    [Header("Settings")]
    [SerializeField] private float hurtDuration = 0.35f;
    [SerializeField] private float attackDuration = 0.8f;
    [SerializeField] private bool startFacingRight = true;

    [Header("Debug")]
    [SerializeField] private bool showStateDebug = true;

    // Estado actual
    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    // Facing centralizado
    public bool IsFacingRight { get; private set; } = true;

    // Eventos
    public event Action<EnemyState> OnStateChanged;
    public event Action OnAttackRequested;
    public event Action OnDeath;

    private float stateTimer = 0f;

    private void Awake()
    {
        if (health == null) health = GetComponent<EnemyHealth>();
        if (movementZone == null) movementZone = GetComponentInChildren<MovementZone>();
        if (detectionZone == null) detectionZone = GetComponentInChildren<DetectionZone>();
        if (attackZone == null) attackZone = GetComponentInChildren<AttackZone>();

        // Facing inicial
        SetFacing(startFacingRight);
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        if (CurrentState == EnemyState.Death) return;

        stateTimer += Time.deltaTime;
        HandleStateLogic();
    }

    private void HandleStateLogic()
    {
        switch (CurrentState)
        {
            case EnemyState.Idle:
            case EnemyState.Patrol:
                if (IsPlayerDetected())
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                if (IsPlayerInAttackRange())
                {
                    ChangeState(EnemyState.Attack);
                }
                else if (!IsPlayerDetected())
                {
                    ChangeState(EnemyState.Patrol);
                }
                break;

            case EnemyState.Attack:
                if (stateTimer >= attackDuration)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Hurt:
                if (stateTimer >= hurtDuration)
                {
                    if (IsPlayerDetected())
                        ChangeState(EnemyState.Chase);
                    else
                        ChangeState(EnemyState.Patrol);
                }
                break;
        }
    }

    public void ChangeState(EnemyState newState)
    {
        if (CurrentState == newState) return;
        if (CurrentState == EnemyState.Death) return;

        if (showStateDebug)
        {
            Debug.Log($"<color=yellow>{gameObject.name}</color> : {CurrentState} → <color=cyan>{newState}</color>");
        }

        CurrentState = newState;
        stateTimer = 0f;

        OnStateChanged?.Invoke(newState);

        if (newState == EnemyState.Attack)
        {
            OnAttackRequested?.Invoke();
        }
    }

    public void ForceHurtState()
    {
        if (CurrentState == EnemyState.Death) return;
        ChangeState(EnemyState.Hurt);
    }

    public void FinishAttack()
    {
        if (CurrentState == EnemyState.Attack)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    private void HandleDeath()
    {
        ChangeState(EnemyState.Death);
        OnDeath?.Invoke();
    }

    // ==================== FACING CENTRALIZADO ====================

    public void SetFacing(bool faceRight)
    {
        if (IsFacingRight == faceRight) return;

        IsFacingRight = faceRight;

        Vector3 scale = transform.localScale;
        scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public void FaceTowards(Vector3 targetPosition)
    {
        float directionX = targetPosition.x - transform.position.x;
        if (Mathf.Abs(directionX) > 0.05f)
        {
            SetFacing(directionX > 0f);
        }
    }

    public void FaceDirection(float directionX)
    {
        if (Mathf.Abs(directionX) > 0.05f)
        {
            SetFacing(directionX > 0f);
        }
    }

    // ==================== API pública para Behaviors ====================

    public bool IsPlayerDetected()
    {
        return detectionZone != null && detectionZone.IsPlayerInRange();
    }

    public bool IsPlayerInAttackRange()
    {
        return attackZone != null && attackZone.IsPlayerInAttackRange();
    }

    public Vector3 GetClampedPosition(Vector3 desiredPosition)
    {
        if (movementZone != null)
            return movementZone.ClampPosition(desiredPosition);

        return desiredPosition;
    }

    public Vector3 GetPlayerPosition()
    {
        return detectionZone != null ? detectionZone.GetPlayerPosition() : transform.position;
    }

    public Vector3 GetMovementZoneCenter()
    {
        return movementZone != null ? movementZone.GetCenter() : transform.position;
    }

    public bool IsInsideMovementZone(Vector3 position)
    {
        return movementZone == null || movementZone.IsInside(position);
    }
}