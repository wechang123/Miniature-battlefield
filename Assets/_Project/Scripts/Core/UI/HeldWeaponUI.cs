using UnityEngine;
using UnityEngine.UI;
using YajaGame.Gameplay;

namespace YajaGame.UI
{
    /// <summary>
    /// 들고 있는 무기 슬롯 UI - 무기를 들고 있을 때만 표시
    /// </summary>
    public class HeldWeaponUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject slotPanel;      // 슬롯 패널 (전체)
        [SerializeField] private Image slotImage;           // 슬롯 이미지

        [Header("Weapon Slot Sprites")]
        [SerializeField] private Sprite emptySlotSprite;           // 빈 슬롯
        [SerializeField] private Sprite pencilSpearSlotSprite;     // 연필창 슬롯
        [SerializeField] private Sprite eraserBombSlotSprite;      // 지우개폭탄 슬롯
        [SerializeField] private Sprite rubberBandSlingSlotSprite; // 고무줄슬링 슬롯

        private ItemCarrySystem itemCarrySystem;
        private ItemThrowSystem itemThrowSystem;

        private void Start()
        {
            // 초기 상태: 빈 슬롯 표시
            ShowEmptySlot();

            // 시스템 자동 찾기
            itemCarrySystem = FindObjectOfType<ItemCarrySystem>();
            itemThrowSystem = FindObjectOfType<ItemThrowSystem>();

            // ItemCarrySystem 이벤트 연결
            if (itemCarrySystem != null)
            {
                itemCarrySystem.OnItemPickedUp.AddListener(OnItemPickedUp);
                itemCarrySystem.OnItemDropped.AddListener(OnItemDropped);
                Debug.Log("[HeldWeaponUI] ItemCarrySystem 이벤트 연결 완료");
            }
            else
            {
                Debug.LogWarning("[HeldWeaponUI] ItemCarrySystem을 찾을 수 없습니다!");
            }

            // ItemThrowSystem 이벤트 연결
            if (itemThrowSystem != null)
            {
                itemThrowSystem.OnItemThrown.AddListener(OnItemThrown);
                Debug.Log("[HeldWeaponUI] ItemThrowSystem 이벤트 연결 완료");
            }
        }

        /// <summary>
        /// 아이템 주웠을 때
        /// </summary>
        private void OnItemPickedUp(ItemBase item)
        {
            Debug.Log($"[HeldWeaponUI] OnItemPickedUp 호출됨! item: {(item != null ? item.name : "null")}");

            if (item == null) return;

            // 1. WeaponPartItem인지 확인
            WeaponPartItem weaponPart = item.GetComponent<WeaponPartItem>();
            if (weaponPart != null)
            {
                ShowSlot(weaponPart.PartType);
                Debug.Log($"[HeldWeaponUI] WeaponPartItem 무기 주움: {weaponPart.PartType}");
                return;
            }

            // 2. MeleeWeaponItem인지 확인 (연필창 등)
            MeleeWeaponItem meleeWeapon = item.GetComponent<MeleeWeaponItem>();
            if (meleeWeapon != null)
            {
                // 아이템 이름으로 무기 타입 판별
                WeaponPartType weaponType = GetWeaponTypeFromName(item.ItemName);
                ShowSlot(weaponType);
                Debug.Log($"[HeldWeaponUI] MeleeWeaponItem 무기 주움: {item.ItemName} -> {weaponType}");
                return;
            }

            // 3. 그 외 무기 아이템 (ItemType으로 판별)
            if (item.Type == ItemType.WeaponPart)
            {
                WeaponPartType weaponType = GetWeaponTypeFromName(item.ItemName);
                ShowSlot(weaponType);
                Debug.Log($"[HeldWeaponUI] WeaponPart 아이템 주움: {item.ItemName} -> {weaponType}");
            }
        }

        /// <summary>
        /// 아이템 이름으로 무기 타입 판별
        /// </summary>
        private WeaponPartType GetWeaponTypeFromName(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return WeaponPartType.PencilSpear;

            string lowerName = itemName.ToLower();

            if (lowerName.Contains("연필") || lowerName.Contains("pencil"))
                return WeaponPartType.PencilSpear;
            if (lowerName.Contains("지우개") || lowerName.Contains("eraser"))
                return WeaponPartType.EraserBomb;
            if (lowerName.Contains("고무줄") || lowerName.Contains("rubber"))
                return WeaponPartType.RubberBandSling;

            // 기본값
            return WeaponPartType.PencilSpear;
        }

        /// <summary>
        /// 아이템 버렸을 때
        /// </summary>
        private void OnItemDropped(ItemBase item)
        {
            ShowEmptySlot();
            Debug.Log("[HeldWeaponUI] 무기 버림 - 빈 슬롯 표시");
        }

        /// <summary>
        /// 아이템 던졌을 때
        /// </summary>
        private void OnItemThrown(ItemBase item)
        {
            ShowEmptySlot();
            Debug.Log("[HeldWeaponUI] 무기 던짐 - 빈 슬롯 표시");
        }

        /// <summary>
        /// 슬롯 표시
        /// </summary>
        private void ShowSlot(WeaponPartType weaponType)
        {
            if (slotPanel != null)
            {
                slotPanel.SetActive(true);
            }

            if (slotImage != null)
            {
                slotImage.sprite = GetSpriteForWeaponType(weaponType);
                slotImage.enabled = true;
            }
        }

        /// <summary>
        /// 빈 슬롯 표시
        /// </summary>
        private void ShowEmptySlot()
        {
            if (slotPanel != null)
            {
                slotPanel.SetActive(true);
            }

            if (slotImage != null && emptySlotSprite != null)
            {
                slotImage.sprite = emptySlotSprite;
                slotImage.enabled = true;
            }
        }

        /// <summary>
        /// 무기 타입에 맞는 스프라이트 반환
        /// </summary>
        private Sprite GetSpriteForWeaponType(WeaponPartType weaponType)
        {
            switch (weaponType)
            {
                case WeaponPartType.PencilSpear:
                    return pencilSpearSlotSprite;
                case WeaponPartType.EraserBomb:
                    return eraserBombSlotSprite;
                case WeaponPartType.RubberBandSling:
                    return rubberBandSlingSlotSprite;
                default:
                    return emptySlotSprite;
            }
        }

        private void OnDestroy()
        {
            if (itemCarrySystem != null)
            {
                itemCarrySystem.OnItemPickedUp.RemoveListener(OnItemPickedUp);
                itemCarrySystem.OnItemDropped.RemoveListener(OnItemDropped);
            }

            if (itemThrowSystem != null)
            {
                itemThrowSystem.OnItemThrown.RemoveListener(OnItemThrown);
            }
        }
    }
}
