using UnityEngine;
using UnityEngine.Events;
using YajaGame.Gameplay.Projectiles;

namespace YajaGame.Gameplay.Weapons
{
    /// <summary>
    /// 플레이어의 무기 발사 관리
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponData currentWeapon;
        [SerializeField] private Transform firePoint; // 발사 위치

        [Header("Ammo")]
        [SerializeField] private int currentAmmo;
        [SerializeField] private bool isReloading = false;

        [Header("Events")]
        public UnityEvent<WeaponData> OnWeaponFired = new UnityEvent<WeaponData>();
        public UnityEvent<WeaponData> OnWeaponChanged = new UnityEvent<WeaponData>();
        public UnityEvent<int, int> OnAmmoChanged = new UnityEvent<int, int>(); // (current, max)
        public UnityEvent OnReloadStarted = new UnityEvent();
        public UnityEvent OnReloadFinished = new UnityEvent();

        private float _lastFireTime = 0f;
        private Camera _mainCamera;

        // 프로퍼티
        public WeaponData CurrentWeapon => currentWeapon;
        public int CurrentAmmo => currentAmmo;
        public bool IsReloading => isReloading;
        public bool CanFire => !isReloading && (currentWeapon.HasInfiniteAmmo || currentAmmo > 0);

        private void Awake()
        {
            _mainCamera = Camera.main;

            // 무기 초기화
            if (currentWeapon != null)
            {
                InitializeWeapon(currentWeapon);
            }
        }

        /// <summary>
        /// 무기 초기화
        /// </summary>
        private void InitializeWeapon(WeaponData weapon)
        {
            currentWeapon = weapon;

            // 탄약 초기화
            if (weapon.HasInfiniteAmmo)
            {
                currentAmmo = -1;
            }
            else
            {
                currentAmmo = weapon.MaxAmmo;
            }

            OnAmmoChanged?.Invoke(currentAmmo, weapon.MaxAmmo);
            Debug.Log($"[WeaponController] 무기 초기화: {weapon.WeaponName} (탄약: {currentAmmo}/{weapon.MaxAmmo})");
        }

        /// <summary>
        /// 무기 변경
        /// </summary>
        public void ChangeWeapon(WeaponData newWeapon)
        {
            if (newWeapon == null)
            {
                Debug.LogWarning("[WeaponController] 변경할 무기가 null입니다!");
                return;
            }

            InitializeWeapon(newWeapon);
            OnWeaponChanged?.Invoke(newWeapon);
            Debug.Log($"[WeaponController] 무기 변경: {newWeapon.WeaponName}");
        }

        /// <summary>
        /// 발사 시도
        /// </summary>
        public bool TryFire()
        {
            // 발사 가능 여부 체크
            if (!CanFire)
            {
                // 탄약이 없으면 빈 소리 재생
                if (!currentWeapon.HasInfiniteAmmo && currentAmmo <= 0 && currentWeapon.EmptySound != null)
                {
                    AudioSource.PlayClipAtPoint(currentWeapon.EmptySound, transform.position);
                }
                return false;
            }

            // 발사 쿨다운 체크
            if (Time.time - _lastFireTime < currentWeapon.FireInterval)
            {
                return false;
            }

            // 발사
            Fire();
            return true;
        }

        /// <summary>
        /// 발사 실행
        /// </summary>
        private void Fire()
        {
            _lastFireTime = Time.time;

            // 탄약 소모
            if (!currentWeapon.HasInfiniteAmmo)
            {
                currentAmmo--;
                OnAmmoChanged?.Invoke(currentAmmo, currentWeapon.MaxAmmo);
            }

            // 발사 위치 및 방향 계산
            Vector3 firePosition = firePoint != null ? firePoint.position : transform.position + transform.forward;
            Vector3 fireDirection = GetFireDirection();

            // 발사체 생성
            if (currentWeapon.ProjectilePrefab != null)
            {
                GameObject projectileObj = Instantiate(currentWeapon.ProjectilePrefab, firePosition, Quaternion.LookRotation(fireDirection));

                // 발사체 초기화
                WeaponProjectileBase projectile = projectileObj.GetComponent<WeaponProjectileBase>();
                if (projectile != null)
                {
                    projectile.Initialize(currentWeapon, fireDirection);
                }
                else
                {
                    // WeaponProjectileBase가 없으면 그냥 속도만 부여
                    Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = fireDirection * currentWeapon.ProjectileSpeed;
                    }
                    Destroy(projectileObj, currentWeapon.ProjectileLifetime);
                }
            }

            // 머즐 플래시
            if (currentWeapon.MuzzleFlashPrefab != null)
            {
                GameObject flash = Instantiate(currentWeapon.MuzzleFlashPrefab, firePosition, Quaternion.LookRotation(fireDirection));
                Destroy(flash, 0.5f);
            }

            // 발사 사운드
            if (currentWeapon.FireSound != null)
            {
                AudioSource.PlayClipAtPoint(currentWeapon.FireSound, firePosition);
            }

            // 이벤트 발생
            OnWeaponFired?.Invoke(currentWeapon);

            Debug.Log($"[WeaponController] 발사! {currentWeapon.WeaponName} (남은 탄약: {currentAmmo}/{currentWeapon.MaxAmmo})");
        }

        /// <summary>
        /// 발사 방향 계산 (화면 중앙 기준)
        /// </summary>
        private Vector3 GetFireDirection()
        {
            if (_mainCamera == null)
            {
                return transform.forward;
            }

            // 화면 중앙에서 Ray 발사
            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            // Raycast로 타겟 찾기
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                return (hit.point - firePoint.position).normalized;
            }
            else
            {
                // 타겟이 없으면 카메라 방향
                return ray.direction;
            }
        }

        /// <summary>
        /// 재장전 시도
        /// </summary>
        public bool TryReload()
        {
            // 무한 탄약이거나 이미 재장전 중이거나 탄약이 풀이면 재장전 불가
            if (currentWeapon.HasInfiniteAmmo || isReloading || currentAmmo >= currentWeapon.MaxAmmo)
            {
                return false;
            }

            StartReload();
            return true;
        }

        /// <summary>
        /// 재장전 시작
        /// </summary>
        private void StartReload()
        {
            isReloading = true;
            OnReloadStarted?.Invoke();

            // 재장전 사운드
            if (currentWeapon.ReloadSound != null)
            {
                AudioSource.PlayClipAtPoint(currentWeapon.ReloadSound, transform.position);
            }

            Debug.Log($"[WeaponController] 재장전 시작... ({currentWeapon.ReloadTime}초)");

            // 재장전 완료
            Invoke(nameof(FinishReload), currentWeapon.ReloadTime);
        }

        /// <summary>
        /// 재장전 완료
        /// </summary>
        private void FinishReload()
        {
            isReloading = false;
            currentAmmo = currentWeapon.MaxAmmo;
            OnAmmoChanged?.Invoke(currentAmmo, currentWeapon.MaxAmmo);
            OnReloadFinished?.Invoke();

            Debug.Log($"[WeaponController] 재장전 완료! (탄약: {currentAmmo}/{currentWeapon.MaxAmmo})");
        }

        /// <summary>
        /// 탄약 추가
        /// </summary>
        public void AddAmmo(int amount)
        {
            if (currentWeapon.HasInfiniteAmmo) return;

            currentAmmo += amount;
            currentAmmo = Mathf.Min(currentAmmo, currentWeapon.MaxAmmo);
            OnAmmoChanged?.Invoke(currentAmmo, currentWeapon.MaxAmmo);

            Debug.Log($"[WeaponController] 탄약 추가: +{amount} (현재: {currentAmmo}/{currentWeapon.MaxAmmo})");
        }

        /// <summary>
        /// 디버그: 무기 정보 출력
        /// </summary>
        [ContextMenu("Print Weapon Info")]
        public void PrintWeaponInfo()
        {
            if (currentWeapon == null)
            {
                Debug.Log("[WeaponController] 장착된 무기 없음");
                return;
            }

            Debug.Log($"=== 무기 정보 ===\n" +
                $"이름: {currentWeapon.WeaponName}\n" +
                $"타입: {currentWeapon.WeaponType}\n" +
                $"데미지: {currentWeapon.Damage}\n" +
                $"탄약: {currentAmmo}/{currentWeapon.MaxAmmo}\n" +
                $"발사속도: {currentWeapon.FireRate}/초\n" +
                $"재장전 시간: {currentWeapon.ReloadTime}초");
        }
    }
}
