using UnityEngine;

public class DroneAIController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float flyHeight = 3f;
    public float patrolRadius = 20f;
    public Vector3 centerPosition;
    public float obstacleAvoidDistance = 3f;  // ← 추가
    public LayerMask obstacleLayer;  // ← 추가

    [Header("Boundaries")]
    public float maxX = 25f;
    public float minX = -25f;
    public float maxZ = 25f;
    public float minZ = -25f;

    [Header("Vision Settings")]
    public float viewAngle = 180f;
    public float viewDistance = 30f;

    [Header("Chase Settings")]
    public float chaseSpeed = 6f;
    public float losePlayerTime = 3f;

    [Header("Visual Effects")]
    public Transform[] propellers;
    public float propellerIdleSpeed = 1000f;
    public float propellerChaseSpeed = 2000f;
    public Light droneLight;
    public float lightIdleIntensity = 2f;  // ← 2로 증가
    public float lightChaseIntensity = 5f;  // ← 5로 증가
    public Color lightIdleColor = Color.white;
    public Color lightChaseColor = Color.red;

    private Transform player;
    private bool isChasing = false;
    private float loseTimer = 0f;
    private Vector3 patrolTarget;
    private float currentPropellerSpeed;
    private float stuckTimer = 0f;  // ← 추가
    private Vector3 lastPosition;  // ← 추가

    void Start()
    {
        Debug.Log("??? 드론 AI 시작! ???");
        
        centerPosition = transform.position;
        lastPosition = transform.position;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log($"? 플레이어 찾음: {player.name}");
        }
        else
        {
            Debug.LogError("? 플레이어를 찾을 수 없습니다! Tag 'Player' 확인!");
        }

        if (droneLight == null)
        {
            droneLight = GetComponentInChildren<Light>();
        }

        if (droneLight != null)
        {
            droneLight.enabled = true;
            droneLight.intensity = lightIdleIntensity;
            droneLight.color = lightIdleColor;
            droneLight.range = 40f;  // ← 추가: 범위 증가
            Debug.Log($"? 조명 초기화! Intensity: {droneLight.intensity}, Range: {droneLight.range}");
        }
        else
        {
            Debug.LogWarning("?? 조명이 없습니다!");
        }

        Debug.Log($"프로펠러 개수: {propellers.Length}");
        if (propellers.Length == 0)
        {
            Debug.LogWarning("?????? 프로펠러가 할당되지 않았습니다! Inspector에서 할당하세요!");
        }

        currentPropellerSpeed = propellerIdleSpeed;
        SetNewPatrolTarget();
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogWarning("플레이어가 null!");
            return;
        }

        bool seePlayer = CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ← 추가: 멈춤 감지
        CheckIfStuck();

        if (seePlayer)
        {
            if (!isChasing)
            {
                isChasing = true;
                Debug.Log("??? 플레이어 발견! 추격 시작! ???");
            }

            Vector3 targetPos = player.position + Vector3.up * flyHeight;
            targetPos = ClampToBoundaries(targetPos);
            
            // ← 수정: 장애물 회피하며 이동
            MoveWithObstacleAvoidance(targetPos, chaseSpeed);

            loseTimer = 0f;
        }
        else if (isChasing)
        {
            loseTimer += Time.deltaTime;

            if (loseTimer >= losePlayerTime)
            {
                isChasing = false;
                SetNewPatrolTarget();
                Debug.Log("드론: 순찰 복귀");
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, patrolTarget) < 2f)
            {
                SetNewPatrolTarget();
            }
            // ← 수정: 장애물 회피하며 이동
            MoveWithObstacleAvoidance(patrolTarget, moveSpeed);
        }

        KeepInBounds();
        RotateDrone();
        UpdateVisualEffects();
        
        lastPosition = transform.position;
    }

    // ← 추가: 장애물 회피 이동
    void MoveWithObstacleAvoidance(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        
        // 전방 장애물 체크
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, obstacleAvoidDistance))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                Debug.Log($"? 장애물 감지: {hit.collider.name}");
                
                // 장애물 회피 방향 계산
                Vector3 avoidDir = Vector3.Cross(direction, Vector3.up);
                
                // 랜덤으로 좌우 선택
                if (Random.value > 0.5f)
                    avoidDir = -avoidDir;
                
                direction = (direction + avoidDir).normalized;
            }
        }
        
        transform.position += direction * speed * Time.deltaTime;
    }

    // ← 추가: 멈춤 감지
    void CheckIfStuck()
    {
        float moveDistance = Vector3.Distance(transform.position, lastPosition);
        
        if (moveDistance < 0.1f)
        {
            stuckTimer += Time.deltaTime;
            
            if (stuckTimer > 2f)
            {
                Debug.Log("? 드론이 멈춰있음! 새 목표 설정");
                SetNewPatrolTarget();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    void SetNewPatrolTarget()
    {
        // ← 수정: 더 안전한 위치 찾기
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            Vector3 testTarget = centerPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            testTarget.y = flyHeight;
            
            testTarget = ClampToBoundaries(testTarget);
            
            // 경로에 장애물이 없는지 체크
            Vector3 dirToTarget = testTarget - transform.position;
            if (!Physics.Raycast(transform.position, dirToTarget.normalized, dirToTarget.magnitude - 1f))
            {
                patrolTarget = testTarget;
                Debug.Log($"새 순찰 목표: {patrolTarget}");
                return;
            }
        }
        
        // 안전한 위치 못 찾으면 위쪽으로
        patrolTarget = transform.position + Vector3.up * 2f;
    }

    Vector3 ClampToBoundaries(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);
        position.y = flyHeight;
        return position;
    }

    void KeepInBounds()
    {
        Vector3 pos = transform.position;

        if (pos.x < minX || pos.x > maxX || pos.z < minZ || pos.z > maxZ)
        {
            pos = ClampToBoundaries(pos);
            transform.position = pos;
            
            if (isChasing)
            {
                isChasing = false;
                SetNewPatrolTarget();
                Debug.Log("드론: 경계 벗어남, 순찰 복귀");
            }
        }
    }

    void RotateDrone()
    {
        Vector3 direction = isChasing ? (player.position - transform.position) : (patrolTarget - transform.position);
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    void UpdateVisualEffects()
    {
        // 프로펠러
        float targetSpeed = isChasing ? propellerChaseSpeed : propellerIdleSpeed;
        currentPropellerSpeed = Mathf.Lerp(currentPropellerSpeed, targetSpeed, Time.deltaTime * 2f);

        foreach (Transform propeller in propellers)
        {
            if (propeller != null)
            {
                propeller.Rotate(Vector3.up, currentPropellerSpeed * Time.deltaTime);
            }
        }

        // 조명
        if (droneLight != null)
        {
            Color targetColor = isChasing ? lightChaseColor : lightIdleColor;
            float targetIntensity = isChasing ? lightChaseIntensity : lightIdleIntensity;

            droneLight.color = Color.Lerp(droneLight.color, targetColor, Time.deltaTime * 3f);
            droneLight.intensity = Mathf.Lerp(droneLight.intensity, targetIntensity, Time.deltaTime * 3f);

            if (isChasing)
            {
                float flicker = Mathf.Sin(Time.time * 10f) * 0.3f + 0.7f;
                droneLight.intensity = targetIntensity * flicker;
            }
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        
        if (distance > viewDistance) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        
        if (angle > viewAngle / 2f) return false;

        // ← 수정: 플레이어 중심(허리)을 향해 Raycast
        Vector3 playerCenter = player.position + Vector3.up * 1f;
        Vector3 dirToCenter = playerCenter - transform.position;
        
        if (Physics.Raycast(transform.position, dirToCenter.normalized, out RaycastHit hit, viewDistance))
        {
            Debug.Log($"? Raycast 충돌: {hit.collider.name} (Tag: {hit.collider.tag})");
            
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("??? 플레이어 감지 성공!");
                return true;
            }
        }

        return false;
    }

    // ← 추가: 충돌 시 방향 전환
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"? 충돌: {collision.gameObject.name}");
        
        if (!isChasing)
        {
            SetNewPatrolTarget();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPosition, patrolRadius);

        Gizmos.color = Color.red;
        Vector3 bottomLeft = new Vector3(minX, flyHeight, minZ);
        Vector3 bottomRight = new Vector3(maxX, flyHeight, minZ);
        Vector3 topLeft = new Vector3(minX, flyHeight, maxZ);
        Vector3 topRight = new Vector3(maxX, flyHeight, maxZ);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);

        Gizmos.color = isChasing ? Color.red : Color.cyan;
        Vector3 forward = transform.forward * viewDistance;
        Gizmos.DrawRay(transform.position, forward);
        
        // ← 추가: 장애물 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleAvoidDistance);
    }
}
