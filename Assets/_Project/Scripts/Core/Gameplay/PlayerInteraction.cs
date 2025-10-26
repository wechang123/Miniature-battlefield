using UnityEngine;
using UnityEngine.Events;
using StarterAssets;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 플레이어의 아이템 감지 및 줍기 기능을 담당하는 컴포넌트
    /// </summary>
    [RequireComponent(typeof(StarterAssetsInputs))]
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask itemLayer = -1;
        [SerializeField] private float scanInterval = 0.2f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private float pickupAnimationDelay = 0.15f; // 줍기 애니메이션 타이밍 (매우 짧게!)
        [SerializeField] private float pickupAnimationSpeed = 5f; // 줍기 애니메이션 속도 (1 = 원래 속도, 5 = 5배속)
        [SerializeField] private bool disableMovementDuringPickup = true; // 줍는 중 움직임 비활성화

        [Header("Highlight Settings")]
        [SerializeField] private bool enableHighlight = true;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float highlightIntensity = 1.5f;

        [Header("Events")]
        public UnityEvent<IPickupable> OnItemInRange;
        public UnityEvent OnItemOutOfRange;
        public UnityEvent<IPickupable> OnItemPickedUp;

        private StarterAssetsInputs _input;
        private ItemCarrySystem _carrySystem;
        private IPickupable _nearestItem;
        private float _scanTimer;
        private Material _highlightedMaterial;
        private Renderer _highlightedRenderer;
        private Material _originalMaterial;
        private bool _isPickingUp = false;

        private void Awake()
        {
            _input = GetComponent<StarterAssetsInputs>();
            _carrySystem = GetComponent<ItemCarrySystem>();

            // Animator 자동 찾기
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                {
                    Debug.LogWarning("[PlayerInteraction] Animator를 찾을 수 없습니다!");
                }
            }
        }

        private void Update()
        {
            // 주기적으로 주변 아이템 스캔
            _scanTimer += Time.deltaTime;
            if (_scanTimer >= scanInterval)
            {
                _scanTimer = 0f;
                ScanForItems();
            }

            // E키 입력 처리
            if (_input.pickup)
            {
                TryPickup();
                _input.pickup = false; // 입력 소모
            }
        }

        /// <summary>
        /// 주변 아이템 스캔
        /// </summary>
        private void ScanForItems()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange, itemLayer);

            IPickupable closestItem = null;
            float closestDistance = float.MaxValue;

            foreach (var col in hitColliders)
            {
                IPickupable pickupable = col.GetComponent<IPickupable>();
                if (pickupable != null && pickupable.IsPickable)
                {
                    float distance = Vector3.Distance(transform.position, pickupable.Transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestItem = pickupable;
                    }
                }
            }

            // 가장 가까운 아이템이 변경되었을 때
            if (closestItem != _nearestItem)
            {
                // 이전 하이라이트 제거
                RemoveHighlight();

                _nearestItem = closestItem;

                if (_nearestItem != null)
                {
                    // 새로운 아이템 하이라이트
                    ApplyHighlight(_nearestItem);
                    OnItemInRange?.Invoke(_nearestItem);
                }
                else
                {
                    OnItemOutOfRange?.Invoke();
                }
            }
        }

        /// <summary>
        /// 아이템 줍기 시도
        /// </summary>
        private void TryPickup()
        {
            // 이미 아이템을 들고 있으면 줍기 불가
            if (_carrySystem != null && _carrySystem.IsCarryingItem)
            {
                Debug.Log("[PlayerInteraction] 이미 아이템을 들고 있습니다!");
                return;
            }

            if (_isPickingUp)
            {
                Debug.Log("[PlayerInteraction] 이미 줍는 중입니다!");
                return;
            }

            if (_nearestItem != null && _nearestItem.IsPickable)
            {
                // 거리 재확인
                float distance = Vector3.Distance(transform.position, _nearestItem.Transform.position);
                if (distance <= interactionRange)
                {
                    // 코루틴 시작
                    StartCoroutine(PickupWithAnimation());
                }
            }
        }

        /// <summary>
        /// 애니메이션과 함께 줍기 실행
        /// </summary>
        private System.Collections.IEnumerator PickupWithAnimation()
        {
            _isPickingUp = true;

            // 줍는 중 움직임 비활성화
            StarterAssetsInputs inputScript = null;
            if (disableMovementDuringPickup)
            {
                inputScript = GetComponent<StarterAssetsInputs>();

                if (inputScript != null)
                {
                    // 모든 입력 초기화
                    inputScript.move = Vector2.zero;
                    inputScript.look = Vector2.zero;
                    inputScript.jump = false;
                    inputScript.sprint = false;
                }

                Debug.Log("[PlayerInteraction] 움직임 비활성화!");
            }

            // 줍기 애니메이션 트리거
            if (animator != null)
            {
                // 애니메이션 속도 설정
                animator.speed = pickupAnimationSpeed;
                animator.SetTrigger("Pickup");
                Debug.Log($"[PlayerInteraction] 줍기 애니메이션 트리거! (속도: {pickupAnimationSpeed}x)");

                // 애니메이션 타이밍까지 대기 (속도에 맞춰 조정)
                float waitTime = pickupAnimationDelay / pickupAnimationSpeed;
                float elapsedTime = 0f;

                while (elapsedTime < waitTime)
                {
                    // 애니메이션 도중 계속 입력 차단
                    if (disableMovementDuringPickup && inputScript != null)
                    {
                        inputScript.move = Vector2.zero;
                        inputScript.look = Vector2.zero;
                        inputScript.jump = false;
                        inputScript.sprint = false;
                    }

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // 애니메이션 속도 원래대로
                animator.speed = 1f;
            }

            // 실제 줍기 실행
            PerformPickup();

            // 움직임 다시 활성화 (입력은 자동으로 다시 받아짐)
            if (disableMovementDuringPickup)
            {
                Debug.Log("[PlayerInteraction] 움직임 활성화!");
            }

            _isPickingUp = false;
        }

        /// <summary>
        /// 실제 줍기 실행
        /// </summary>
        private void PerformPickup()
        {
            if (_nearestItem == null || !_nearestItem.IsPickable)
            {
                Debug.Log("[PlayerInteraction] 줍을 아이템이 없습니다!");
                return;
            }

            OnItemPickedUp?.Invoke(_nearestItem);

            // ItemCarrySystem으로 아이템 들기
            ItemBase itemBase = _nearestItem.Transform.GetComponent<ItemBase>();
            if (_carrySystem != null && itemBase != null)
            {
                bool success = _carrySystem.PickupItem(itemBase);
                if (!success)
                {
                    // 들기 실패
                    return;
                }
            }
            else
            {
                // CarrySystem이 없으면 기존 방식 (OnPickup 호출)
                _nearestItem.OnPickup();
            }

            // 하이라이트 제거
            RemoveHighlight();
            _nearestItem = null;
            OnItemOutOfRange?.Invoke();
        }

        /// <summary>
        /// 아이템에 하이라이트 적용
        /// </summary>
        private void ApplyHighlight(IPickupable item)
        {
            if (!enableHighlight) return;

            _highlightedRenderer = item.Transform.GetComponentInChildren<Renderer>();
            if (_highlightedRenderer != null)
            {
                _originalMaterial = _highlightedRenderer.material;
                _highlightedMaterial = new Material(_originalMaterial);
                _highlightedMaterial.color = highlightColor;

                // Emission 설정
                _highlightedMaterial.EnableKeyword("_EMISSION");
                _highlightedMaterial.SetColor("_EmissionColor", highlightColor * highlightIntensity);

                _highlightedRenderer.material = _highlightedMaterial;
            }
        }

        /// <summary>
        /// 하이라이트 제거
        /// </summary>
        private void RemoveHighlight()
        {
            if (_highlightedRenderer != null && _originalMaterial != null)
            {
                _highlightedRenderer.material = _originalMaterial;
                if (_highlightedMaterial != null)
                {
                    Destroy(_highlightedMaterial);
                }
                _highlightedRenderer = null;
                _highlightedMaterial = null;
                _originalMaterial = null;
            }
        }

        /// <summary>
        /// 현재 근처에 있는 아이템 반환
        /// </summary>
        public IPickupable GetNearestItem()
        {
            return _nearestItem;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }

        private void OnDisable()
        {
            RemoveHighlight();
        }
    }
}
