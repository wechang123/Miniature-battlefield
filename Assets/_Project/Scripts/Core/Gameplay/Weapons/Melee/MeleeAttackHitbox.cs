using UnityEngine;
using System.Collections.Generic;
using YajaGame.Gameplay.Combat;

namespace YajaGame.Gameplay.Weapons.Melee
{
    /// <summary>
    /// 근접 무기 공격 히트박스
    /// 공격 중에만 활성화되어 적과의 충돌을 감지
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MeleeAttackHitbox : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask targetLayers; // 공격 대상 레이어
        [SerializeField] private bool debugMode = true;

        private Collider _collider;
        private HashSet<GameObject> _hitTargets = new HashSet<GameObject>(); // 이번 공격에서 이미 맞은 대상들
        private MeleeWeaponBase _weapon;

        public bool IsActive { get; private set; } = false;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true; // 트리거로 설정
            _weapon = GetComponentInParent<MeleeWeaponBase>();

            // 시작 시 비활성화
            Deactivate();
        }

        /// <summary>
        /// 히트박스 활성화 (공격 시작)
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            _collider.enabled = true;
            _hitTargets.Clear(); // 이전 공격 기록 초기화

            if (debugMode)
            {
                Debug.Log($"[MeleeHitbox] 히트박스 활성화: {gameObject.name}");
            }
        }

        /// <summary>
        /// 히트박스 비활성화 (공격 종료)
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            _collider.enabled = false;

            if (debugMode && _hitTargets.Count > 0)
            {
                Debug.Log($"[MeleeHitbox] 히트박스 비활성화. 총 {_hitTargets.Count}명 타격");
            }

            _hitTargets.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsActive) return;

            // 레이어 체크
            if (((1 << other.gameObject.layer) & targetLayers) == 0)
            {
                return; // 공격 대상이 아님
            }

            // 이미 맞은 대상은 스킵 (한 번의 공격으로 여러 번 맞지 않도록)
            if (_hitTargets.Contains(other.gameObject))
            {
                return;
            }

            // 데미지를 받을 수 있는 대상인지 확인
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && _weapon != null)
            {
                _hitTargets.Add(other.gameObject);
                _weapon.OnHitTarget(other.gameObject, damageable);

                if (debugMode)
                {
                    Debug.Log($"[MeleeHitbox] {other.gameObject.name} 타격!");
                }
            }
        }

        private void OnDrawGizmos()
        {
            // 히트박스 시각화
            if (IsActive)
            {
                Gizmos.color = Color.red;
                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;

                    if (col is BoxCollider boxCol)
                    {
                        Gizmos.DrawWireCube(boxCol.center, boxCol.size);
                    }
                    else if (col is SphereCollider sphereCol)
                    {
                        Gizmos.DrawWireSphere(sphereCol.center, sphereCol.radius);
                    }
                }
            }
            else
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;

                    if (col is BoxCollider boxCol)
                    {
                        Gizmos.DrawWireCube(boxCol.center, boxCol.size);
                    }
                }
            }
        }
    }
}
