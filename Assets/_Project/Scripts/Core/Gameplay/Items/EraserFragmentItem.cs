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

        private void Start()
        {
            itemName = "지우개 파편";
            itemType = ItemType.Consumable; // 탄약은 소모품으로 분류
            itemValue = fragmentValue;
        }

        protected override void ProcessPickup()
        {
            // InventoryManager에 파편 개수 추가
            if (InventoryManager.Instance != null)
            {
                // 파편을 인벤토리에 추가
                // TODO: 인벤토리에 파편 카운트 추가 (고무줄슬링 탄약으로 사용)
                Debug.Log($"[EraserFragmentItem] 지우개 파편 {fragmentValue}개 획득!");

                // 임시: 지우개폭탄 부품으로 저장 (나중에 고무줄슬링 탄약 시스템 구현 후 변경)
                // InventoryManager.Instance.AddWeaponPart(WeaponPartType.EraserBomb, fragmentValue);
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
