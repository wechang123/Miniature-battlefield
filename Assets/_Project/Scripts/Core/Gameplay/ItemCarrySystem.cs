using UnityEngine;
using UnityEngine.Events;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// �÷��̾ �������� ��� �ٴϴ� �ý���
    /// </summary>
    public class ItemCarrySystem : MonoBehaviour
    {
        [Header("Carry Settings")]
        [SerializeField] private Transform holdPoint; // �������� �� ��ġ
        [SerializeField] private Vector3 holdOffset = new Vector3(0.5f, 1f, 0.5f);
        [SerializeField] private bool autoCreateHoldPoint = true;
        
        [Header("Physics")]
        [SerializeField] private bool disablePhysicsWhileCarrying = true;
        
        [Header("Events")]
        public UnityEvent<ItemBase> OnItemPickedUp;
        public UnityEvent<ItemBase> OnItemReleased;
        
        private ItemBase currentItem;
        private Rigidbody itemRigidbody;
        private Collider itemCollider;
        
        public bool IsCarryingItem => currentItem != null;
        public ItemBase CurrentItem => currentItem;
        
        private void Awake()
        {
            if (holdPoint == null && autoCreateHoldPoint)
            {
                GameObject holdPointObj = new GameObject("HoldPoint");
                holdPoint = holdPointObj.transform;
                holdPoint.SetParent(transform);
                holdPoint.localPosition = holdOffset;
                Debug.Log("[ItemCarrySystem] HoldPoint �ڵ� ������");
            }
        }
        
        /// <summary>
        /// ������ �ݱ�
        /// </summary>
        public bool PickupItem(ItemBase item)
        {
            if (IsCarryingItem)
            {
                Debug.LogWarning("[ItemCarrySystem] �̹� �������� ��� �ֽ��ϴ�!");
                return false;
            }
            
            if (item == null)
            {
                Debug.LogError("[ItemCarrySystem] �������� null�Դϴ�!");
                return false;
            }
            
            currentItem = item;
            
            // ������ ��ġ �̵�
            currentItem.transform.SetParent(holdPoint);
            currentItem.transform.localPosition = Vector3.zero;
            currentItem.transform.localRotation = Quaternion.identity;
            
            // ���� ��Ȱ��ȭ
            itemRigidbody = currentItem.GetComponent<Rigidbody>();
            itemCollider = currentItem.GetComponent<Collider>();
            
            if (disablePhysicsWhileCarrying)
            {
                if (itemRigidbody != null)
                {
                    itemRigidbody.isKinematic = true;
                    itemRigidbody.useGravity = false;
                }
                
                if (itemCollider != null)
                {
                    itemCollider.enabled = false;
                }
            }
            
            currentItem.OnPickup();
            OnItemPickedUp?.Invoke(currentItem);
            
            Debug.Log($"[ItemCarrySystem] {currentItem.ItemName} �������� ������ϴ�!");
            return true;
        }
        
        /// <summary>
        /// ������ ��������
        /// </summary>
        public ItemBase ReleaseItem()
        {
            if (!IsCarryingItem)
            {
                Debug.LogWarning("[ItemCarrySystem] ��� �ִ� �������� �����ϴ�!");
                return null;
            }
            
            ItemBase releasedItem = currentItem;
            
            // �θ� ����
            releasedItem.transform.SetParent(null);
            
            // ���� ��Ȱ��ȭ
            if (itemRigidbody != null)
            {
                itemRigidbody.isKinematic = false;
                itemRigidbody.useGravity = true;
            }
            
            if (itemCollider != null)
            {
                itemCollider.enabled = true;
            }
            
            releasedItem.OnDrop();
            OnItemReleased?.Invoke(releasedItem);
            
            Debug.Log($"[ItemCarrySystem] {releasedItem.ItemName} �������� ���ҽ��ϴ�!");
            
            currentItem = null;
            itemRigidbody = null;
            itemCollider = null;
            
            return releasedItem;
        }
        
        /// <summary>
        /// ������ ������ ��� (����߸���)
        /// </summary>
        public void DropItem(Vector3 dropVelocity = default)
        {
            ItemBase droppedItem = ReleaseItem();
            
            if (droppedItem != null && itemRigidbody != null)
            {
                itemRigidbody.linearVelocity = dropVelocity;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (holdPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(holdPoint.position, 0.1f);
                Gizmos.DrawLine(transform.position, holdPoint.position);
            }
            else if (autoCreateHoldPoint)
            {
                Vector3 previewPos = transform.position + transform.TransformDirection(holdOffset);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(previewPos, 0.1f);
                Gizmos.DrawLine(transform.position, previewPos);
            }
        }
    }
}