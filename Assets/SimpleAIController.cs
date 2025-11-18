using UnityEngine;
using UnityEngine.AI;

public class SimpleAIController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveRadius = 20f;
    public float minWaitTime = 0f;        // ← 0으로 변경
    public float maxWaitTime = 0.3f;      // ← 0.3으로 변경
    public float moveSpeed = 2f;
    public float stuckCheckTime = 3f;     // ← 추가: 멈춤 감지 시간

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

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private bool isChasing = false;
    private float loseTimer = 0f;
    private float waitTimer = 0f;
    
    // ← 추가: 멈춤 감지
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
        }

        lastPosition = transform.position;  // ← 추가
        MoveToRandomPoint();
    }

    void Update()
    {
        bool seePlayer = CanSeePlayer();

        if (seePlayer)
        {
            if (!isChasing)
            {
                isChasing = true;
                agent.speed = chaseSpeed;
                Debug.Log("플레이어 발견!");
            }
            
            agent.SetDestination(player.position);
            loseTimer = 0f;
            stuckTimer = 0f;  // ← 추가: 타이머 리셋
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= catchDistance)
            {
                CatchPlayer();
            }
        }
        else if (isChasing)
        {
            loseTimer += Time.deltaTime;
            
            if (loseTimer >= losePlayerTime)
            {
                isChasing = false;
                agent.speed = moveSpeed;
                MoveToRandomPoint();
                Debug.Log("추적 종료. 순찰 복귀.");
            }
        }
        else
        {
            // 목적지 도착 확인
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                waitTimer -= Time.deltaTime;
                
                if (waitTimer <= 0f)
                {
                    MoveToRandomPoint();
                }
            }
            
            // ← 추가: 멈춤 감지 (순찰 중에만)
            CheckIfStuck();
        }

        UpdateAnimation();
        lastPosition = transform.position;  // ← 추가: 위치 업데이트
    }

    // ← 추가: 멈춤 감지 함수
    void CheckIfStuck()
    {
        float moveDistance = Vector3.Distance(transform.position, lastPosition);
        
        // 거의 움직이지 않음
        if (moveDistance < 0.01f && agent.hasPath)
        {
            stuckTimer += Time.deltaTime;
            
            // 일정 시간 이상 멈춰있으면 새 목적지
            if (stuckTimer >= stuckCheckTime)
            {
                Debug.Log("NPC가 멈춰있음. 새 목적지로 이동.");
                agent.ResetPath();  // 경로 리셋
                MoveToRandomPoint();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;  // 움직이고 있으면 타이머 리셋
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

    void CatchPlayer()
    {
        Debug.Log("플레이어를 잡았습니다!");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerCaught();
        }
        
        agent.isStopped = true;
        this.enabled = false;
    }

    void MoveToRandomPoint()
    {
        // ← 수정: 더 간단하고 안정적인 랜덤 이동
        for (int i = 0; i < 10; i++)  // 최대 10번 시도
        {
            Vector2 randomCircle = Random.insideUnitCircle * moveRadius;
            Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
            {
                // 경로 계산 가능한지 확인
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(hit.position);
                    waitTimer = Random.Range(minWaitTime, maxWaitTime);
                    stuckTimer = 0f;  // ← 추가: 타이머 리셋
                    return;
                }
            }
        }
        
        // 10번 실패하면 잠시 후 재시도
        Debug.Log("경로를 찾지 못함. 0.5초 후 재시도.");
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
}