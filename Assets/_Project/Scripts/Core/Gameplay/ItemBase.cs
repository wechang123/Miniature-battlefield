using UnityEngine;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 모든 아이템의 기본 클래스
    /// </summary>
    public class ItemBase : MonoBehaviour, IPickupable
    {
        [Header("Item Info")]
        [SerializeField] private string itemName = "Item";
        [SerializeField] private string itemDescription = "";
        
        [Header("Pickup Settings")]
        [SerializeField] private bool isPickable = true;
        
        [Header("Physics")]
        [SerializeField] private Rigidbody rb;
        
        public string ItemName => itemName;
        public string ItemDescription => itemDescription;
        public Transform Transform => transform;
        public bool IsPickable => isPickable;
        public Rigidbody Rigidbody => rb;
        
        protected virtual void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
        }
        
        public virtual void OnPickup()
        {
            Debug.Log($"[ItemBase] {itemName} 아이템이 주워졌습니다!");
            isPickable = false;
        }
        
        public virtual void OnDrop()
        {
            Debug.Log($"[ItemBase] {itemName} 아이템이 놓여졌습니다!");
            isPickable = true;
        }
        
        public void SetPickable(bool pickable)
        {
            isPickable = pickable;
        }
    }
}