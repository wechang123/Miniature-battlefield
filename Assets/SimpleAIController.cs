using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    public float attackDuration = 1f;
    public float attackDamage = 50f;
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    [Header("Flashlight")]
    public Light flashlight;
    public float patrolLightIntensity = 1.5f;
    public float chaseLightIntensity = 3f;

    [Header("Health System")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isInvincible = false;
    public bool canKillPlayer = true;
    public bool isBoss = false;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.3f;
    private bool isKnockedBack = false;
    private Vector3 knockbackVelocity;

    [Header("Visual Effects")]
    public GameObject deathEffect;
    public Renderer bodyRenderer;
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private bool isChasing = false;
    private bool isAttacking = false;
    private bool isDead = false;
    private float loseTimer = 0f;
    private float waitTimer = 0f;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;

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

            Debug.Log($"플레이어 찾음: {player.name}");
        }
        else
        {
            Debug.LogError("플레이어를 찾을 수 없습니다! Tag 'Player' 확인!");
        }

        if (bodyRenderer == null)
        {
            bodyRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<MeshRenderer>();
            }
        }

        lastPosition = transform.position;
        MoveToRandomPoint();
    }

    void Update()
    {
        if (isDead) return;

        if (isKnockedBack)
        {
            ApplyKnockback();
            return;
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

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= catchDistance)
            {
                if (canKillPlayer)
                {
                    CatchPlayer();
                }
                else
                {
                    AttackPlayer();
                }
            }
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

    void AttackPlayer()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;

        Debug.Log("선생님이 플레이어를 공격!");

        if (animator != null)
        {
            animator.SetTrigger(attackTrigger);
        }

        // SendMessage로 플레이어 데미지
        if (player != null)
        {
            player.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        if (isDead) return;

        if (isInvincible)
        {
            Debug.Log("선생님은 무적 상태입니다!");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} 피격! 남은 체력: {currentHealth}/{maxHealth}");

        ApplyKnockbackEffect(hitDirection);
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ApplyKnockbackEffect(Vector3 hitDirection)
    {
        isKnockedBack = true;
        agent.enabled = false;

        knockbackVelocity = -hitDirection.normalized * knockbackForce;
        knockbackVelocity.y = 0;

        Invoke(nameof(EndKnockback), knockbackDuration);
    }

    void ApplyKnockback()
    {
        transform.position += knockbackVelocity * Time.deltaTime;
        knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * 10f);
    }

    void EndKnockback()
    {
        isKnockedBack = false;
        agent.enabled = true;
    }

    IEnumerator HitFlash()
    {
        if (bodyRenderer != null)
        {
            Color originalColor = bodyRenderer.material.color;
            bodyRenderer.material.color = hitColor;
            yield return new WaitForSeconds(0.1f);
            bodyRenderer.material.color = originalColor;
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} 사망!");

        agent.isStopped = true;
        agent.enabled = false;
        this.enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position + Vector3.up, Quaternion.identity);
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(gameObject, 5f);
    }

    void CatchPlayer()
    {
        Debug.Log("플레이어를 잡았습니다!");

        isAttacking = true;
        agent.isStopped = true;
        agent.enabled = false;

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetTrigger(attackTrigger);
        }

        Invoke(nameof(TriggerGameOver), attackDuration);
        Invoke(nameof(DisableAI), attackDuration + 0.1f);
    }

    void TriggerGameOver()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerCaught(transform);
        }
    }

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

    void SetFlashlightChaseMode(bool chasing)
    {
        if (flashlight == null) return;

        if (chasing)
        {
            flashlight.intensity = chaseLightIntensity;
            flashlight.color = new Color(1f, 0.9f, 0.9f);
        }
        else
        {
            flashlight.intensity = patrolLightIntensity;
            flashlight.color = new Color(1f, 0.95f, 0.8f);
        }
    }
}
