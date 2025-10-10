using UnityEngine;
using UnityEngine.Events;
using StarterAssets;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 들고 있는 아이템을 던지는 시스템
    /// </summary>
    [RequireComponent(typeof(ItemCarrySystem))]
    [RequireComponent(typeof(StarterAssetsInputs))]
    public class ItemThrowSystem : MonoBehaviour
    {
        [Header("Throw Settings")]
        [SerializeField] private float throwForce = 15f;
        [SerializeField] private float throwAngle = 30f; // 위로 던지는 각도
        [SerializeField] private bool useMouseAim = true;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private bool autoFindCamera = true;

        [Header("Throw Statistics")]
        [SerializeField] private int totalThrowCount = 0;

        [Header("Events")]
        public UnityEvent<ItemBase> OnItemThrown;

        private ItemCarrySystem _carrySystem;
        private StarterAssetsInputs _input;
        private TrajectoryPredictor _trajectoryPredictor;

        private void Awake()
        {
            _carrySystem = GetComponent<ItemCarrySystem>();
            _input = GetComponent<StarterAssetsInputs>();
            _trajectoryPredictor = GetComponent<TrajectoryPredictor>();

            if (autoFindCamera && cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
                if (cameraTransform == null)
                {
                    Debug.LogWarning("[ItemThrowSystem] 카메라를 찾을 수 없습니다!");
                }
            }
        }

        private void Update()
        {
            // 던지기 입력 처리
            if (_input.@throw)
            {
                TryThrowItem();
                _input.@throw = false; // 입력 소모
            }

            // 궤적 미리보기 업데이트
            if (_trajectoryPredictor != null && _carrySystem.IsCarryingItem)
            {
                Vector3 throwDirection = CalculateThrowDirection();
                Vector3 throwVelocity = throwDirection * throwForce;
                _trajectoryPredictor.ShowTrajectory(_carrySystem.CurrentItem.Transform.position, throwVelocity);
            }
            else if (_trajectoryPredictor != null)
            {
                _trajectoryPredictor.HideTrajectory();
            }
        }

        /// <summary>
        /// 아이템 던지기 시도
        /// </summary>
        private void TryThrowItem()
        {
            if (!_carrySystem.IsCarryingItem)
            {
                Debug.Log("[ItemThrowSystem] 들고 있는 아이템이 없습니다!");
                return;
            }

            // 아이템 놓기
            ItemBase itemToThrow = _carrySystem.ReleaseItem();

            // 던지는 방향 계산
            Vector3 throwDirection = CalculateThrowDirection();

            // ThrownProjectile 컴포넌트 추가 (없으면)
            ThrownProjectile projectile = itemToThrow.GetComponent<ThrownProjectile>();
            if (projectile == null)
            {
                projectile = itemToThrow.gameObject.AddComponent<ThrownProjectile>();
            }

            // 던지기
            projectile.Launch(throwDirection * throwForce);

            // 통계 업데이트
            totalThrowCount++;
            Debug.Log($"[ItemThrowSystem] {itemToThrow.ItemName} 던지기! (총 {totalThrowCount}회)");

            // InventoryManager에 통계 업데이트
            if (InventoryManager.Instance != null)
            {
                WeaponPartItem weaponPart = itemToThrow.GetComponent<WeaponPartItem>();
                if (weaponPart != null)
                {
                    InventoryManager.Instance.AddThrowCount(weaponPart.PartType, 1);
                }
            }

            OnItemThrown?.Invoke(itemToThrow);
        }

        /// <summary>
        /// 던지는 방향 계산
        /// </summary>
        private Vector3 CalculateThrowDirection()
        {
            Vector3 direction;

            if (useMouseAim && cameraTransform != null)
            {
                // 카메라가 바라보는 방향
                direction = cameraTransform.forward;
            }
            else
            {
                // 플레이어가 바라보는 방향
                direction = transform.forward;
            }

            // 위로 각도 추가 (포물선)
            direction = Quaternion.AngleAxis(-throwAngle, cameraTransform != null ? cameraTransform.right : transform.right) * direction;

            return direction.normalized;
        }

        /// <summary>
        /// 던지는 힘 설정
        /// </summary>
        public void SetThrowForce(float force)
        {
            throwForce = Mathf.Max(1f, force);
        }

        /// <summary>
        /// 던지는 각도 설정
        /// </summary>
        public void SetThrowAngle(float angle)
        {
            throwAngle = Mathf.Clamp(angle, 0f, 89f);
        }

        /// <summary>
        /// 총 던진 횟수 반환
        /// </summary>
        public int GetTotalThrowCount()
        {
            return totalThrowCount;
        }

        private void OnDrawGizmosSelected()
        {
            if (_carrySystem != null && _carrySystem.IsCarryingItem && cameraTransform != null)
            {
                // 던지는 방향 표시
                Vector3 throwDirection = CalculateThrowDirection();
                Vector3 startPos = _carrySystem.CurrentItem.Transform.position;
                Gizmos.color = Color.red;
                Gizmos.DrawRay(startPos, throwDirection * 3f);
            }
        }
    }
}
