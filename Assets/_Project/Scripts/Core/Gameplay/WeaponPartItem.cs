using UnityEngine;

namespace YajaGame.Gameplay
{
    // 무기 파츠 타입 열거형
    public enum WeaponPartType
    {
        Barrel,
        Stock,
        Grip,
        Sight,
        Magazine
    }
    
    /// <summary>
    /// 무기 파츠 아이템
    /// </summary>
    public class WeaponPartItem : ItemBase
    {
        [Header("Weapon Part Info")]
        [SerializeField] private WeaponPartType partType;
        
        public WeaponPartType PartType => partType;
        
        public override void OnPickup()
        {
            base.OnPickup();
            Debug.Log($"[WeaponPartItem] {partType} 파츠를 주웠습니다!");
        }
    }
    
    /// <summary>
    /// 인벤토리 매니저 (싱글톤)
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        private static InventoryManager instance;
        public static InventoryManager Instance => instance;
        
        [Header("Statistics")]
        [SerializeField] private int totalPickupCount = 0;
        [SerializeField] private int totalThrowCount = 0;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 던지기 횟수 추가
        /// </summary>
        public void AddThrowCount(WeaponPartType partType, int count)
        {
            totalThrowCount += count;
            Debug.Log($"[InventoryManager] {partType} 던지기 횟수 추가: {count}, 총: {totalThrowCount}");
        }
        
        /// <summary>
        /// 줍기 횟수 추가
        /// </summary>
        public void AddPickupCount(int count)
        {
            totalPickupCount += count;
            Debug.Log($"[InventoryManager] 줍기 횟수 추가: {count}, 총: {totalPickupCount}");
        }
        
        public int GetTotalThrowCount() => totalThrowCount;
        public int GetTotalPickupCount() => totalPickupCount;
    }
}