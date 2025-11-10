using UnityEngine;
using YajaGame.Gameplay.Combat;

namespace YajaGame.Gameplay.Projectiles
{
    /// <summary>
    /// 고무줄 슬링 발사체
    /// 특징: 빠른 원거리 공격, 적 슬로우 효과
    /// </summary>
    public class RubberBandProjectile : WeaponProjectileBase
    {
        [Header("Rubber Band Settings")]
        [SerializeField] private float slowDuration = 2f; // 슬로우 지속 시간
        [SerializeField] private float slowIntensity = 0.5f; // 슬로우 강도 (0.5 = 50% 감속)
        [SerializeField] private GameObject slowEffectPrefab; // 슬로우 이펙트
        [SerializeField] private Color slowTrailColor = new Color(0.5f, 0.8f, 1f, 0.5f); // 탄도 색상

        private TrailRenderer _trailRenderer;

        protected override void Awake()
        {
            base.Awake();

            // 충돌 시 즉시 파괴
            destroyOnHit = true;

            // TrailRenderer 설정
            _trailRenderer = GetComponent<TrailRenderer>();
            if (_trailRenderer != null)
            {
                _trailRenderer.startColor = slowTrailColor;
                _trailRenderer.endColor = new Color(slowTrailColor.r, slowTrailColor.g, slowTrailColor.b, 0f);
            }
        }

        protected override void HandleEnemyHit(Collision collision, IDamageable damageable)
        {
            // 기본 데미지 적용
            base.HandleEnemyHit(collision, damageable);

            // 슬로우 효과 적용
            ApplySlowEffect(collision.gameObject);
        }

        /// <summary>
        /// 슬로우 효과 적용
        /// </summary>
        private void ApplySlowEffect(GameObject target)
        {
            // 슬로우 효과 컴포넌트 확인
            SlowEffect slowEffect = target.GetComponent<SlowEffect>();
            if (slowEffect == null)
            {
                // 없으면 추가
                slowEffect = target.AddComponent<SlowEffect>();
            }

            // 슬로우 적용
            slowEffect.ApplySlow(slowDuration, slowIntensity);

            // 슬로우 이펙트 생성
            if (slowEffectPrefab != null)
            {
                GameObject effect = Instantiate(slowEffectPrefab, target.transform.position, Quaternion.identity);
                effect.transform.SetParent(target.transform);
                Destroy(effect, slowDuration);
            }

            Debug.Log($"[RubberBand] {target.name}에게 슬로우 효과 적용 ({slowIntensity * 100}% 감속, {slowDuration}초)");
        }
    }

    /// <summary>
    /// 슬로우 효과 컴포넌트
    /// </summary>
    public class SlowEffect : MonoBehaviour
    {
        private float _originalSpeed = 1f;
        private float _slowEndTime = 0f;
        private bool _isSlowed = false;

        // NavMeshAgent 또는 다른 이동 컴포넌트
        private UnityEngine.AI.NavMeshAgent _navMeshAgent;
        private float _navMeshOriginalSpeed;

        private void Awake()
        {
            _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (_navMeshAgent != null)
            {
                _navMeshOriginalSpeed = _navMeshAgent.speed;
            }
        }

        /// <summary>
        /// 슬로우 적용
        /// </summary>
        public void ApplySlow(float duration, float intensity)
        {
            if (!_isSlowed)
            {
                _isSlowed = true;

                // NavMeshAgent 속도 감소
                if (_navMeshAgent != null)
                {
                    _navMeshAgent.speed = _navMeshOriginalSpeed * (1f - intensity);
                }
            }

            // 슬로우 시간 연장 (중복 적용 시)
            _slowEndTime = Mathf.Max(_slowEndTime, Time.time + duration);

            Debug.Log($"[SlowEffect] 슬로우 적용: 강도={intensity}, 지속={duration}초");
        }

        private void Update()
        {
            if (_isSlowed && Time.time >= _slowEndTime)
            {
                RemoveSlow();
            }
        }

        /// <summary>
        /// 슬로우 제거
        /// </summary>
        private void RemoveSlow()
        {
            _isSlowed = false;

            // NavMeshAgent 속도 복원
            if (_navMeshAgent != null)
            {
                _navMeshAgent.speed = _navMeshOriginalSpeed;
            }

            Debug.Log("[SlowEffect] 슬로우 제거");

            // 컴포넌트 제거
            Destroy(this);
        }

        private void OnDestroy()
        {
            // 파괴 시 속도 복원
            if (_isSlowed && _navMeshAgent != null)
            {
                _navMeshAgent.speed = _navMeshOriginalSpeed;
            }
        }
    }
}
