using UnityEngine;

namespace YajaGame.Gameplay.Combat
{
    /// <summary>
    /// 데미지 타입
    /// </summary>
    public enum DamageType
    {
        Physical,       // 물리 데미지 (연필창)
        Explosion,      // 폭발 데미지 (지우개폭탄)
        Impact          // 충격 데미지 (고무줄 슬링)
    }

    /// <summary>
    /// 데미지 정보 구조체
    /// </summary>
    public struct DamageInfo
    {
        /// <summary>
        /// 데미지 양
        /// </summary>
        public float Amount;

        /// <summary>
        /// 데미지 타입
        /// </summary>
        public DamageType Type;

        /// <summary>
        /// 데미지를 가한 게임오브젝트 (무기)
        /// </summary>
        public GameObject Source;

        /// <summary>
        /// 데미지 발생 위치
        /// </summary>
        public Vector3 HitPoint;

        /// <summary>
        /// 데미지 방향 (넉백용)
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// 넉백 힘
        /// </summary>
        public float KnockbackForce;

        /// <summary>
        /// 생성자
        /// </summary>
        public DamageInfo(float amount, DamageType type, GameObject source, Vector3 hitPoint, Vector3 direction, float knockbackForce = 0f)
        {
            Amount = amount;
            Type = type;
            Source = source;
            HitPoint = hitPoint;
            Direction = direction.normalized;
            KnockbackForce = knockbackForce;
        }

        /// <summary>
        /// 간단한 데미지 생성
        /// </summary>
        public static DamageInfo Create(float amount, DamageType type, GameObject source)
        {
            return new DamageInfo(amount, type, source, Vector3.zero, Vector3.zero, 0f);
        }
    }
}
