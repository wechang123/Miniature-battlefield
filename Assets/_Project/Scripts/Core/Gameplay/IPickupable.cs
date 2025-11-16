
using UnityEngine;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 주울 수 있는 아이템을 위한 인터페이스
    /// </summary>
    public interface IPickupable
    {
        /// <summary>
        /// 아이템의 Transform
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// 주울 수 있는 상태인지
        /// </summary>
        bool IsPickable { get; }

        /// <summary>
        /// 아이템이 주워졌을 때 호출
        /// </summary>
        void OnPickup();
    }
}