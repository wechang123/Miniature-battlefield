using UnityEngine;
using UnityEngine.Events;
using YajaGame.Gameplay.Combat;

namespace YajaGame.Gameplay.Weapons.Melee
{
    /// <summary>
    /// 근접 무기 기본 클래스
    /// 애니메이션과 연동하여 공격 처리
    /// </summary>
    public abstract class MeleeWeaponBase : MonoBehaviour
    {
        [Header("Weapon Data")]
        [SerializeField] protected WeaponData weaponData;

        [Header("Attack Settings")]
        [SerializeField] protected MeleeAttackHitbox hitbox; // 공격 히트박스
        [SerializeField] protected float attackCooldown = 0.5f; // 공격 쿨다운

        [Header("Animation (Optional)")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected string attackTriggerName = "Attack"; // 공격 애니메이션 트리거
        [SerializeField] protected string attackStateName = "Attack"; // 공격 애니메이션 상태 이름 (Play용)

        [Header("Audio")]
        [SerializeField] protected AudioClip attackSound; // 공격 사운드
        [SerializeField] protected AudioClip hitSound; // 타격 사운드
        [SerializeField] protected float soundVolume = 1f; // 사운드 볼륨

        [Header("Visual Effects")]
        [SerializeField] protected GameObject slashEffectPrefab; // 슬래시 이펙트 프리팹
        [SerializeField] protected Vector3 slashEffectOffset = new Vector3(0, 0, 1f); // 이펙트 위치 오프셋

        [Header("Events")]
        public UnityEvent OnAttackStarted = new UnityEvent();
        public UnityEvent OnAttackEnded = new UnityEvent();
        public UnityEvent<GameObject> OnHitEnemy = new UnityEvent<GameObject>();

        protected float _lastAttackTime = -999f;
        protected bool _isAttacking = false;

        // 프로퍼티
        public WeaponData WeaponData => weaponData;
        public bool IsAttacking => _isAttacking;
        public bool CanAttack => Time.time >= _lastAttackTime + attackCooldown;

        protected virtual void Awake()
        {
            // 히트박스 자동 찾기
            if (hitbox == null)
            {
                hitbox = GetComponentInChildren<MeleeAttackHitbox>();
            }

            // Animator 자동 찾기
            if (animator == null)
            {
                animator = GetComponentInParent<Animator>();
            }
        }

        protected virtual void Start()
        {
            // 시작 시 히트박스 비활성화
            if (hitbox != null)
            {
                hitbox.Deactivate();
            }
        }

        /// <summary>
        /// 공격 시도
        /// </summary>
        public virtual bool TryAttack()
        {
            if (!CanAttack)
            {
                float timeRemaining = attackCooldown - (Time.time - _lastAttackTime);
                Debug.Log($"[MeleeWeapon] 공격 쿨다운 중: {timeRemaining:F2}초 남음");
                return false;
            }

            if (_isAttacking)
            {
                Debug.Log($"[MeleeWeapon] 이미 공격 중");
                return false;
            }

            StartAttack();
            return true;
        }

        /// <summary>
        /// 공격 시작
        /// </summary>
        protected virtual void StartAttack()
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;

            string weaponName = weaponData != null ? weaponData.WeaponName : "Unknown";
            Debug.Log($"[MeleeWeapon] 공격 시작: {weaponName}");

            // 공격 사운드 재생
            if (attackSound != null)
            {
                AudioSource.PlayClipAtPoint(attackSound, transform.position, soundVolume);
            }

            // 슬래시 이펙트 생성
            SpawnSlashEffect();

            // 즉시 히트박스 활성화 (클릭하자마자 공격)
            ActivateHitbox();
            Invoke(nameof(DeactivateHitbox), 0.2f); // 0.2초 후 비활성화
            Invoke(nameof(EndAttack), 0.3f); // 0.3초 후 공격 종료

            // 애니메이션 즉시 재생 (강제)
            if (animator != null)
            {
                // Play를 사용하여 현재 상태 무시하고 즉시 재생
                animator.Play(attackStateName, 0, 0f); // 레이어 0, 정규화된 시간 0 (처음부터)
                Debug.Log($"[MeleeWeapon] 애니메이션 강제 재생: {attackStateName}");
            }

            OnAttackStarted?.Invoke();
        }

        /// <summary>
        /// 슬래시 이펙트 생성
        /// </summary>
        protected virtual void SpawnSlashEffect()
        {
            if (slashEffectPrefab == null)
            {
                Debug.LogWarning("[MeleeWeapon] slashEffectPrefab이 설정되지 않았습니다!");
                return;
            }

            // 플레이어 위치 기준으로 이펙트 생성
            Transform playerTransform = animator != null ? animator.transform : transform;
            Vector3 effectPos = playerTransform.position + playerTransform.TransformDirection(slashEffectOffset);
            effectPos.y += 1f; // 플레이어 높이 보정
            Quaternion effectRot = playerTransform.rotation;

            GameObject effect = Instantiate(slashEffectPrefab, effectPos, effectRot);
            Debug.Log($"[MeleeWeapon] 슬래시 이펙트 생성! 위치: {effectPos}");

            // 자동 삭제
            Destroy(effect, 1f);
        }

        /// <summary>
        /// 공격 종료
        /// </summary>
        protected virtual void EndAttack()
        {
            _isAttacking = false;

            Debug.Log($"[MeleeWeapon] 공격 종료");

            OnAttackEnded?.Invoke();
        }

        /// <summary>
        /// 히트박스 활성화 (Animation Event에서 호출)
        /// </summary>
        public virtual void ActivateHitbox()
        {
            if (hitbox != null)
            {
                hitbox.Activate();
                Debug.Log($"[MeleeWeapon] 히트박스 활성화");
            }
        }

        /// <summary>
        /// 히트박스 비활성화 (Animation Event에서 호출)
        /// </summary>
        public virtual void DeactivateHitbox()
        {
            if (hitbox != null)
            {
                hitbox.Deactivate();
                Debug.Log($"[MeleeWeapon] 히트박스 비활성화");
            }
        }

        /// <summary>
        /// 적을 타격했을 때 (히트박스에서 호출)
        /// </summary>
        public virtual void OnHitTarget(GameObject target, IDamageable damageable)
        {
            if (weaponData == null) return;

            // 데미지 정보 생성
            Vector3 hitDirection = (target.transform.position - transform.position).normalized;
            DamageInfo damageInfo = new DamageInfo(
                weaponData.Damage,
                weaponData.DamageType,
                gameObject,
                target.transform.position,
                hitDirection,
                weaponData.KnockbackForce
            );

            // 데미지 적용
            damageable.TakeDamage(damageInfo);

            // 히트 이펙트
            if (weaponData.HitEffectPrefab != null)
            {
                Instantiate(weaponData.HitEffectPrefab, target.transform.position, Quaternion.identity);
            }

            // 히트 사운드 (hitSound가 설정되어 있으면 우선 사용, 아니면 WeaponData의 FireSound 사용)
            AudioClip soundToPlay = hitSound != null ? hitSound : weaponData.FireSound;
            if (soundToPlay != null)
            {
                AudioSource.PlayClipAtPoint(soundToPlay, target.transform.position, soundVolume);
            }

            OnHitEnemy?.Invoke(target);

            Debug.Log($"[MeleeWeapon] {target.name}에게 {weaponData.Damage} 데미지!");
        }

        /// <summary>
        /// 무기 데이터 설정
        /// </summary>
        public virtual void SetWeaponData(WeaponData data)
        {
            weaponData = data;
            Debug.Log($"[MeleeWeapon] 무기 데이터 설정: {data.WeaponName}");
        }

        /// <summary>
        /// Animator 설정 (플레이어 애니메이터를 할당할 때 사용)
        /// </summary>
        public virtual void SetAnimator(Animator anim)
        {
            animator = anim;
            if (animator != null)
            {
                Debug.Log($"[MeleeWeapon] Animator 설정 완료");
            }
            else
            {
                Debug.LogWarning($"[MeleeWeapon] Animator가 null입니다!");
            }
        }

        // Animation Event용 메서드들
        public void OnAttackAnimationEnd()
        {
            EndAttack();
        }

        private void OnDrawGizmos()
        {
            // 공격 범위 표시
            if (_isAttacking)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 1.5f);
            }
        }
    }
}
