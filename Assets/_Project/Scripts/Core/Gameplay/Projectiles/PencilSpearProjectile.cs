using UnityEngine;
using YajaGame.Gameplay.Combat;

namespace YajaGame.Gameplay.Projectiles
{
    /// <summary>
    /// 연필창 발사체
    /// 특징: 적을 관통하며 날아감
    /// </summary>
    public class PencilSpearProjectile : WeaponProjectileBase
    {
        [Header("Pencil Spear Settings")]
        [SerializeField] private int maxPenetrations = 3; // 최대 관통 횟수
        [SerializeField] private float penetrationDamageMultiplier = 0.8f; // 관통 시 데미지 감소율
        [SerializeField] private bool rotateWhileFlying = true; // 날아가면서 회전
        [SerializeField] private float rotationSpeed = 720f; // 회전 속도 (도/초)

        private int _penetrationCount = 0;

        protected override void Awake()
        {
            base.Awake();

            // 관통을 위해 충돌 시 파괴하지 않음
            destroyOnHit = false;
        }

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            // 날아가면서 회전
            if (rotateWhileFlying && _rigidbody != null && _rigidbody.linearVelocity.magnitude > 0.1f)
            {
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }
        }

        protected override void HandleEnemyHit(Collision collision, IDamageable damageable)
        {
            if (weaponData == null) return;

            // 관통 횟수 증가
            _penetrationCount++;

            // 충돌 지점 계산
            Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            Vector3 hitDirection = (collision.transform.position - transform.position).normalized;

            // 데미지 계산 (관통할수록 감소)
            float damage = weaponData.Damage * Mathf.Pow(penetrationDamageMultiplier, _penetrationCount - 1);

            // 데미지 정보 생성
            DamageInfo damageInfo = new DamageInfo(
                damage,
                weaponData.DamageType,
                gameObject,
                hitPoint,
                hitDirection,
                weaponData.KnockbackForce * 0.5f // 넉백은 절반
            );

            // 데미지 적용
            damageable.TakeDamage(damageInfo);

            // 히트 이펙트
            if (weaponData.HitEffectPrefab != null)
            {
                Instantiate(weaponData.HitEffectPrefab, hitPoint, Quaternion.LookRotation(hitDirection));
            }

            OnHitEnemy?.Invoke(collision.gameObject);

            Debug.Log($"[PencilSpear] 적 관통! {collision.gameObject.name}에게 {damage:F1} 데미지 (관통 {_penetrationCount}회)");

            // 최대 관통 횟수 도달 시 파괴
            if (_penetrationCount >= maxPenetrations)
            {
                Debug.Log($"[PencilSpear] 최대 관통 횟수 도달. 파괴됩니다.");
                DestroyProjectile();
            }
        }

        protected override void HandleEnvironmentHit(Collision collision)
        {
            // 환경과 충돌 시 즉시 파괴
            base.HandleEnvironmentHit(collision);
            DestroyProjectile();
        }

        private void OnDrawGizmos()
        {
            // 관통 횟수 표시
            if (Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.3f + _penetrationCount * 0.1f);
            }
        }
    }
}
