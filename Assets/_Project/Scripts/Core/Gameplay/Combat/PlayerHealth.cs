using UnityEngine;
using UnityEngine.Events;

namespace YajaGame.Gameplay.Combat
{
    /// <summary>
    /// 플레이어 체력 시스템
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Invincibility")]
        [SerializeField] private float invincibilityDuration = 1f;
        private float _invincibilityTimer = 0f;
        private bool _isInvincible = false;

        [Header("Audio")]
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;

        [Header("Events")]
        public UnityEvent<float, float> OnHealthChanged = new UnityEvent<float, float>(); // current, max
        public UnityEvent<DamageInfo> OnDamageTaken = new UnityEvent<DamageInfo>();
        public UnityEvent OnPlayerDeath = new UnityEvent();

        // 프로퍼티 (IDamageable 구현)
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;
        public Transform Transform => transform;

        private bool _isDead = false;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Start()
        {
            // 초기 HP 이벤트 발생 (UI 초기화용)
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Update()
        {
            // 무적 시간 타이머
            if (_isInvincible)
            {
                _invincibilityTimer -= Time.deltaTime;
                if (_invincibilityTimer <= 0f)
                {
                    _isInvincible = false;
                }
            }
        }

        /// <summary>
        /// 데미지를 받습니다
        /// </summary>
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (_isDead) return;
            if (_isInvincible) return;

            // 체력 감소
            currentHealth -= damageInfo.Amount;
            currentHealth = Mathf.Max(0, currentHealth);

            Debug.Log($"[PlayerHealth] 플레이어가 {damageInfo.Amount} 데미지를 받았습니다. (남은 체력: {currentHealth}/{maxHealth})");

            // 히트 사운드
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }

            // TODO: 카메라 쉐이크 (CameraShake 스크립트 설정 후 활성화)
            // if (YajaGame.Effects.CameraShake.Instance != null)
            // {
            //     YajaGame.Effects.CameraShake.Instance.ShakeOnDamage();
            // }

            // 무적 시간 시작
            _isInvincible = true;
            _invincibilityTimer = invincibilityDuration;

            // 이벤트 발생
            OnDamageTaken?.Invoke(damageInfo);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

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

            float previousHealth = currentHealth;
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            if (currentHealth != previousHealth)
            {
                Debug.Log($"[PlayerHealth] 플레이어가 {amount} 체력을 회복했습니다. (현재 체력: {currentHealth}/{maxHealth})");
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// 죽음 처리
        /// </summary>
        private void Die()
        {
            _isDead = true;

            Debug.Log("[PlayerHealth] 플레이어가 사망했습니다!");

            // 죽음 사운드
            if (deathSound != null)
            {
                AudioSource.PlayClipAtPoint(deathSound, transform.position);
            }

            // 이벤트 발생
            OnPlayerDeath?.Invoke();

            // GameManager에 알림
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerCaught();
            }
            else
            {
                Debug.LogWarning("[PlayerHealth] GameManager.Instance를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 무적 상태 확인
        /// </summary>
        public bool IsInvincible => _isInvincible;

        /// <summary>
        /// 체력 초기화
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            _isDead = false;
            _isInvincible = false;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        #region Debug Methods

        [ContextMenu("Take 10 Damage")]
        public void DebugTake10Damage()
        {
            TakeDamage(DamageInfo.Create(10f, DamageType.Physical, null));
        }

        [ContextMenu("Take 50 Damage")]
        public void DebugTake50Damage()
        {
            TakeDamage(DamageInfo.Create(50f, DamageType.Physical, null));
        }

        [ContextMenu("Heal 20")]
        public void DebugHeal20()
        {
            Heal(20f);
        }

        [ContextMenu("Kill Player")]
        public void DebugKillPlayer()
        {
            currentHealth = 0;
            Die();
        }

        [ContextMenu("Print Health")]
        public void PrintHealth()
        {
            Debug.Log($"[PlayerHealth] 플레이어 체력: {currentHealth}/{maxHealth} ({(currentHealth / maxHealth) * 100:F1}%)");
        }

        #endregion
    }
}
