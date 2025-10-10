using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 인벤토리 변경 이벤트
    /// </summary>
    [Serializable]
    public class InventoryChangedEvent : UnityEvent<WeaponPartType, int> { }

    /// <summary>
    /// 플레이어의 인벤토리를 관리하는 싱글톤 매니저
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Weapon Parts Inventory")]
        [SerializeField] private int pencilSpearParts = 0;
        [SerializeField] private int eraserBombParts = 0;
        [SerializeField] private int rubberBandSlingParts = 0;

        [Header("Throw Statistics")]
        [SerializeField] private int pencilSpearThrows = 0;
        [SerializeField] private int eraserBombThrows = 0;
        [SerializeField] private int rubberBandSlingThrows = 0;

        [Header("Upgrade Requirements")]
        [SerializeField] private int partsRequiredForUpgrade = 5;

        [Header("Events")]
        public InventoryChangedEvent OnInventoryChanged = new InventoryChangedEvent();
        public UnityEvent<WeaponPartType> OnWeaponUpgraded = new UnityEvent<WeaponPartType>();
        public UnityEvent<WeaponPartType, int> OnThrowCountChanged = new UnityEvent<WeaponPartType, int>();

        // 무기별 부품 딕셔너리
        private Dictionary<WeaponPartType, int> weaponParts;
        private Dictionary<WeaponPartType, int> weaponPartThrows;

        private void Awake()
        {
            // 싱글톤 패턴
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeInventory();
        }

        private void InitializeInventory()
        {
            weaponParts = new Dictionary<WeaponPartType, int>
            {
                { WeaponPartType.PencilSpear, pencilSpearParts },
                { WeaponPartType.EraserBomb, eraserBombParts },
                { WeaponPartType.RubberBandSling, rubberBandSlingParts }
            };

            weaponPartThrows = new Dictionary<WeaponPartType, int>
            {
                { WeaponPartType.PencilSpear, pencilSpearThrows },
                { WeaponPartType.EraserBomb, eraserBombThrows },
                { WeaponPartType.RubberBandSling, rubberBandSlingThrows }
            };
        }

        /// <summary>
        /// 무기 부품 추가
        /// </summary>
        public void AddWeaponPart(WeaponPartType partType, int amount = 1)
        {
            if (!weaponParts.ContainsKey(partType))
            {
                weaponParts[partType] = 0;
            }

            weaponParts[partType] += amount;

            // Inspector 값 동기화
            UpdateSerializedValues();

            Debug.Log($"[InventoryManager] {partType} 부품 추가: {amount}개 (총: {weaponParts[partType]}개)");

            // 이벤트 발생
            OnInventoryChanged?.Invoke(partType, weaponParts[partType]);

            // 업그레이드 가능 여부 체크
            CheckForUpgrade(partType);
        }

        /// <summary>
        /// 무기 부품 제거
        /// </summary>
        public bool RemoveWeaponPart(WeaponPartType partType, int amount = 1)
        {
            if (!weaponParts.ContainsKey(partType) || weaponParts[partType] < amount)
            {
                Debug.LogWarning($"[InventoryManager] {partType} 부품이 부족합니다.");
                return false;
            }

            weaponParts[partType] -= amount;
            UpdateSerializedValues();

            OnInventoryChanged?.Invoke(partType, weaponParts[partType]);
            return true;
        }

        /// <summary>
        /// 특정 무기 부품 개수 반환
        /// </summary>
        public int GetWeaponPartCount(WeaponPartType partType)
        {
            return weaponParts.ContainsKey(partType) ? weaponParts[partType] : 0;
        }

        /// <summary>
        /// 업그레이드 가능 여부 체크
        /// </summary>
        public bool CanUpgrade(WeaponPartType partType)
        {
            return GetWeaponPartCount(partType) >= partsRequiredForUpgrade;
        }

        /// <summary>
        /// 무기 업그레이드 실행
        /// </summary>
        public bool TryUpgradeWeapon(WeaponPartType partType)
        {
            if (!CanUpgrade(partType))
            {
                Debug.LogWarning($"[InventoryManager] {partType} 업그레이드에 필요한 부품이 부족합니다. " +
                    $"(필요: {partsRequiredForUpgrade}, 현재: {GetWeaponPartCount(partType)})");
                return false;
            }

            // 부품 소모
            if (RemoveWeaponPart(partType, partsRequiredForUpgrade))
            {
                Debug.Log($"[InventoryManager] {partType} 무기가 업그레이드되었습니다!");
                OnWeaponUpgraded?.Invoke(partType);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 자동 업그레이드 체크
        /// </summary>
        private void CheckForUpgrade(WeaponPartType partType)
        {
            if (CanUpgrade(partType))
            {
                Debug.Log($"[InventoryManager] {partType} 업그레이드가 가능합니다!");
            }
        }

        /// <summary>
        /// Inspector 값 업데이트
        /// </summary>
        private void UpdateSerializedValues()
        {
            pencilSpearParts = weaponParts[WeaponPartType.PencilSpear];
            eraserBombParts = weaponParts[WeaponPartType.EraserBomb];
            rubberBandSlingParts = weaponParts[WeaponPartType.RubberBandSling];
        }

        /// <summary>
        /// 전체 인벤토리 초기화
        /// </summary>
        public void ClearInventory()
        {
            foreach (var key in new List<WeaponPartType>(weaponParts.Keys))
            {
                weaponParts[key] = 0;
            }
            UpdateSerializedValues();
            Debug.Log("[InventoryManager] 인벤토리가 초기화되었습니다.");
        }

        /// <summary>
        /// 던진 횟수 추가
        /// </summary>
        public void AddThrowCount(WeaponPartType partType, int amount = 1)
        {
            if (!weaponPartThrows.ContainsKey(partType))
            {
                weaponPartThrows[partType] = 0;
            }

            weaponPartThrows[partType] += amount;

            // Inspector 값 동기화
            UpdateThrowStatistics();

            Debug.Log($"[InventoryManager] {partType} 던지기: {amount}회 (총: {weaponPartThrows[partType]}회)");

            // 이벤트 발생
            OnThrowCountChanged?.Invoke(partType, weaponPartThrows[partType]);
        }

        /// <summary>
        /// 특정 무기 던진 횟수 반환
        /// </summary>
        public int GetThrowCount(WeaponPartType partType)
        {
            return weaponPartThrows.ContainsKey(partType) ? weaponPartThrows[partType] : 0;
        }

        /// <summary>
        /// 전체 던진 횟수 반환
        /// </summary>
        public int GetTotalThrowCount()
        {
            int total = 0;
            foreach (var count in weaponPartThrows.Values)
            {
                total += count;
            }
            return total;
        }

        /// <summary>
        /// 던진 횟수 통계 업데이트
        /// </summary>
        private void UpdateThrowStatistics()
        {
            pencilSpearThrows = weaponPartThrows[WeaponPartType.PencilSpear];
            eraserBombThrows = weaponPartThrows[WeaponPartType.EraserBomb];
            rubberBandSlingThrows = weaponPartThrows[WeaponPartType.RubberBandSling];
        }

        /// <summary>
        /// 디버그: 인벤토리 상태 출력
        /// </summary>
        [ContextMenu("Print Inventory")]
        public void PrintInventory()
        {
            Debug.Log("=== 인벤토리 상태 ===");
            foreach (var kvp in weaponParts)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value}개 (던진 횟수: {GetThrowCount(kvp.Key)}회)");
            }
            Debug.Log($"총 던진 횟수: {GetTotalThrowCount()}회");
            Debug.Log("==================");
        }
    }
}
