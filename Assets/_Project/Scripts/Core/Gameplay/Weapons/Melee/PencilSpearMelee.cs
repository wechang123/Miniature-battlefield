using UnityEngine;
using YajaGame.Gameplay.Combat;

namespace YajaGame.Gameplay.Weapons.Melee
{
    /// <summary>
    /// 연필창 근접 무기
    /// 특징: 빠른 공격 속도, 관통 공격 (한 번에 여러 적 타격)
    /// </summary>
    public class PencilSpearMelee : MeleeWeaponBase
    {
        [Header("Pencil Spear Settings")]
        [SerializeField] private int maxHitsPerAttack = 3; // 한 번의 공격으로 최대 타격 가능한 적 수
        [SerializeField] private float damageMultiplierPerHit = 0.9f; // 타격마다 데미지 감소율 (90%)
        [SerializeField] private bool showTrailEffect = true; // 휘두르기 궤적 표시

        [Header("Visual Effects")]
        [SerializeField] private TrailRenderer trailRenderer; // 휘두르기 궤적

        private int _currentHitCount = 0;

        protected override void Awake()
        {
            base.Awake();

            // TrailRenderer 자동 찾기
            if (trailRenderer == null)
            {
                trailRenderer = GetComponentInChildren<TrailRenderer>();
            }

            // 시작 시 Trail 비활성화
            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
            }

            // 연필창 기본 공격 속도 (빠름)
            if (attackCooldown == 0.5f) // 기본값이면
            {
                attackCooldown = 0.4f; // 조금 더 빠르게
            }
        }

        protected override void StartAttack()
        {
            _currentHitCount = 0; // 타격 카운트 초기화
            base.StartAttack();

            // TrailRenderer 활성화
            if (trailRenderer != null && showTrailEffect)
            {
                trailRenderer.emitting = true;
                trailRenderer.Clear(); // 이전 궤적 제거
            }
        }

        protected override void EndAttack()
        {
            base.EndAttack();

            // TrailRenderer 비활성화
            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
            }

            Debug.Log($"[PencilSpear] 공격 완료. 총 {_currentHitCount}명 타격");
        }

        public override void OnHitTarget(GameObject target, IDamageable damageable)
        {
            if (weaponData == null) return;

            // 최대 타격 수 체크
            if (_currentHitCount >= maxHitsPerAttack)
            {
                Debug.Log($"[PencilSpear] 최대 타격 수({maxHitsPerAttack}) 도달. 더 이상 타격 불가");
                return;
            }

            // 타격 횟수에 따른 데미지 감소
            float damageMultiplier = Mathf.Pow(damageMultiplierPerHit, _currentHitCount);
            float finalDamage = weaponData.Damage * damageMultiplier;

            // 데미지 정보 생성
            Vector3 hitDirection = (target.transform.position - transform.position).normalized;
            DamageInfo damageInfo = new DamageInfo(
                finalDamage,
                weaponData.DamageType,
                gameObject,
                target.transform.position,
                hitDirection,
                weaponData.KnockbackForce * damageMultiplier // 넉백도 감소
            );

            // 데미지 적용
            damageable.TakeDamage(damageInfo);

            // 히트 이펙트
            if (weaponData.HitEffectPrefab != null)
            {
                Instantiate(weaponData.HitEffectPrefab, target.transform.position, Quaternion.identity);
            }

            // 히트 사운드
            if (weaponData.FireSound != null)
            {
                AudioSource.PlayClipAtPoint(weaponData.FireSound, target.transform.position, 0.5f);
            }

            OnHitEnemy?.Invoke(target);

            _currentHitCount++;

            Debug.Log($"[PencilSpear] {target.name}에게 {finalDamage:F1} 데미지! (타격 {_currentHitCount}/{maxHitsPerAttack}, 배율 {damageMultiplier:F2})");
        }

        public override void ActivateHitbox()
        {
            base.ActivateHitbox();

            // Trail 시작
            if (trailRenderer != null && showTrailEffect)
            {
                trailRenderer.Clear();
            }
        }

        private void OnDrawGizmos()
        {
            // 공격 범위 표시
            if (_isAttacking)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 1.5f);

                // 타격 수 표시
                Gizmos.color = Color.white;
                Vector3 textPos = transform.position + Vector3.up * 2f;
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(textPos, $"Hits: {_currentHitCount}/{maxHitsPerAttack}");
                #endif
            }
        }
    }
}
