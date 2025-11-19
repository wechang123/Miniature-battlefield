using UnityEngine;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 무기 부품 아이템 (연필창, 지우개폭탄, 고무줄 슬링 부품)
    /// </summary>
    public class WeaponPartItem : ItemBase
    {
        [Header("Weapon Part Settings")]
        [SerializeField] private WeaponPartType weaponPartType;

        public WeaponPartType PartType => weaponPartType;

        protected override void Awake()
        {
            base.Awake();
            itemType = ItemType.WeaponPart;
        }

        protected override void ProcessPickup()
        {
            // InventoryManager에 부품 추가
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddWeaponPart(weaponPartType, itemValue);
                Debug.Log($"[WeaponPartItem] {weaponPartType} 부품 {itemValue}개 획득!");
            }
            else
            {
                Debug.LogWarning("[WeaponPartItem] InventoryManager를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 플레이어가 아이템을 들었을 때 호출
        /// </summary>
        public void OnEquipped()
        {
            // AnimateItem() 중지 (손에 제대로 붙게)
            isPickable = false;
            Debug.Log($"[WeaponPartItem] {weaponPartType} 장착됨 - 애니메이션 중지");
        }

        /// <summary>
        /// 플레이어가 아이템을 버렸을 때 호출
        /// </summary>
        public void OnUnequipped()
        {
            // AnimateItem() 재개 (바닥에서 다시 애니메이션)
            isPickable = true;
            Debug.Log($"[WeaponPartItem] {weaponPartType} 해제됨 - 애니메이션 재개");
        }
    }
}
