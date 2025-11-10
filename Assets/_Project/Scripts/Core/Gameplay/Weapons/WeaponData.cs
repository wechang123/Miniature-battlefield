using UnityEngine;
using YajaGame.Gameplay.Combat;

namespace YajaGame.Gameplay.Weapons
{
    /// <summary>
    /// 무기 데이터 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon", menuName = "YajaGame/Weapons/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string weaponName = "Weapon";
        [SerializeField] private WeaponPartType weaponType;
        [SerializeField] private Sprite weaponIcon;
        [TextArea(3, 5)]
        [SerializeField] private string description = "무기 설명";

        [Header("Combat Stats")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private float knockbackForce = 5f;

        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float projectileLifetime = 5f;
        [SerializeField] private float projectileGravityScale = 1f;

        [Header("Fire Settings")]
        [SerializeField] private float fireRate = 1f; // 초당 발사 횟수
        [SerializeField] private int maxAmmo = 10; // 최대 탄약 (무한이면 -1)
        [SerializeField] private float reloadTime = 2f;

        [Header("Audio")]
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip reloadSound;
        [SerializeField] private AudioClip emptySound;

        [Header("Visual Effects")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject hitEffectPrefab;

        // 프로퍼티
        public string WeaponName => weaponName;
        public WeaponPartType WeaponType => weaponType;
        public Sprite WeaponIcon => weaponIcon;
        public string Description => description;

        public float Damage => damage;
        public DamageType DamageType => damageType;
        public float KnockbackForce => knockbackForce;

        public GameObject ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public float ProjectileGravityScale => projectileGravityScale;

        public float FireRate => fireRate;
        public int MaxAmmo => maxAmmo;
        public float ReloadTime => reloadTime;

        public AudioClip FireSound => fireSound;
        public AudioClip ReloadSound => reloadSound;
        public AudioClip EmptySound => emptySound;

        public GameObject MuzzleFlashPrefab => muzzleFlashPrefab;
        public GameObject HitEffectPrefab => hitEffectPrefab;

        /// <summary>
        /// 발사 간격 (초)
        /// </summary>
        public float FireInterval => 1f / fireRate;

        /// <summary>
        /// 무한 탄약 여부
        /// </summary>
        public bool HasInfiniteAmmo => maxAmmo < 0;

        private void OnValidate()
        {
            // 발사속도는 0보다 커야 함
            if (fireRate <= 0)
            {
                fireRate = 1f;
                Debug.LogWarning($"[WeaponData] {weaponName}의 발사속도가 0 이하입니다. 1로 설정합니다.");
            }

            // 데미지는 0보다 커야 함
            if (damage < 0)
            {
                damage = 0;
                Debug.LogWarning($"[WeaponData] {weaponName}의 데미지가 0 미만입니다. 0으로 설정합니다.");
            }
        }
    }
}
