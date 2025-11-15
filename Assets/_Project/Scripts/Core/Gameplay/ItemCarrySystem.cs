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
                    Debug.Log($"[ItemCarrySystem] CarryPosition 찾음: {found.name} (위치: {found.position})");
                }
                else
                {
                    // 못 찾으면 플레이어 자식으로 새로 생성
                    GameObject carryPosGO = new GameObject(carryPositionName);
                    carryPosGO.transform.SetParent(transform);
                    carryPosGO.transform.localPosition = new Vector3(0.5f, 1.5f, 0.5f); // 오른쪽 위
                    carryPosGO.transform.localRotation = Quaternion.identity;
                    carryPosition = carryPosGO.transform;
                    Debug.LogWarning($"[ItemCarrySystem] CarryPosition을 찾지 못해 자동 생성했습니다. 위치: {carryPosGO.transform.localPosition} (권장: Animator의 RightHand 본으로 설정하세요)");
                }
            }

            if (carryPosition == null)
            {
                Debug.LogError("[ItemCarrySystem] carryPosition이 설정되지 않았습니다!");
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
            if (item == null)
            {
                Debug.LogWarning("[ItemCarrySystem] 들 아이템이 null입니다!");
                return false;
            }

            // 무기(WeaponPartItem 또는 MeleeWeaponItem) 확인
            WeaponPartItem newWeaponPart = item.GetComponent<WeaponPartItem>();
            MeleeWeaponItem newMeleeWeapon = item.GetComponent<MeleeWeaponItem>();
            bool isNewItemWeapon = (newWeaponPart != null) || (newMeleeWeapon != null);

            // 이미 아이템을 들고 있는 경우
            if (IsCarryingItem)
            {
                WeaponPartItem currentWeaponPart = _currentItem.GetComponent<WeaponPartItem>();
                MeleeWeaponItem currentMeleeWeapon = _currentItem.GetComponent<MeleeWeaponItem>();
                bool isCurrentItemWeapon = (currentWeaponPart != null) || (currentMeleeWeapon != null);

                // 새 아이템이 무기인 경우
                if (isNewItemWeapon)
                {
                    // 현재도 무기를 들고 있는 경우: 자동 교체하지 않고 경고
                    if (isCurrentItemWeapon)
                    {
                        Debug.LogWarning($"[ItemCarrySystem] 이미 무기를 들고 있습니다! F키를 눌러 '{_currentItem.ItemName}'을(를) 먼저 버려주세요.");
                        return false;
                    }
                    // 작은 아이템을 들고 있는 경우도 거부
                    else
                    {
                        Debug.LogWarning($"[ItemCarrySystem] 이미 '{_currentItem.ItemName}'을(를) 들고 있습니다! F키를 눌러 먼저 버려주세요.");
                        return false;
                    }
                }
                // 새 아이템이 작은 아이템인 경우
                else
                {
                    Debug.LogWarning("[ItemCarrySystem] 이미 아이템을 들고 있습니다!");
                    return false;
                }
            }

            _currentItem = item;
            _originalItemScale = item.transform.localScale;

            Debug.Log($"[ItemCarrySystem] 아이템 원본 스케일: {_originalItemScale}");

            // 아이템을 CarryPosition의 자식으로 설정
            item.transform.SetParent(carryPosition);

            // 아이템별 커스텀 설정 확인 및 적용
            if (item.UseCustomCarrySettings)
            {
                item.transform.localPosition = item.CustomCarryOffset;
                item.transform.localRotation = Quaternion.Euler(item.CustomCarryRotation);
                item.transform.localScale = _originalItemScale * item.CustomCarryScale;
                Debug.Log($"[ItemCarrySystem] 커스텀 Carry 설정 사용: Offset={item.CustomCarryOffset}, Scale={item.CustomCarryScale}");
            }
            else
            {
                item.transform.localPosition = carryOffset;
                item.transform.localRotation = Quaternion.Euler(carryRotation);
                item.transform.localScale = _originalItemScale * carryScale;
                Debug.Log($"[ItemCarrySystem] 기본 Carry 설정 사용");
            }

            Debug.Log($"[ItemCarrySystem] 설정된 위치: {item.transform.localPosition}, 스케일: {item.transform.localScale}");
            Debug.Log($"[ItemCarrySystem] 월드 위치: {item.transform.position}");

            // 렌더러 상태 확인
            Renderer[] renderers = item.GetComponentsInChildren<Renderer>();
            Debug.Log($"[ItemCarrySystem] 렌더러 개수: {renderers.Length}");
            foreach (var renderer in renderers)
            {
                Debug.Log($"[ItemCarrySystem] 렌더러: {renderer.name}, enabled={renderer.enabled}, active={renderer.gameObject.activeSelf}");
            }

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

            // 무기 아이템인 경우 무기 해제 처리
            MeleeWeaponItem meleeWeapon = droppedItem.GetComponent<MeleeWeaponItem>();
            if (meleeWeapon != null)
            {
                meleeWeapon.OnUnequipped();
            }

            // 아이템 애니메이션 다시 활성화
            droppedItem.enabled = true;

            // 플레이어 앞 바닥에 부드럽게 떨어뜨리기 (플레이어 forward 방향 사용)
            Vector3 dropPosition = transform.position + transform.forward * 1.5f;
            dropPosition.y = transform.position.y; // 같은 높이에 놓기
            droppedItem.transform.position = dropPosition;

            // Rigidbody가 있으면 약간의 힘을 가해서 자연스럽게 떨어지도록
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * 2f + Vector3.down * 1f, ForceMode.Impulse);
            }

            Debug.Log($"[ItemCarrySystem] {droppedItem.ItemName} F키로 버림! 위치: {dropPosition}");
        }

        /// <summary>
        /// 현재 무기를 바닥에 던지기 (무기 교체용)
        /// </summary>
        private void DropCurrentItem()
        {
            if (!IsCarryingItem) return;

            ItemBase droppedItem = ReleaseItem();

            // 아이템 애니메이션 다시 활성화
            droppedItem.enabled = true;

            // 플레이어 앞쪽에 떨어뜨리기
            Vector3 dropPosition = transform.position + transform.forward * 1.5f;
            droppedItem.transform.position = dropPosition;

            // 약간 앞으로 던지기
            Rigidbody itemRb = droppedItem.GetComponent<Rigidbody>();
            if (itemRb != null)
            {
                itemRb.AddForce(transform.forward * 3f, ForceMode.Impulse);
            }

            Debug.Log($"[ItemCarrySystem] {droppedItem.ItemName} 바닥에 떨어뜨림!");
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

