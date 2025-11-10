using UnityEngine;

namespace YajaGame.Gameplay.Combat
{
    /// <summary>
    /// 데미지를 받을 수 있는 엔티티 인터페이스
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 데미지를 받습니다
        /// </summary>
        /// <param name="damageInfo">데미지 정보</param>
        void TakeDamage(DamageInfo damageInfo);

        /// <summary>
        /// 현재 체력
        /// </summary>
        float CurrentHealth { get; }

        /// <summary>
        /// 최대 체력
        /// </summary>
        float MaxHealth { get; }

        /// <summary>
        /// 살아있는지 여부
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Transform (위치 정보)
        /// </summary>
        Transform Transform { get; }
    }
}
