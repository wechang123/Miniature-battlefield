using UnityEngine;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 줍기 가능한 아이템에 대한 인터페이스
    /// </summary>
    public interface IPickupable
    {
        /// <summary>
        /// 아이템이 플레이어에게 수집될 때 호출됩니다
        /// </summary>
        void OnPickup();

        /// <summary>
        /// 아이템의 트랜스폼
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// 아이템이 줍기 가능한 상태인지 여부
        /// </summary>
        bool IsPickable { get; }
    }
}
