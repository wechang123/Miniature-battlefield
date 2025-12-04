using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

namespace YajaGame.Gameplay.Combat
{
    /// <summary>
    /// 적 체력 시스템
    /// </summary>
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Damage Feedback")]
        [SerializeField] private float hitFlashDuration = 0.1f;
        [SerializeField] private Color hitFlashColor = Color.red;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;

        [Header("Knockback Settings")]
        [SerializeField] private float knockbackDuration = 1.0f; // 넉백 지속 시간
        [SerializeField] private float knockbackStunTime = 1.0f; // 넉백 후 경직 시간
        [SerializeField] private float knockbackDistanceMultiplier = 1.0f; // 넉백 거리 배율

        [Header("Death Settings")]
        [SerializeField] private float deathDelay = 2f; // 죽은 후 제거까지 시간
        [SerializeField] private GameObject deathEffectPrefab;

        [Header("Drop Settings")]
        [SerializeField] private GameObject[] dropPrefabs;  // 드랍할 아이템 프리팹들
        [SerializeField] private float dropChance = 0.5f;   // 드랍 확률 (0~1)
        [SerializeField] private int minDropCount = 1;      // 최소 드랍 개수
        [SerializeField] private int maxDropCount = 2;      // 최대 드랍 개수
        [SerializeField] private float dropSpreadRadius = 0.5f;  // 드랍 아이템 퍼지는 범위
        [SerializeField] private float dropUpwardForce = 3f;     // 위로 튀어오르는 힘

        [Header("Events")]
        public UnityEvent<DamageInfo> OnDamageTaken = new UnityEvent<DamageInfo>();
        public UnityEvent OnDeath = new UnityEvent();

        // 프로퍼티
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;
        public Transform Transform => transform;

        private Renderer[] _renderers;
        private Rigidbody _rigidbody;
        private NavMeshAgent _navAgent;
        private Animator _animator;
        private MonoBehaviour _aiController; // SimpleAIController 등
        private bool _isDead = false;
        private bool _isKnockedBack = false;

        private void Awake()
        {
            currentHealth = maxHealth;
            _renderers = GetComponentsInChildren<Renderer>();
            _rigidbody = GetComponent<Rigidbody>();
            _navAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            // AI 컨트롤러 찾기 (SimpleAIController 등)
            _aiController = GetComponent("SimpleAIController") as MonoBehaviour;
        }

        /// <summary>
        /// 데미지를 받습니다
        /// </summary>
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (_isDead) return;

            // 체력 감소
            currentHealth -= damageInfo.Amount;
            currentHealth = Mathf.Max(0, currentHealth);

            Debug.Log($"[EnemyHealth] {gameObject.name}이(가) {damageInfo.Amount} 데미지를 받았습니다. (남은 체력: {currentHealth}/{maxHealth})");

            // 히트 이펙트
            PlayHitEffect(damageInfo);

            // 넉백
            ApplyKnockback(damageInfo);

            // 이벤트 발생
            OnDamageTaken?.Invoke(damageInfo);

            // 죽음 체크
            if (currentHealth <= 0 && !_isDead)
            {
                Die();
            }
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(float amount)
        {
            if (_isDead) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            Debug.Log($"[EnemyHealth] {gameObject.name}이(가) {amount} 체력을 회복했습니다. (현재 체력: {currentHealth}/{maxHealth})");
        }

        /// <summary>
        /// 히트 이펙트 재생
        /// </summary>
        private void PlayHitEffect(DamageInfo damageInfo)
        {
            // 히트 이펙트 생성
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, damageInfo.HitPoint, Quaternion.LookRotation(damageInfo.Direction));
            }

            // 히트 사운드
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }

            // 깜빡임 효과
            StartCoroutine(FlashEffect());
        }

        /// <summary>
        /// 깜빡임 효과
        /// </summary>
        private System.Collections.IEnumerator FlashEffect()
        {
            // 원래 색상 저장
            Color[][] originalColors = new Color[_renderers.Length][];
            for (int i = 0; i < _renderers.Length; i++)
            {
                Material[] materials = _renderers[i].materials;
                originalColors[i] = new Color[materials.Length];
                for (int j = 0; j < materials.Length; j++)
                {
                    originalColors[i][j] = materials[j].color;
                    materials[j].color = hitFlashColor;
                }
            }

            yield return new WaitForSeconds(hitFlashDuration);

            // 원래 색상 복원
            for (int i = 0; i < _renderers.Length; i++)
            {
                Material[] materials = _renderers[i].materials;
                for (int j = 0; j < materials.Length && j < originalColors[i].Length; j++)
                {
                    materials[j].color = originalColors[i][j];
                }
            }
        }

        /// <summary>
        /// 넉백 적용
        /// </summary>
        private void ApplyKnockback(DamageInfo damageInfo)
        {
            if (damageInfo.KnockbackForce <= 0) return;
            if (_isKnockedBack) return;

            // NavMeshAgent가 있으면 잠시 비활성화
            if (_navAgent != null && _navAgent.enabled)
            {
                StartCoroutine(KnockbackCoroutine(damageInfo));
            }
            else if (_rigidbody != null)
            {
                // NavMeshAgent 없으면 바로 넉백
                _rigidbody.AddForce(damageInfo.Direction * damageInfo.KnockbackForce, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// 넉백 코루틴 (NavMeshAgent 잠시 비활성화)
        /// </summary>
        private System.Collections.IEnumerator KnockbackCoroutine(DamageInfo damageInfo)
        {
            _isKnockedBack = true;

            // AI 컨트롤러 비활성화 (애니메이션 덮어쓰기 방지)
            if (_aiController != null)
            {
                _aiController.enabled = false;
            }

            // NavMeshAgent 비활성화
            _navAgent.isStopped = true;
            _navAgent.updatePosition = false;
            _navAgent.updateRotation = false;

            // 애니메이션 중지
            if (_animator != null)
            {
                _animator.speed = 0f;
            }

            // 직접 위치 이동 (Rigidbody 대신 Transform 사용)
            Vector3 knockbackDirection = damageInfo.Direction;
            knockbackDirection.y = 0; // 수평으로만 밀림
            knockbackDirection.Normalize();

            float knockbackDistance = damageInfo.KnockbackForce * knockbackDistanceMultiplier; // 거리 계산
            Vector3 targetPosition = transform.position + knockbackDirection * knockbackDistance;

            Debug.Log($"[EnemyHealth] 넉백! 방향: {knockbackDirection}, 거리: {knockbackDistance}");

            // 부드럽게 밀려남 (이동 중 충돌 체크)
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            float checkRadius = 0.4f; // 캐릭터 반경

            while (elapsed < knockbackDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / knockbackDuration;
                t = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic

                Vector3 nextPos = Vector3.Lerp(startPos, targetPosition, t);
                Vector3 moveDir = nextPos - transform.position;
                float moveDist = moveDir.magnitude;

                if (moveDist > 0.001f)
                {
                    // 이동 방향으로 장애물 체크 (자기 자신 제외)
                    Vector3 origin = transform.position + Vector3.up * 0.5f;
                    if (Physics.SphereCast(origin, checkRadius, moveDir.normalized, out RaycastHit hit, moveDist + 0.1f))
                    {
                        // 자기 자신이 아니면 멈춤
                        if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
                        {
                            Debug.Log($"[EnemyHealth] 넉백 중 장애물 감지: {hit.collider.name}");
                            break;
                        }
                    }
                    transform.position = nextPos;
                }

                yield return null;
            }

            // 경직 시간 대기
            yield return new WaitForSeconds(knockbackStunTime);

            // 애니메이션 재개
            if (_animator != null)
            {
                _animator.speed = 1f;
            }

            // NavMeshAgent 다시 활성화
            if (!_isDead && _navAgent != null)
            {
                _navAgent.Warp(transform.position); // 현재 위치로 NavMesh 동기화
                _navAgent.updatePosition = true;
                _navAgent.updateRotation = true;
                _navAgent.isStopped = false;
            }

            // AI 컨트롤러 다시 활성화
            if (!_isDead && _aiController != null)
            {
                _aiController.enabled = true;
            }

            _isKnockedBack = false;
        }

        /// <summary>
        /// 죽음 처리
        /// </summary>
        private void Die()
        {
            _isDead = true;

            Debug.Log($"[EnemyHealth] {gameObject.name}이(가) 죽었습니다!");

            // 죽음 사운드
            if (deathSound != null)
            {
                AudioSource.PlayClipAtPoint(deathSound, transform.position);
            }

            // 죽음 이펙트
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // 아이템 드랍
            SpawnDrops();

            // 이벤트 발생
            OnDeath?.Invoke();

            // AI 비활성화 (있다면)
            var ai = GetComponent<MonoBehaviour>();
            if (ai != null)
            {
                ai.enabled = false;
            }

            // Collider 비활성화
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // 일정 시간 후 제거
            Destroy(gameObject, deathDelay);
        }

        /// <summary>
        /// 아이템 드랍
        /// </summary>
        private void SpawnDrops()
        {
            // 드랍 프리팹이 없으면 종료
            if (dropPrefabs == null || dropPrefabs.Length == 0)
            {
                Debug.Log($"[EnemyHealth] {gameObject.name}: 드랍 프리팹이 설정되지 않음");
                return;
            }

            // 드랍 확률 체크
            if (Random.value > dropChance)
            {
                Debug.Log($"[EnemyHealth] {gameObject.name}: 드랍 확률 실패 ({dropChance * 100}%)");
                return;
            }

            // 드랍 개수 결정
            int dropCount = Random.Range(minDropCount, maxDropCount + 1);
            Debug.Log($"[EnemyHealth] {gameObject.name}: {dropCount}개 아이템 드랍!");

            // 아이템 스폰
            for (int i = 0; i < dropCount; i++)
            {
                // 랜덤 프리팹 선택
                GameObject prefab = dropPrefabs[Random.Range(0, dropPrefabs.Length)];
                if (prefab == null) continue;

                // 스폰 위치 계산 (약간 랜덤하게 퍼뜨림)
                Vector2 randomCircle = Random.insideUnitCircle * dropSpreadRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);

                // 아이템 생성
                GameObject droppedItem = Instantiate(prefab, spawnPos, Quaternion.identity);

                // Rigidbody가 있으면 위로 튀어오르게
                Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 randomDirection = new Vector3(
                        Random.Range(-1f, 1f),
                        1f,
                        Random.Range(-1f, 1f)
                    ).normalized;
                    rb.AddForce(randomDirection * dropUpwardForce, ForceMode.Impulse);
                }

                Debug.Log($"[EnemyHealth] 드랍 아이템 생성: {prefab.name} at {spawnPos}");
            }
        }

        /// <summary>
        /// 디버그: 즉시 죽이기
        /// </summary>
        [ContextMenu("Kill Enemy")]
        public void KillInstantly()
        {
            currentHealth = 0;
            Die();
        }

        /// <summary>
        /// 디버그: 체력 출력
        /// </summary>
        [ContextMenu("Print Health")]
        public void PrintHealth()
        {
            Debug.Log($"[EnemyHealth] {gameObject.name} - 체력: {currentHealth}/{maxHealth} ({(currentHealth / maxHealth) * 100:F1}%)");
        }

        private void OnDrawGizmosSelected()
        {
            // 체력 바 표시 (에디터용)
            if (!Application.isPlaying) return;

            Gizmos.color = Color.red;
            Vector3 healthBarPos = transform.position + Vector3.up * 2f;
            float healthPercent = currentHealth / maxHealth;
            Gizmos.DrawWireCube(healthBarPos, new Vector3(1f, 0.1f, 0.1f));
            Gizmos.color = Color.green;
            Gizmos.DrawCube(healthBarPos - Vector3.right * (1 - healthPercent) * 0.5f, new Vector3(healthPercent, 0.1f, 0.1f));
        }
    }
}
