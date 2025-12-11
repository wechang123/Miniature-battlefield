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

        [Header("Fragment Settings")]
        [SerializeField] private GameObject fragmentPrefab; // 파편 프리팹
        [SerializeField] private int fragmentCount = 8; // 생성할 파편 개수
        [SerializeField] private float fragmentForce = 2f; // 파편이 튀는 힘 (5→2로 감소)
        [SerializeField] private float fragmentSpread = 0.5f; // 파편 퍼짐 정도 (1→0.5로 감소)

        private bool _hasExploded = false;

        protected override void Start()
        {
            base.Start(); // 부모 클래스에서 _spawnTime 설정

            // 폭발 시 파괴
            destroyOnHit = true;

            // Player와 충돌 무시 (던질 때 즉시 폭발 방지)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && _collider != null)
            {
                Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
                foreach (Collider playerCollider in playerColliders)
                {
                    Physics.IgnoreCollision(_collider, playerCollider, true);
                }
                Debug.Log($"[EraserBomb] Player와 충돌 무시 설정 완료 ({playerColliders.Length}개 collider)");
            }

            Debug.Log($"[EraserBomb] Start 호출! explodeOnImpact={explodeOnImpact}, Collider enabled={_collider?.enabled}, Rigidbody={_rigidbody != null}");
            Debug.Log($"[EraserBomb] Collider isTrigger={_collider?.isTrigger}, Layer={gameObject.layer}");
        }

        private void Update()
        {
            // 퓨즈 시간 체크 - 이미 폭발했거나 퓨즈가 없으면 체크하지 않음 (성능 최적화)
            if (fuseTime > 0 && !_hasExploded)
            {
                if (Time.time - _spawnTime >= fuseTime)
                {
                    Debug.Log("[EraserBomb] 퓨즈 시간 종료. 폭발!");
                    Explode(transform.position);
                }
            }
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"[EraserBomb] OnCollisionEnter 호출됨! 충돌 대상: {collision.gameObject.name}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");

            if (_hasExploded)
            {
                Debug.Log("[EraserBomb] 이미 폭발함. 무시.");
                return;
            }

            // 충돌 시 폭발
            if (explodeOnImpact)
            {
                Debug.Log("[EraserBomb] explodeOnImpact=true, 폭발 시작!");
                Vector3 explosionPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
                Explode(explosionPoint);
            }
            else
            {
                Debug.Log("[EraserBomb] explodeOnImpact=false, 기본 충돌 처리");
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

            // 파편 생성
            SpawnFragments(explosionPoint);

            // 발사체 파괴
            DestroyProjectile();
        }

        /// <summary>
        /// 파편 생성
        /// </summary>
        private void SpawnFragments(Vector3 explosionPoint)
        {
            if (fragmentPrefab == null || fragmentCount <= 0) return;

            Debug.Log($"[EraserBomb] 파편 {fragmentCount}개 생성!");

            for (int i = 0; i < fragmentCount; i++)
            {
                // 수평 방향 랜덤 계산
                Vector3 horizontalDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    0f,
                    Random.Range(-1f, 1f)
                ).normalized;

                // 위쪽으로 살짝 튀게 (자연스러운 폭발 효과)
                float upwardForce = Random.Range(0.3f, 0.7f);
                Vector3 direction = (horizontalDirection + Vector3.up * upwardForce).normalized;

                // 파편 생성 위치 (폭발 중심에서 약간 떨어진 곳)
                Vector3 spawnPosition = explosionPoint + direction * fragmentSpread;

                // 파편 생성
                GameObject fragment = Instantiate(fragmentPrefab, spawnPosition, Random.rotation);

                // Rigidbody에 힘 적용 (위로 약간 튀고 옆으로 퍼짐)
                Rigidbody rb = fragment.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 수평 방향 + 위로 튀는 힘
                    Vector3 force = horizontalDirection * fragmentForce + Vector3.up * (fragmentForce * 0.5f);
                    rb.AddForce(force, ForceMode.Impulse);

                    // 랜덤 회전 추가
                    rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
                }
            }
        }

        /// <summary>
        /// 폭발 데미지 적용
        /// </summary>
        private void ApplyExplosionDamage(Vector3 explosionPoint)
        {
            float baseDamage = weaponData != null ? weaponData.Damage : 10f; // 기본 데미지 10
            float knockbackForce = weaponData != null ? weaponData.KnockbackForce : 5f; // 기본 넉백 5

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
                    float finalDamage = baseDamage * explosionDamageMultiplier * damageMultiplier;

                    // 방향 계산
                    Vector3 direction = (col.transform.position - explosionPoint).normalized;

                    // 데미지 정보 생성
                    DamageInfo damageInfo = new DamageInfo(
                        finalDamage,
                        DamageType.Explosion,
                        gameObject,
                        explosionPoint,
                        direction,
                        knockbackForce * damageMultiplier
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
