using UnityEngine;
using UnityEngine.Events;
using YajaGame.Gameplay.Combat;
using YajaGame.Gameplay.Weapons;

namespace YajaGame.Gameplay.Projectiles
{
    /// <summary>
    /// 무기 발사체 기본 클래스
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public abstract class WeaponProjectileBase : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] protected float lifeTime = 5f;

        [Header("Hit Settings")]
        [SerializeField] protected LayerMask hitLayers = -1; // 충돌 가능한 레이어
        [SerializeField] protected bool destroyOnHit = true; // 충돌 시 파괴 여부

        [Header("Events")]
        public UnityEvent<GameObject> OnHitEnemy = new UnityEvent<GameObject>();
        public UnityEvent<Vector3> OnHitEnvironment = new UnityEvent<Vector3>();
        public UnityEvent OnDestroyed = new UnityEvent();

        protected WeaponData weaponData;
        protected Rigidbody _rigidbody;
        protected Collider _collider;
        protected bool _hasHit = false;
        protected float _spawnTime;

        // 프로퍼티
        public WeaponData WeaponData => weaponData;
        public bool HasHit => _hasHit;

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();

            // Collider 설정
            if (_collider != null)
            {
                _collider.isTrigger = false; // 물리 충돌 사용
            }
        }

        protected virtual void Start()
        {
            _spawnTime = Time.time;

            // 수명 후 자동 파괴
            Destroy(gameObject, lifeTime);
        }

        /// <summary>
        /// 발사체 초기화
        /// </summary>
        public virtual void Initialize(WeaponData weapon, Vector3 direction)
        {
            weaponData = weapon;

            // 속도 설정
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = direction.normalized * weapon.ProjectileSpeed;
                _rigidbody.useGravity = weapon.ProjectileGravityScale > 0;

                if (weapon.ProjectileGravityScale != 1f)
                {
                    // 중력 스케일 조정 (Unity 2022.3 이상)
                    _rigidbody.linearDamping = weapon.ProjectileGravityScale;
                }
            }

            // 회전 설정 (날아가는 방향으로)
            transform.rotation = Quaternion.LookRotation(direction);

            // 수명 업데이트
            lifeTime = weapon.ProjectileLifetime;

            Debug.Log($"[WeaponProjectileBase] 발사체 초기화: {weapon.WeaponName}, 속도={weapon.ProjectileSpeed}");
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (_hasHit) return;

            // 레이어 체크
            if (((1 << collision.gameObject.layer) & hitLayers) == 0)
            {
                return; // 충돌 대상이 아님
            }

            _hasHit = true;

            // 적과 충돌
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                HandleEnemyHit(collision, damageable);
            }
            else
            {
                // 환경과 충돌
                HandleEnvironmentHit(collision);
            }

            // 충돌 시 파괴
            if (destroyOnHit)
            {
                DestroyProjectile();
            }
        }

        /// <summary>
        /// 적과의 충돌 처리
        /// </summary>
        protected virtual void HandleEnemyHit(Collision collision, IDamageable damageable)
        {
            if (weaponData == null) return;

            // 충돌 지점 계산
            Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            Vector3 hitDirection = (collision.transform.position - transform.position).normalized;

            // 데미지 정보 생성
            DamageInfo damageInfo = new DamageInfo(
                weaponData.Damage,
                weaponData.DamageType,
                gameObject,
                hitPoint,
                hitDirection,
                weaponData.KnockbackForce
            );

            // 데미지 적용
            damageable.TakeDamage(damageInfo);

            // 히트 이펙트
            if (weaponData.HitEffectPrefab != null)
            {
                Instantiate(weaponData.HitEffectPrefab, hitPoint, Quaternion.LookRotation(hitDirection));
            }

            OnHitEnemy?.Invoke(collision.gameObject);

            Debug.Log($"[WeaponProjectileBase] 적 명중! {collision.gameObject.name}에게 {weaponData.Damage} 데미지");
        }

        /// <summary>
        /// 환경과의 충돌 처리
        /// </summary>
        protected virtual void HandleEnvironmentHit(Collision collision)
        {
            Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;

            // 히트 이펙트
            if (weaponData != null && weaponData.HitEffectPrefab != null)
            {
                Instantiate(weaponData.HitEffectPrefab, hitPoint, Quaternion.LookRotation(collision.contacts[0].normal));
            }

            OnHitEnvironment?.Invoke(hitPoint);

            Debug.Log($"[WeaponProjectileBase] 환경 충돌: {collision.gameObject.name}");
        }

        /// <summary>
        /// 발사체 파괴
        /// </summary>
        protected virtual void DestroyProjectile()
        {
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            // 발사체 궤적 표시 (디버그용)
            if (_rigidbody != null && Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + _rigidbody.linearVelocity.normalized * 0.5f);
            }
        }
    }
}
