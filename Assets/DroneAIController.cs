using UnityEngine;
using System.Collections;
using YajaGame.Gameplay.Combat;

public class DroneAIController : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float flyHeight = 3f;
    public float patrolRadius = 20f;
    public Vector3 centerPosition;
    public float obstacleAvoidDistance = 3f;
    public LayerMask obstacleLayer;

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

    [Header("Attack Settings")]
    public float attackRange = 10f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 15f;
    private float lastAttackTime = 0f;

    [Header("Visual Effects")]
    public Transform[] propellers;
    public float propellerIdleSpeed = 1000f;
    public float propellerChaseSpeed = 2000f;
    public Light droneLight;
    public float lightIdleIntensity = 2f;
    public float lightChaseIntensity = 5f;
    public Color lightIdleColor = Color.white;
    public Color lightChaseColor = Color.red;

    [Header("Renderer")]
    public Renderer bodyRenderer;
    public Color normalColor = Color.white;
    public Color hitColor = Color.yellow;

    [Header("Health System")]
    public float maxHealth = 100f;
    public float currentHealth;
    public GameObject explosionEffect;

    private Transform player;
    private bool isChasing = false;
    private float loseTimer = 0f;
    private Vector3 patrolTarget;
    private float currentPropellerSpeed;
    private float stuckTimer = 0f;
    private Vector3 lastPosition;
    private bool isDead = false;

    // IDamageable 인터페이스 구현
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => !isDead;
    public Transform Transform => transform;

    void Start()
    {
        Debug.Log("? ��� AI ����!");
        
        currentHealth = maxHealth;
        centerPosition = transform.position;
        lastPosition = transform.position;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log($"? �÷��̾� ã��: {player.name}");
        }
        else
        {
            Debug.LogError("? �÷��̾ ã�� �� �����ϴ�! Tag 'Player' Ȯ��!");
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
            droneLight.range = 40f;
        }

        if (bodyRenderer == null)
        {
            bodyRenderer = GetComponentInChildren<MeshRenderer>();
        }
        
        if (bodyRenderer != null)
        {
            normalColor = bodyRenderer.material.color;
        }

        currentPropellerSpeed = propellerIdleSpeed;
        SetNewPatrolTarget();
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        bool seePlayer = CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        CheckIfStuck();

        if (seePlayer)
        {
            if (!isChasing)
            {
                isChasing = true;
                Debug.Log("? ���: �÷��̾� �߰�!");
            }

            Vector3 targetPos = player.position + Vector3.up * flyHeight;
            targetPos = ClampToBoundaries(targetPos);
            MoveWithObstacleAvoidance(targetPos, chaseSpeed);

            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
            }

            loseTimer = 0f;
        }
        else if (isChasing)
        {
            loseTimer += Time.deltaTime;
            if (loseTimer >= losePlayerTime)
            {
                isChasing = false;
                SetNewPatrolTarget();
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, patrolTarget) < 2f)
            {
                SetNewPatrolTarget();
            }
            MoveWithObstacleAvoidance(patrolTarget, moveSpeed);
        }

        KeepInBounds();
        RotateDrone();
        UpdateVisualEffects();
        lastPosition = transform.position;
    }

    void AttackPlayer()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        lastAttackTime = Time.time;
        
        Debug.Log("��� �߻�!");
        
        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        Vector3 direction = (player.position - spawnPos).normalized;
        
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));
            
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed;
            }
        }
        
        StartCoroutine(AttackFlash());
    }

    IEnumerator AttackFlash()
    {
        if (droneLight != null)
        {
            Color originalColor = droneLight.color;
            float originalIntensity = droneLight.intensity;
            
            for (int i = 0; i < 2; i++)
            {
                droneLight.color = Color.red;
                droneLight.intensity = 8f;
                yield return new WaitForSeconds(0.1f);
                
                droneLight.color = originalColor;
                droneLight.intensity = originalIntensity;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        Debug.Log($"[드론] DamageInfo로 피격! 데미지: {damageInfo.Amount}");
        TakeDamage(damageInfo.Amount);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"[드론] 피격! 데미지: {damage}, 남은 체력: {currentHealth}/{maxHealth}");

        StartCoroutine(HitColorChange());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitColorChange()
    {
        if (bodyRenderer != null)
        {
            bodyRenderer.material.color = hitColor;
            yield return new WaitForSeconds(0.2f);
            bodyRenderer.material.color = normalColor;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[드론] 파괴됨!");

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // RoundManager에 알림
        if (YajaGame.Gameplay.RoundManager.Instance != null)
        {
            YajaGame.Gameplay.RoundManager.Instance.OnEnemyDeath(gameObject);
        }

        DropPropellers();
        Destroy(gameObject, 0.5f);
    }

    void DropPropellers()
    {
        foreach (Transform propeller in propellers)
        {
            if (propeller != null)
            {
                propeller.SetParent(null);
                Rigidbody rb = propeller.gameObject.AddComponent<Rigidbody>();
                rb.AddForce(Random.insideUnitSphere * 5f, ForceMode.Impulse);
                Destroy(propeller.gameObject, 5f);
            }
        }
    }

    void MoveWithObstacleAvoidance(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, obstacleAvoidDistance))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                Vector3 avoidDir = Vector3.Cross(direction, Vector3.up);
                if (Random.value > 0.5f) avoidDir = -avoidDir;
                direction = (direction + avoidDir).normalized;
            }
        }
        
        transform.position += direction * speed * Time.deltaTime;
    }

    void CheckIfStuck()
    {
        float moveDistance = Vector3.Distance(transform.position, lastPosition);
        
        if (moveDistance < 0.1f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 2f)
            {
                SetNewPatrolTarget();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    void SetNewPatrolTarget()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            Vector3 testTarget = centerPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            testTarget.y = flyHeight;
            testTarget = ClampToBoundaries(testTarget);
            
            Vector3 dirToTarget = testTarget - transform.position;
            if (!Physics.Raycast(transform.position, dirToTarget.normalized, dirToTarget.magnitude - 1f))
            {
                patrolTarget = testTarget;
                return;
            }
        }
        
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
        float targetSpeed = isChasing ? propellerChaseSpeed : propellerIdleSpeed;
        currentPropellerSpeed = Mathf.Lerp(currentPropellerSpeed, targetSpeed, Time.deltaTime * 2f);

        foreach (Transform propeller in propellers)
        {
            if (propeller != null)
            {
                propeller.Rotate(Vector3.up, currentPropellerSpeed * Time.deltaTime);
            }
        }

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

        Vector3 playerCenter = player.position + Vector3.up * 1f;
        Vector3 dirToCenter = playerCenter - transform.position;
        
        if (Physics.Raycast(transform.position, dirToCenter.normalized, out RaycastHit hit, viewDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void OnCollisionEnter(Collision collision)
    {
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
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleAvoidDistance);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}