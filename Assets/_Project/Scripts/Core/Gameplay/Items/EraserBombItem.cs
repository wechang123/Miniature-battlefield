using UnityEngine;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 지우개폭탄 아이템 (맵에서 주울 수 있는)
    /// </summary>
    public class EraserBombItem : ItemBase
    {
        [Header("Eraser Bomb Settings")]
        [SerializeField] private int bombCount = 1; // 주우면 얻는 개수

        private void Start()
        {
            itemName = "지우개폭탄";
            itemType = ItemType.WeaponPart;
        }

        protected override void ProcessPickup()
        {
            // InventoryManager 싱글톤 사용
            if (InventoryManager.Instance != null)
            {
                // 지우개폭탄을 인벤토리에 추가
                InventoryManager.Instance.AddWeaponPart(WeaponPartType.EraserBomb, bombCount);
                Debug.Log($"[EraserBombItem] 지우개폭탄 {bombCount}개 획득!");
            }
            else
            {
                Debug.LogWarning("[EraserBombItem] InventoryManager 인스턴스를 찾을 수 없습니다!");
            }

            // 효과음/이펙트는 ItemBase.OnPickup()에서 자동 처리됨
        }

        public int GetBombCount()
        {
            return bombCount;
        }

        /// <summary>
        /// 플레이어가 아이템을 들었을 때 호출
        /// </summary>
        public void OnEquipped()
        {
            // AnimateItem() 중지 (손에 제대로 붙게)
            isPickable = false;
            Debug.Log($"[EraserBombItem] {itemName} 장착됨 - 애니메이션 중지");
        }

        /// <summary>
        /// 플레이어가 아이템을 버렸을 때 호출
        /// </summary>
        public void OnUnequipped()
        {
            // AnimateItem() 재개 (바닥에서 다시 애니메이션)
            isPickable = true;
            Debug.Log($"[EraserBombItem] {itemName} 해제됨 - 애니메이션 재개");
        }
    }
}
