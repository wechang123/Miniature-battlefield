using UnityEngine;
using UnityEngine.Events;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 플레이어가 아이템을 들고 다니는 시스템
    /// 한 번에 하나의 아이템만 들 수 있음
    /// </summary>
    public class ItemCarrySystem : MonoBehaviour
    {
        [Header("Carry Settings")]
        [SerializeField] private Transform carryPosition;
        [SerializeField] private Vector3 carryOffset = Vector3.zero;
        [SerializeField] private Vector3 carryRotation = Vector3.zero;
        [SerializeField] private float carryScale = 1f;

        [Header("Auto Find Carry Position")]
        [SerializeField] private bool autoFindCarryPosition = true;
        [SerializeField] private string carryPositionName = "CarryPosition";

        [Header("Events")]
        public UnityEvent<ItemBase> OnItemPickedUp;
        public UnityEvent<ItemBase> OnItemDropped;

        private ItemBase _currentItem;
        private Vector3 _originalItemScale;

        private void Start()
        {
            // 자동으로 CarryPosition 찾기
            if (autoFindCarryPosition && carryPosition == null)
            {
                Transform found = FindCarryPosition(transform);
                if (found != null)
                {
                    carryPosition = found;
                }
                else
                {
                    // 못 찾으면 플레이어 자식으로 새로 생성
                    GameObject carryPosGO = new GameObject(carryPositionName);
                    carryPosGO.transform.SetParent(transform);
                    carryPosGO.transform.localPosition = new Vector3(0.5f, 1.5f, 0.5f); // 오른쪽 위
                    carryPosGO.transform.localRotation = Quaternion.identity;
                    carryPosition = carryPosGO.transform;
                    Debug.Log($"[ItemCarrySystem] CarryPosition 자동 생성: {carryPosGO.transform.localPosition}");
                }
            }
        }

        /// <summary>
        /// 재귀적으로 CarryPosition 찾기
        /// </summary>
        private Transform FindCarryPosition(Transform parent)
        {
            if (parent.name.Contains(carryPositionName))
            {
                return parent;
            }

            foreach (Transform child in parent)
            {
                Transform found = FindCarryPosition(child);
                if (found != null) return found;
            }

            return null;
        }

        /// <summary>
        /// 현재 아이템을 들고 있는지 확인
        /// </summary>
        public bool IsCarryingItem => _currentItem != null;

        /// <summary>
        /// 현재 들고 있는 아이템
        /// </summary>
        public ItemBase CurrentItem => _currentItem;

        /// <summary>
        /// 아이템을 들기
        /// </summary>
        public bool PickupItem(ItemBase item)
        {
            if (IsCarryingItem)
            {
                Debug.LogWarning("[ItemCarrySystem] 이미 아이템을 들고 있습니다!");
                return false;
            }

            if (item == null)
            {
                Debug.LogWarning("[ItemCarrySystem] 들 아이템이 null입니다!");
                return false;
            }

            _currentItem = item;
            _originalItemScale = item.transform.localScale;

            // 아이템을 CarryPosition의 자식으로 설정
            item.transform.SetParent(carryPosition);
            item.transform.localPosition = carryOffset;
            item.transform.localRotation = Quaternion.Euler(carryRotation);
            item.transform.localScale = _originalItemScale * carryScale;

            // 아이템의 물리/충돌 비활성화
            Collider itemCollider = item.GetComponent<Collider>();
            if (itemCollider != null)
            {
                itemCollider.enabled = false;
            }

            Rigidbody itemRb = item.GetComponent<Rigidbody>();
            if (itemRb != null)
            {
                itemRb.isKinematic = true;
            }

            // 아이템 애니메이션 비활성화
            item.enabled = false;

            OnItemPickedUp?.Invoke(item);
            Debug.Log($"[ItemCarrySystem] {item.ItemName} 들기 완료!");

            return true;
        }

        /// <summary>
        /// 들고 있는 아이템을 놓기 (던지기 전 준비)
        /// </summary>
        public ItemBase ReleaseItem()
        {
            if (!IsCarryingItem)
            {
                return null;
            }

            ItemBase releasedItem = _currentItem;

            // 부모 관계 해제
            releasedItem.transform.SetParent(null);

            // 원래 스케일 복원
            releasedItem.transform.localScale = _originalItemScale;

            // 물리/충돌 활성화
            Collider itemCollider = releasedItem.GetComponent<Collider>();
            if (itemCollider != null)
            {
                itemCollider.enabled = true;
            }

            Rigidbody itemRb = releasedItem.GetComponent<Rigidbody>();
            if (itemRb != null)
            {
                itemRb.isKinematic = false;
            }

            OnItemDropped?.Invoke(releasedItem);
            Debug.Log($"[ItemCarrySystem] {releasedItem.ItemName} 놓기 완료!");

            _currentItem = null;

            return releasedItem;
        }

        /// <summary>
        /// 강제로 아이템 드롭 (사망 등)
        /// </summary>
        public void ForceDropItem()
        {
            if (!IsCarryingItem) return;

            ItemBase droppedItem = ReleaseItem();

            // 아이템 애니메이션 다시 활성화
            droppedItem.enabled = true;

            // 바닥에 떨어뜨리기
            droppedItem.transform.position = transform.position + Vector3.forward;
        }

        private void OnDrawGizmosSelected()
        {
            if (carryPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(carryPosition.position, 0.1f);
                Gizmos.DrawLine(transform.position, carryPosition.position);
            }
        }
    }
}
