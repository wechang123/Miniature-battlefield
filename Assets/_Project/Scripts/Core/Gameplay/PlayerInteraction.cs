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

        private void Awake()
        {
            _input = GetComponent<StarterAssetsInputs>();
            _carrySystem = GetComponent<ItemCarrySystem>();
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

            if (_nearestItem != null && _nearestItem.IsPickable)
            {
                // 거리 재확인
                float distance = Vector3.Distance(transform.position, _nearestItem.Transform.position);
                if (distance <= interactionRange)
                {
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
            }
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
