using System.Collections.Generic;
using UnityEngine;
using YajaGame.Gameplay.Combat;

namespace YajaGame.Gameplay.Projectiles
{
    /// <summary>
    /// 지우개폭탄 발사체
    /// 특징: 충돌 시 범위 폭발 데미지
    /// </summary>
    public class EraserBombProjectile : WeaponProjectileBase
    {
        [Header("Eraser Bomb Settings")]
        [SerializeField] private float explosionRadius = 3f; // 폭발 반경
        [SerializeField] private float explosionDamageMultiplier = 1.5f; // 폭발 중심 데미지 배율
        [SerializeField] private float explosionForce = 500f; // 폭발 힘
        [SerializeField] private GameObject explosionEffectPrefab; // 폭발 이펙트
        [SerializeField] private AudioClip explosionSound; // 폭발 사운드
        [SerializeField] private LayerMask explosionLayers = -1; // 폭발 영향 레이어
        [SerializeField] private bool showExplosionRadius = true; // 폭발 반경 표시

        [Header("Fuse Settings")]
        [SerializeField] private float fuseTime = 3f; // 퓨즈 시간 (이 시간 후 자동 폭발, -1이면 비활성화)
        [SerializeField] private bool explodeOnImpact = true; // 충돌 시 즉시 폭발

        private float _spawnTime;
        private bool _hasExploded = false;

        protected override void Start()
        {
            base.Start();
            _spawnTime = Time.time;

            // 폭발 시 파괴
            destroyOnHit = true;
        }

        private void Update()
        {
            // 퓨즈 시간 체크
            if (fuseTime > 0 && !_hasExploded && Time.time - _spawnTime >= fuseTime)
            {
                Debug.Log("[EraserBomb] 퓨즈 시간 종료. 폭발!");
                Explode(transform.position);
            }
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            if (_hasExploded) return;

            // 충돌 시 폭발
            if (explodeOnImpact)
            {
                Vector3 explosionPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
                Explode(explosionPoint);
            }
            else
            {
                base.OnCollisionEnter(collision);
            }
        }

        /// <summary>
        /// 폭발 실행
        /// </summary>
        private void Explode(Vector3 explosionPoint)
        {
            if (_hasExploded) return;
            _hasExploded = true;

            Debug.Log($"[EraserBomb] 폭발! 위치={explosionPoint}, 반경={explosionRadius}");

            // 폭발 이펙트
            if (explosionEffectPrefab != null)
            {
                GameObject effect = Instantiate(explosionEffectPrefab, explosionPoint, Quaternion.identity);
                // 크기를 폭발 반경에 맞춤
                effect.transform.localScale = Vector3.one * (explosionRadius * 0.5f);
                Destroy(effect, 3f);
            }

            // 폭발 사운드
            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, explosionPoint, 1f);
            }

            // 범위 내 모든 적에게 데미지
            ApplyExplosionDamage(explosionPoint);

            // 물리적 폭발력 (Rigidbody가 있는 오브젝트에게)
            ApplyExplosionForce(explosionPoint);

            // 발사체 파괴
            DestroyProjectile();
        }

        /// <summary>
        /// 폭발 데미지 적용
        /// </summary>
        private void ApplyExplosionDamage(Vector3 explosionPoint)
        {
            if (weaponData == null) return;

            // 범위 내 Collider 찾기
            Collider[] colliders = Physics.OverlapSphere(explosionPoint, explosionRadius, explosionLayers);
            HashSet<IDamageable> hitDamageables = new HashSet<IDamageable>();

            foreach (Collider col in colliders)
            {
                IDamageable damageable = col.GetComponent<IDamageable>();
                if (damageable != null && !hitDamageables.Contains(damageable))
                {
                    hitDamageables.Add(damageable);

                    // 거리에 따른 데미지 감소
                    float distance = Vector3.Distance(explosionPoint, col.transform.position);
                    float damageMultiplier = 1f - (distance / explosionRadius);
                    damageMultiplier = Mathf.Clamp01(damageMultiplier);

                    // 폭발 중심은 데미지 증가
                    float finalDamage = weaponData.Damage * explosionDamageMultiplier * damageMultiplier;

                    // 방향 계산
                    Vector3 direction = (col.transform.position - explosionPoint).normalized;

                    // 데미지 정보 생성
                    DamageInfo damageInfo = new DamageInfo(
                        finalDamage,
                        DamageType.Explosion,
                        gameObject,
                        explosionPoint,
                        direction,
                        weaponData.KnockbackForce * damageMultiplier
                    );

                    // 데미지 적용
                    damageable.TakeDamage(damageInfo);

                    Debug.Log($"[EraserBomb] {col.name}에게 폭발 데미지 {finalDamage:F1} (거리: {distance:F1}m, 배율: {damageMultiplier:F2})");

                    OnHitEnemy?.Invoke(col.gameObject);
                }
            }

            Debug.Log($"[EraserBomb] 총 {hitDamageables.Count}개의 적에게 데미지");
        }

        /// <summary>
        /// 폭발 물리력 적용
        /// </summary>
        private void ApplyExplosionForce(Vector3 explosionPoint)
        {
            Collider[] colliders = Physics.OverlapSphere(explosionPoint, explosionRadius);
            foreach (Collider col in colliders)
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, explosionPoint, explosionRadius, 1f, ForceMode.Impulse);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showExplosionRadius) return;

            // 폭발 반경 표시
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, explosionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
