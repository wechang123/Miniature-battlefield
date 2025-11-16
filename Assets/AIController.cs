using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 areaCenter = Vector3.zero;
    public float areaRadius = 20f;
    public float minMoveDistance = 5f;
    public float maxMoveDistance = 15f;
    public float waitTimeMin = 0.5f;
    public float waitTimeMax = 2f;

    [Header("Vision Settings")]
    public float normalViewAngle = 120f;
    public float flashlightViewAngle = 60f;
    public float normalViewDistance = 8f;
    public float flashlightViewDistance = 15f;
    public LayerMask targetLayer;
    
    [Header("Item System")]
    public bool startWithFlashlight = false;  // 시작 시 손전등 소지

    private ItemHolder itemHolder;
    private NavMeshAgent agent;
    private Animator animator;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private float currentViewAngle;
    private float currentViewDistance;
    private bool hasFlashlight = false;
    private GameObject currentFlashlight;
    private Light flashlightLight;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        itemHolder = GetComponent<ItemHolder>();
        
        if (itemHolder == null)
        {
            Debug.LogError("ItemHolder 컴포넌트가 필요합니다!");
            return;
        }

        agent.acceleration = 15f;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = 0.2f;
        agent.autoBraking = true;

        // 시작 시 손전등 장착
        if (startWithFlashlight)
        {
            PickupFlashlight();
        }

        UpdateFlashlightStatus();
        MoveToRandomPosition();
    }

    void Update()
    {
        UpdateFlashlightStatus();
        DetectTargets();

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                MoveToRandomPosition();
            }
        }
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                isWaiting = true;
                waitTimer = Random.Range(waitTimeMin, waitTimeMax);
            }
        }

        if (animator != null && agent != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("MotionSpeed", speed < 0.1f ? 0f : 1f);
        }
    }

    void UpdateFlashlightStatus()
    {
        currentFlashlight = itemHolder != null ? itemHolder.GetCurrentItem() : null;
        hasFlashlight = currentFlashlight != null;
        
        if (hasFlashlight)
        {
            currentViewAngle = flashlightViewAngle;
            currentViewDistance = flashlightViewDistance;
            
            if (flashlightLight == null)
            {
                flashlightLight = currentFlashlight.GetComponentInChildren<Light>();
            }
            
            if (flashlightLight != null)
            {
                flashlightLight.enabled = true;
                flashlightLight.type = LightType.Spot;
                flashlightLight.spotAngle = flashlightViewAngle;
                flashlightLight.range = flashlightViewDistance;
                flashlightLight.intensity = 2f;
                flashlightLight.color = Color.white;
            }
        }
        else
        {
            currentViewAngle = normalViewAngle;
            currentViewDistance = normalViewDistance;
            flashlightLight = null;
        }
    }

    void DetectTargets()
    {
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, currentViewDistance, targetLayer);

        foreach (Collider target in targetsInRadius)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            
            if (angleToTarget < currentViewAngle / 2f)
            {
                Vector3 rayStart = transform.position + Vector3.up * 1.5f;
                
                if (Physics.Raycast(rayStart, directionToTarget, out RaycastHit hit, currentViewDistance, -1))
                {
                    if (hit.collider == target)
                    {
                        OnTargetDetected(target.gameObject, Vector3.Distance(transform.position, target.transform.position));
                    }
                }
            }
        }
    }

    void OnTargetDetected(GameObject target, float distance)
    {
        string visionType = hasFlashlight ? "손전등" : "일반 시야";
        Debug.Log($"[{gameObject.name}] {visionType}로 {target.name}을(를) {distance:F1}m 거리에서 발견!");
    }

    public void PickupFlashlight()
    {
        if (itemHolder != null)
        {
            currentFlashlight = itemHolder.EquipFlashlight();
            if (currentFlashlight != null)
            {
                Debug.Log($"{gameObject.name}이(가) 손전등을 들었습니다.");
            }
        }
    }

    public void DropFlashlight()
    {
        if (itemHolder != null)
        {
            itemHolder.UnequipItem();
            currentFlashlight = null;
            flashlightLight = null;
            Debug.Log($"{gameObject.name}이(가) 손전등을 버렸습니다.");
        }
    }

    public bool HasFlashlight()
    {
        return hasFlashlight;
    }

    void MoveToRandomPosition()
    {
        Vector3 currentPos = transform.position;
        
        for (int tryCount = 0; tryCount < 30; tryCount++)
        {
            float randomDistance = Random.Range(minMoveDistance, maxMoveDistance);
            float randomAngle = Random.Range(0f, 360f);
            Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
            
            Vector3 targetPos = currentPos + randomDirection * randomDistance;
            targetPos.y = currentPos.y;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
            {
                float distFromCenter = Vector3.Distance(navHit.position, areaCenter);
                if (distFromCenter <= areaRadius)
                {
                    NavMeshPath path = new NavMeshPath();
                    if (agent.CalculatePath(navHit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        float actualDistance = GetPathLength(path);
                        if (actualDistance >= minMoveDistance)
                        {
                            agent.SetDestination(navHit.position);
                            return;
                        }
                    }
                }
            }
        }
        
        FallbackMove();
    }

    void FallbackMove()
    {
        Vector3 randomPos = areaCenter + Random.insideUnitSphere * areaRadius;
        randomPos.y = transform.position.y;
        
        if (NavMesh.SamplePosition(randomPos, out NavMeshHit navHit, areaRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
    }

    float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(areaCenter, areaRadius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minMoveDistance);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxMoveDistance);

        float angle = Application.isPlaying ? currentViewAngle : normalViewAngle;
        float distance = Application.isPlaying ? currentViewDistance : normalViewDistance;
        bool usingFlashlight = Application.isPlaying && hasFlashlight;

        Gizmos.color = usingFlashlight ? Color.yellow : Color.cyan;
        
        Vector3 leftBoundary = Quaternion.Euler(0, -angle / 2f, 0) * transform.forward * distance;
        Vector3 rightBoundary = Quaternion.Euler(0, angle / 2f, 0) * transform.forward * distance;
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * distance);
        
        Vector3 previousPoint = transform.position + leftBoundary;
        for (int i = 1; i <= 30; i++)
        {
            float currentAngle = -angle / 2f + (angle / 30f) * i;
            Vector3 point = transform.position + Quaternion.Euler(0, currentAngle, 0) * transform.forward * distance;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
}