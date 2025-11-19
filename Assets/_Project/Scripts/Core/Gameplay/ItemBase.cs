using UnityEngine;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 아이템 타입 정의
    /// </summary>
    public enum ItemType
    {
        WeaponPart,      // 무기 부품 (연필창, 지우개, 고무줄 부품)
        Consumable,      // 소모품 (체력 회복 등)
        Currency         // 화폐/포인트
    }

    /// <summary>
    /// 무기 부품 타입
    /// </summary>
    public enum WeaponPartType
    {
        PencilSpear,     // 연필창 부품
        EraserBomb,      // 지우개폭탄 부품
        RubberBandSling  // 고무줄 슬링 부품
    }

    /// <summary>
    /// 모든 아이템의 기본 클래스
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class ItemBase : MonoBehaviour, IPickupable
    {
        [Header("Item Settings")]
        [SerializeField] protected string itemName = "Item";
        [SerializeField] protected ItemType itemType;
        [SerializeField] protected Sprite itemIcon;
        [SerializeField] protected int itemValue = 1;

        [Header("Pickup Settings")]
        [SerializeField] protected float pickupRange = 2f;
        [SerializeField] protected bool isPickable = true;
        [SerializeField] protected GameObject pickupEffect;
        [SerializeField] protected AudioClip pickupSound;

        [Header("Visual Feedback")]
        [SerializeField] protected float bobSpeed = 1f;
        [SerializeField] protected float bobHeight = 0.1f; // 떠있는 높이 (낮춤)
        [SerializeField] protected float rotationSpeed = 50f;

        [Header("Carry Settings (Optional Override)")]
        [SerializeField] protected bool useCustomCarrySettings = false;
        [SerializeField] protected Vector3 customCarryOffset = Vector3.zero;
        [SerializeField] protected Vector3 customCarryRotation = Vector3.zero;
        [SerializeField] protected float customCarryScale = 1f;

        protected Vector3 startPosition;
        protected Collider itemCollider;

        // IPickupable 구현
        public Transform Transform => transform;
        public bool IsPickable => isPickable;

        // 속성
        public string ItemName => itemName;
        public ItemType Type => itemType;
        public Sprite Icon => itemIcon;
        public int Value => itemValue;

        // Carry 설정 속성
        public bool UseCustomCarrySettings => useCustomCarrySettings;
        public Vector3 CustomCarryOffset => customCarryOffset;
        public Vector3 CustomCarryRotation => customCarryRotation;
        public float CustomCarryScale => customCarryScale;

        protected virtual void Awake()
        {
            itemCollider = GetComponent<Collider>();
            itemCollider.isTrigger = true;
            startPosition = transform.position;
        }

        protected virtual void Update()
        {
            if (isPickable)
            {
                AnimateItem();
            }
        }

        /// <summary>
        /// 아이템 애니메이션 (떠다니는 효과)
        /// </summary>
        protected virtual void AnimateItem()
        {
            // 상하 움직임
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // 회전
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 아이템이 수집될 때 호출 (IPickupable 구현)
        /// </summary>
        public virtual void OnPickup()
        {
            if (!isPickable) return;

            // 이펙트 재생
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // 사운드 재생
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // 수집 처리
            ProcessPickup();

            // 아이템 제거
            Destroy(gameObject);
        }

        /// <summary>
        /// 아이템별 고유 수집 처리 (서브클래스에서 구현)
        /// </summary>
        protected abstract void ProcessPickup();

        /// <summary>
        /// 플레이어와의 거리 체크
        /// </summary>
        public bool IsInRange(Vector3 playerPosition)
        {
            return Vector3.Distance(transform.position, playerPosition) <= pickupRange;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
        }
    }
}
