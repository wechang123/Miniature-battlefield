using UnityEngine;
using UnityEngine.Events;

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

        [Header("Death Settings")]
        [SerializeField] private float deathDelay = 2f; // 죽은 후 제거까지 시간
        [SerializeField] private GameObject deathEffectPrefab;

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
        private bool _isDead = false;

        private void Awake()
        {
            currentHealth = maxHealth;
            _renderers = GetComponentsInChildren<Renderer>();
            _rigidbody = GetComponent<Rigidbody>();
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
            if (_rigidbody != null && damageInfo.KnockbackForce > 0)
            {
                _rigidbody.AddForce(damageInfo.Direction * damageInfo.KnockbackForce, ForceMode.Impulse);
            }
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
