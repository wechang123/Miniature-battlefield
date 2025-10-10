using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// 아이템 줍기 시스템
    /// 플레이어가 아이템을 줍고 들고 다닐 수 있게 함
    /// </summary>
    public class ItemPickupSystem : MonoBehaviour
    {
        [Header("Pickup Settings")]
        [Tooltip("아이템을 감지할 범위")]
        public float pickupRange = 2.5f;

        [Tooltip("들고 있는 아이템의 위치 (플레이어 앞)")]
        public Transform holdPosition;

        [Tooltip("카메라 참조 (던지기 방향 계산용)")]
        public Transform cameraTransform;

        [Tooltip("줍기 가능한 레이어")]
        public LayerMask pickableLayer;

        [Header("Hold Settings")]
        [Tooltip("아이템을 부드럽게 따라오게 할 속도")]
        public float holdSmoothSpeed = 15f;

        [Tooltip("아이템 회전 고정")]
        public bool lockRotation = false;

        [Header("Animation")]
        [Tooltip("애니메이터 사용 여부")]
        public bool useAnimator = false;

        // 상태
        private PickableObject _currentItem;
        private PickableObject _nearestItem;
        private Animator _animator;
        private StarterAssetsInputs _input;

        // 애니메이션 파라미터 IDs
        private int _animIDPickup;
        private int _animIDIsHolding;

        public bool IsHoldingItem => _currentItem != null;
        public PickableObject CurrentItem => _currentItem;

        private void Start()
        {
            _input = GetComponent<StarterAssetsInputs>();

            // 애니메이터 설정
            if (useAnimator && TryGetComponent(out _animator))
            {
                _animIDPickup = Animator.StringToHash("Pickup");
                _animIDIsHolding = Animator.StringToHash("IsHolding");
            }

            // holdPosition이 설정되지 않았다면 자동 생성
            if (holdPosition == null)
            {
                GameObject holdPoint = new GameObject("HoldPosition");
                holdPoint.transform.SetParent(transform);
                holdPoint.transform.localPosition = new Vector3(0.5f, 1.2f, 0.8f); // 캐릭터 앞쪽
                holdPosition = holdPoint.transform;
            }

            // 카메라 자동 찾기
            if (cameraTransform == null)
            {
                GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                if (mainCamera != null)
                {
                    cameraTransform = mainCamera.transform;
                }
            }
        }

        private void Update()
        {
            // 가장 가까운 아이템 감지 및 하이라이트
            DetectNearestItem();

            // 줍기 입력 처리
            if (_input.pickup)
            {
                _input.pickup = false; // 입력 소비

                if (IsHoldingItem)
                {
                    DropItem();
                }
                else if (_nearestItem != null)
                {
                    PickupItem(_nearestItem);
                }
            }

            // 아이템을 들고 있으면 위치 업데이트
            if (IsHoldingItem)
            {
                UpdateHeldItemPosition();
            }
        }

        /// <summary>
        /// 범위 내 가장 가까운 줍기 가능한 아이템 감지
        /// </summary>
        private void DetectNearestItem()
        {
            // 이미 들고 있으면 감지 안 함
            if (IsHoldingItem)
            {
                if (_nearestItem != null)
                {
                    _nearestItem.RemoveHighlight();
                    _nearestItem = null;
                }
                return;
            }

            // 범위 내 모든 줍기 가능한 오브젝트 찾기
            Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange, pickableLayer);

            PickableObject closestItem = null;
            float closestDistance = float.MaxValue;

            foreach (Collider col in colliders)
            {
                PickableObject item = col.GetComponent<PickableObject>();

                if (item != null && item.canBePickedUp && !item.IsBeingHeld)
                {
                    float distance = Vector3.Distance(transform.position, item.transform.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestItem = item;
                    }
                }
            }

            // 이전 하이라이트 제거
            if (_nearestItem != null && _nearestItem != closestItem)
            {
                _nearestItem.RemoveHighlight();
            }

            // 새 아이템 하이라이트
            _nearestItem = closestItem;
            if (_nearestItem != null)
            {
                _nearestItem.Highlight();
            }
        }

        /// <summary>
        /// 아이템 줍기
        /// </summary>
        private void PickupItem(PickableObject item)
        {
            if (item == null || IsHoldingItem) return;

            _currentItem = item;
            _currentItem.OnPickup();

            // 아이템을 holdPosition의 자식으로 설정
            _currentItem.transform.SetParent(holdPosition);

            // 애니메이션 트리거
            if (useAnimator && _animator != null)
            {
                _animator.SetTrigger(_animIDPickup);
                _animator.SetBool(_animIDIsHolding, true);
            }

            Debug.Log($"Picked up: {_currentItem.itemName}");
        }

        /// <summary>
        /// 아이템 내려놓기
        /// </summary>
        public void DropItem()
        {
            if (!IsHoldingItem) return;

            // 부모 관계 해제
            _currentItem.transform.SetParent(null);

            _currentItem.OnDrop();

            // 애니메이션 업데이트
            if (useAnimator && _animator != null)
            {
                _animator.SetBool(_animIDIsHolding, false);
            }

            Debug.Log($"Dropped: {_currentItem.itemName}");

            _currentItem = null;
        }

        /// <summary>
        /// 들고 있는 아이템 위치 업데이트
        /// </summary>
        private void UpdateHeldItemPosition()
        {
            if (!IsHoldingItem) return;

            // 부드럽게 holdPosition으로 이동
            _currentItem.transform.localPosition = Vector3.Lerp(
                _currentItem.transform.localPosition,
                Vector3.zero,
                Time.deltaTime * holdSmoothSpeed
            );

            // 회전 고정 옵션
            if (lockRotation)
            {
                _currentItem.transform.localRotation = Quaternion.Lerp(
                    _currentItem.transform.localRotation,
                    Quaternion.identity,
                    Time.deltaTime * holdSmoothSpeed
                );
            }
        }

        /// <summary>
        /// 디버그용 기즈모
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // 줍기 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRange);

            // holdPosition 표시
            if (holdPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(holdPosition.position, 0.1f);
            }
        }
    }
}
