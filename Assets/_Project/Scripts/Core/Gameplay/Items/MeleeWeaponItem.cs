using UnityEngine;
using YajaGame.Gameplay.Weapons.Melee;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 근접 무기 아이템 (연필창 등)
    /// 줍으면 들고 다니며 공격할 수 있는 아이템
    /// </summary>
    public class MeleeWeaponItem : ItemBase
    {
        [Header("Melee Weapon Settings")]
        [SerializeField] private MeleeWeaponBase weaponComponent; // 무기 컴포넌트

        protected override void Awake()
        {
            base.Awake();
            itemType = ItemType.WeaponPart;

            // 무기 컴포넌트 자동 찾기
            if (weaponComponent == null)
            {
                weaponComponent = GetComponentInChildren<MeleeWeaponBase>();
            }

            // 초기에는 무기 컴포넌트만 비활성화 (GameObject는 활성 상태 유지 - 모델이 보이도록)
            if (weaponComponent != null)
            {
                weaponComponent.enabled = false; // GameObject 대신 컴포넌트만 비활성화
            }
        }

        protected override void ProcessPickup()
        {
            // 근접 무기는 소멸되지 않고, ItemCarrySystem으로 처리됨
            // 플레이어의 ItemCarrySystem이 이 아이템을 들면
            // OnPickedUp 이벤트가 발생하고, PlayerMeleeController가 이를 감지함

            Debug.Log($"[MeleeWeaponItem] {itemName} 근접 무기 획득!");
        }

        /// <summary>
        /// 무기가 장착되었을 때 (플레이어가 들고 있을 때)
        /// </summary>
        public void OnEquipped()
        {
            // 무기 컴포넌트 활성화
            if (weaponComponent != null)
            {
                weaponComponent.enabled = true;
            }

            // 들고 있을 때는 AnimateItem() 비활성화 (carryPosition을 따라가야 함)
            isPickable = false;

            Debug.Log($"[MeleeWeaponItem] {itemName} 무기 장착!");
        }

        /// <summary>
        /// 무기가 해제되었을 때 (플레이어가 떨어뜨렸을 때)
        /// </summary>
        public void OnUnequipped()
        {
            // 무기 컴포넌트 비활성화
            if (weaponComponent != null)
            {
                weaponComponent.enabled = false; // 컴포넌트만 비활성화
            }

            // 떨어뜨린 후 다시 줍을 수 있도록 AnimateItem() 재활성화
            isPickable = true;

            Debug.Log($"[MeleeWeaponItem] {itemName} 무기 해제!");
        }

        /// <summary>
        /// 무기 컴포넌트 반환
        /// </summary>
        public MeleeWeaponBase GetWeapon()
        {
            return weaponComponent;
        }
    }
}
