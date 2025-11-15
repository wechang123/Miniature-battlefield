using UnityEngine;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 지우개 파편 아이템
    /// 지우개폭탄이 폭발할 때 생성되며, 고무줄슬링의 탄약으로 사용됨
    /// </summary>
    public class EraserFragmentItem : ItemBase
    {
        [Header("Fragment Settings")]
        [SerializeField] private int fragmentValue = 1; // 파편 개수 (기본 1개)

        private Rigidbody rb;
        private float spawnTime;

        protected override void Awake()
        {
            base.Awake();
            // 파편은 물리로 떨어져야 하므로 AnimateItem 비활성화
            isPickable = false;

            // Rigidbody 가져오기 및 설정 강제
            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.mass = 0.1f;
                rb.linearDamping = 0.5f;
                rb.constraints = RigidbodyConstraints.None; // 모든 제약 해제
                Debug.Log($"[Fragment] Rigidbody 초기화: gravity={rb.useGravity}, kinematic={rb.isKinematic}, constraints={rb.constraints}");
            }
            else
            {
                Debug.LogError("[Fragment] Rigidbody가 없습니다!");
            }

            // BoxCollider 임시 비활성화 (충돌 방지)
            BoxCollider boxCol = GetComponent<BoxCollider>();
            if (boxCol != null)
            {
                boxCol.enabled = false;
                Invoke(nameof(EnableBoxCollider), 0.1f); // 0.1초 후 활성화
            }

            spawnTime = Time.time;
        }

        private void EnableBoxCollider()
        {
            BoxCollider boxCol = GetComponent<BoxCollider>();
            if (boxCol != null)
            {
                boxCol.enabled = true;
                Debug.Log("[Fragment] BoxCollider 활성화");
            }
        }

        private void Start()
        {
            itemName = "지우개 파편";
            itemType = ItemType.Consumable; // 탄약은 소모품으로 분류
            itemValue = fragmentValue;

            // 5초 후 줍기 가능하게 (충분히 떨어진 후)
            Invoke(nameof(EnablePickup), 5f);
        }

        private void EnablePickup()
        {
            isPickable = true;
        }

        protected override void Update()
        {
            // 파편은 AnimateItem()을 실행하지 않음 (물리로만 동작)
            // base.Update()를 호출하지 않아서 AnimateItem() 방지
        }

        private void FixedUpdate()
        {
            // 처음 3초 동안만 디버그 로그
            if (Time.time - spawnTime < 3f && rb != null)
            {
                Debug.Log($"[Fragment] Y={transform.position.y:F2}, Velocity.Y={rb.linearVelocity.y:F2}, Gravity={rb.useGravity}, Kinematic={rb.isKinematic}");
            }
        }

        protected override void ProcessPickup()
        {
            // InventoryManager에 파편 개수 추가
            if (InventoryManager.Instance != null)
            {
                // 파편을 인벤토리에 추가 (고무줄슬링의 탄약으로 사용될 예정)
                // 임시로 지우개폭탄 카운트에 저장 (나중에 별도 탄약 시스템 추가 시 변경 가능)
                InventoryManager.Instance.AddWeaponPart(WeaponPartType.EraserBomb, fragmentValue);

                Debug.Log($"[EraserFragmentItem] 지우개 파편 {fragmentValue}개 획득! (고무줄슬링 탄약용)");
            }
            else
            {
                Debug.LogWarning("[EraserFragmentItem] InventoryManager 인스턴스를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 파편 개수 반환
        /// </summary>
        public int GetFragmentValue()
        {
            return fragmentValue;
        }

        /// <summary>
        /// 파편 개수 설정
        /// </summary>
        public void SetFragmentValue(int value)
        {
            fragmentValue = Mathf.Max(1, value);
            itemValue = fragmentValue;
        }
    }
}
