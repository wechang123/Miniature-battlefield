using UnityEngine;
using UnityEngine.Events;
using YajaGame.Gameplay.Projectiles;

namespace YajaGame.Gameplay.Weapons
{
    /// <summary>
    /// InventoryManager와 연동되는 무기 컨트롤러
    /// 인벤토리의 무기 부품 개수를 탄약으로 사용
    /// </summary>
    public class InventoryWeaponController : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private WeaponPartType weaponPartType;
        [SerializeField] private Transform firePoint; // 발사 위치

        [Header("Events")]
        public UnityEvent<WeaponData> OnWeaponFired = new UnityEvent<WeaponData>();
        public UnityEvent<int> OnAmmoChanged = new UnityEvent<int>();

        private float _lastFireTime = 0f;
        private Camera _mainCamera;
        private InventoryManager _inventory;

        // 프로퍼티
        public WeaponData WeaponData => weaponData;
        public int CurrentAmmo => _inventory != null ? _inventory.GetWeaponPartCount(weaponPartType) : 0;
        public bool CanFire => CurrentAmmo > 0 && Time.time - _lastFireTime >= weaponData.FireInterval;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            // InventoryManager 찾기
            _inventory = InventoryManager.Instance;
            if (_inventory == null)
            {
                Debug.LogWarning("[InventoryWeaponController] InventoryManager를 찾을 수 없습니다!");
            }
            else
            {
                // 인벤토리 변경 이벤트 구독
                _inventory.OnInventoryChanged.AddListener(OnInventoryUpdated);

                // 초기 탄약 UI 업데이트
                OnAmmoChanged?.Invoke(CurrentAmmo);
                Debug.Log($"[InventoryWeaponController] {weaponData.WeaponName} 초기화 완료 (탄약: {CurrentAmmo})");
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (_inventory != null)
            {
                _inventory.OnInventoryChanged.RemoveListener(OnInventoryUpdated);
            }
        }

        /// <summary>
        /// 인벤토리 업데이트 시 호출
        /// </summary>
        private void OnInventoryUpdated(WeaponPartType partType, int newCount)
        {
            // 이 무기의 탄약이 변경되었으면 이벤트 발생
            if (partType == weaponPartType)
            {
                OnAmmoChanged?.Invoke(newCount);
                Debug.Log($"[InventoryWeaponController] {weaponPartType} 탄약 업데이트: {newCount}개");
            }
        }

        /// <summary>
        /// 발사 시도
        /// </summary>
        public bool TryFire()
        {
            if (_inventory == null)
            {
                Debug.LogWarning("[InventoryWeaponController] InventoryManager가 없습니다!");
                return false;
            }

            // 발사 가능 여부 체크
            if (!CanFire)
            {
                // 탄약이 없으면 빈 소리 재생
                if (CurrentAmmo <= 0 && weaponData.EmptySound != null)
                {
                    AudioSource.PlayClipAtPoint(weaponData.EmptySound, transform.position);
                    Debug.Log("[InventoryWeaponController] 탄약이 없습니다!");
                }
                else
                {
                    Debug.Log($"[InventoryWeaponController] 쿨다운 중 ({Time.time - _lastFireTime:F2}/{weaponData.FireInterval:F2}초)");
                }
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

            // 인벤토리에서 탄약 소모
            if (!_inventory.RemoveWeaponPart(weaponPartType, 1))
            {
                Debug.LogWarning("[InventoryWeaponController] 탄약 제거 실패!");
                return;
            }

            // 발사 위치 및 방향 계산
            Vector3 firePosition = firePoint != null ? firePoint.position : transform.position + transform.forward;
            Vector3 fireDirection = GetFireDirection();

            // 발사체 생성
            if (weaponData.ProjectilePrefab != null)
            {
                GameObject projectileObj = Instantiate(weaponData.ProjectilePrefab, firePosition, Quaternion.LookRotation(fireDirection));

                // 발사체 초기화
                WeaponProjectileBase projectile = projectileObj.GetComponent<WeaponProjectileBase>();
                if (projectile != null)
                {
                    projectile.Initialize(weaponData, fireDirection);
                }
                else
                {
                    // WeaponProjectileBase가 없으면 그냥 속도만 부여
                    Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = fireDirection * weaponData.ProjectileSpeed;
                    }
                    Destroy(projectileObj, weaponData.ProjectileLifetime);
                }
            }

            // 머즐 플래시
            if (weaponData.MuzzleFlashPrefab != null)
            {
                GameObject flash = Instantiate(weaponData.MuzzleFlashPrefab, firePosition, Quaternion.LookRotation(fireDirection));
                Destroy(flash, 0.5f);
            }

            // 발사 사운드
            if (weaponData.FireSound != null)
            {
                AudioSource.PlayClipAtPoint(weaponData.FireSound, firePosition);
            }

            // 던지기 통계 업데이트
            _inventory.AddThrowCount(weaponPartType, 1);

            // 이벤트 발생
            OnWeaponFired?.Invoke(weaponData);

            Debug.Log($"[InventoryWeaponController] 발사! {weaponData.WeaponName} (남은 탄약: {CurrentAmmo})");
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
                Vector3 targetPoint = hit.point;
                Vector3 firePos = firePoint != null ? firePoint.position : transform.position;
                return (targetPoint - firePos).normalized;
            }
            else
            {
                // 타겟이 없으면 카메라 방향
                return ray.direction;
            }
        }

        /// <summary>
        /// 디버그: 무기 정보 출력
        /// </summary>
        [ContextMenu("Print Weapon Info")]
        public void PrintWeaponInfo()
        {
            if (weaponData == null)
            {
                Debug.Log("[InventoryWeaponController] WeaponData가 없습니다!");
                return;
            }

            Debug.Log($"=== 무기 정보 ===\n" +
                $"이름: {weaponData.WeaponName}\n" +
                $"타입: {weaponPartType}\n" +
                $"데미지: {weaponData.Damage}\n" +
                $"탄약: {CurrentAmmo}\n" +
                $"발사속도: {weaponData.FireRate}/초\n" +
                $"발사 간격: {weaponData.FireInterval}초");
        }
    }
}
