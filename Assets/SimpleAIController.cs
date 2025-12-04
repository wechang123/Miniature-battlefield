using UnityEngine;
using UnityEngine.AI;

public class SimpleAIController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveRadius = 20f;
    public float minWaitTime = 0f;
    public float maxWaitTime = 0.3f;
    public float moveSpeed = 2f;
    public float stuckCheckTime = 3f;

    [Header("Vision Settings")]
    public float viewAngle = 60f;
    public float viewDistance = 15f;
    public LayerMask playerLayer;

    [Header("Chase Settings")]
    public float chaseSpeed = 5f;
    public float losePlayerTime = 2f;
    public float catchDistance = 1.5f;

    [Header("Animation")]
    public bool useAnimator = true;
    public float walkAnimationSpeed = 2f;
    public float runAnimationSpeed = 5f;

    [Header("Attack")]
    public string attackTrigger = "Attack";
    public float attackDuration = 5f;

    [Header("Flashlight")]
    public Light flashlight; // 손전등 (Spot Light)
    public float patrolLightIntensity = 1.5f;
    public float chaseLightIntensity = 3f;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private bool isChasing = false;
    private bool isAttacking = false;
    private float loseTimer = 0f;
    private float waitTimer = 0f;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = moveSpeed;
        agent.acceleration = 20f;
        agent.angularSpeed = 720f;
        agent.stoppingDistance = 0.5f;
        agent.autoBraking = false;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;

            // 선생님과 플레이어 물리 충돌 무시
            Collider myCollider = GetComponent<Collider>();
            Collider playerCollider = playerObject.GetComponent<Collider>();
            if (myCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(myCollider, playerCollider, true);
            }
        }

        lastPosition = transform.position;
        MoveToRandomPoint();
    }

    void Update()
    {
        // 항상 거리 체크 (넉백 중에도 잡을 수 있도록)
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= catchDistance)
            {
                CatchPlayer();
                return;
            }
        }

        if (isAttacking) return;

        bool seePlayer = CanSeePlayer();

        if (seePlayer)
        {
            if (!isChasing)
            {
                isChasing = true;
                agent.speed = chaseSpeed;
                SetFlashlightChaseMode(true);
                Debug.Log("플레이어 발견!");
            }

            agent.SetDestination(player.position);
            loseTimer = 0f;
            stuckTimer = 0f;
        }
        else if (isChasing)
        {
            loseTimer += Time.deltaTime;

            if (loseTimer >= losePlayerTime)
            {
                isChasing = false;
                agent.speed = moveSpeed;
                SetFlashlightChaseMode(false);
                MoveToRandomPoint();
                Debug.Log("추적 종료. 순찰 재개.");
            }
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                waitTimer -= Time.deltaTime;

                if (waitTimer <= 0f)
                {
                    MoveToRandomPoint();
                }
            }

            CheckIfStuck();
        }

        UpdateAnimation();
        lastPosition = transform.position;
    }

    void CatchPlayer()
    {
        Debug.Log("플레이어를 잡았습니다!");

        isAttacking = true;
        agent.isStopped = true;
        agent.enabled = false; // NavMeshAgent 완전히 비활성화

        // 플레이어 방향 바라보기
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }

        // 공격 애니메이션 재생 (Root Motion 비활성화)
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetTrigger(attackTrigger);
        }

        // 애니메이션 재생 후 GameOver 호출 (attackDuration 후)
        Invoke(nameof(TriggerGameOver), attackDuration);

        // 공격 종료 후 비활성화
        Invoke(nameof(DisableAI), attackDuration + 0.1f);
    }

    void TriggerGameOver()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerCaught(transform);
        }
    }

    // AI 비활성화
    void DisableAI()
    {
        this.enabled = false;
    }

    void CheckIfStuck()
    {
        float moveDistance = Vector3.Distance(transform.position, lastPosition);

        if (moveDistance < 0.01f && agent.hasPath)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= stuckCheckTime)
            {
                Debug.Log("NPC가 막혔습니다. 새 위치로 이동.");
                agent.ResetPath();
                MoveToRandomPoint();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    void UpdateAnimation()
    {
        if (!useAnimator || animator == null) return;

        float currentSpeed = agent.velocity.magnitude;

        if (currentSpeed < 0.1f)
        {
            animator.SetFloat("MotionSpeed", 0f);
            animator.speed = 1f;
            return;
        }

        animator.SetFloat("MotionSpeed", 1f);

        if (isChasing)
        {
            float speedRatio = currentSpeed / runAnimationSpeed;
            animator.speed = speedRatio;
        }
        else
        {
            float speedRatio = currentSpeed / walkAnimationSpeed;
            animator.speed = speedRatio;
        }
    }

    void MoveToRandomPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * moveRadius;
            Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(hit.position);
                    waitTimer = Random.Range(minWaitTime, maxWaitTime);
                    stuckTimer = 0f;
                    return;
                }
            }
        }

        Invoke(nameof(MoveToRandomPoint), 0.5f);
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2f) return false;

        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out RaycastHit hit, viewDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, moveRadius);

        Gizmos.color = isChasing ? Color.red : Color.cyan;

        Vector3 forward = transform.forward * viewDistance;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * forward;

        Gizmos.DrawRay(transform.position, left);
        Gizmos.DrawRay(transform.position, right);
        Gizmos.DrawRay(transform.position, forward);

        Vector3 prev = transform.position + left;
        for (int i = 1; i <= 20; i++)
        {
            float currentAngle = -viewAngle / 2f + (viewAngle / 20f * i);
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 point = transform.position + dir;
            Gizmos.DrawLine(prev, point);
            prev = point;
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }

    /// <summary>
    /// 손전등 모드 설정
    /// </summary>
    void SetFlashlightChaseMode(bool chasing)
    {
        if (flashlight == null) return;

        if (chasing)
        {
            flashlight.intensity = chaseLightIntensity;
            flashlight.color = new Color(1f, 0.9f, 0.9f); // 약간 붉은색
        }
        else
        {
            flashlight.intensity = patrolLightIntensity;
            flashlight.color = new Color(1f, 0.95f, 0.8f); // 따뜻한 색
        }
    }
}
