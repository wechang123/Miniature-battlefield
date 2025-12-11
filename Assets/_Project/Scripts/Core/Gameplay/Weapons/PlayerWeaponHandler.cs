using UnityEngine;
using StarterAssets;

namespace YajaGame.Gameplay.Weapons
{
    /// <summary>
    /// 플레이어의 무기 입력 처리
    /// WeaponController와 Input System을 연결
    /// </summary>
    [RequireComponent(typeof(WeaponController))]
    public class PlayerWeaponHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StarterAssetsInputs input;
        [SerializeField] private Transform firePoint; // 무기 발사 위치

        [Header("Weapon Switching")]
        [SerializeField] private WeaponData[] availableWeapons; // 사용 가능한 무기들
        [SerializeField] private int currentWeaponIndex = 0;

        [Header("Input Settings")]
        [SerializeField] private bool useMouseForFire = true; // 마우스 좌클릭으로 발사
        [SerializeField] private bool useThrowButtonForFire = true; // Throw 버튼으로도 발사 가능

        private WeaponController _weaponController;
        private bool _isFireButtonPressed = false;

        private void Awake()
        {
            _weaponController = GetComponent<WeaponController>();

            // Input 찾기
            if (input == null)
            {
                input = GetComponent<StarterAssetsInputs>();
            }

            // FirePoint 설정
            if (firePoint != null && _weaponController != null)
            {
                // WeaponController의 firePoint는 private이므로 Reflection 사용하거나
                // Inspector에서 직접 설정해야 함
                Debug.Log($"[PlayerWeaponHandler] FirePoint 설정: {firePoint.name}");
            }
        }

        private void Start()
        {
            // 첫 무기 장착
            if (availableWeapons != null && availableWeapons.Length > 0)
            {
                EquipWeapon(0);
            }
        }

        private void Update()
        {
            // 무기가 있을 때만 입력 처리
            if (_weaponController != null)
            {
                HandleWeaponInput();
                HandleWeaponSwitching();
            }
        }

        /// <summary>
        /// 무기 입력 처리
        /// </summary>
        private void HandleWeaponInput()
        {
            if (_weaponController == null) return;

            // 발사 입력 체크
            bool fireInput = false;

            // 마우스 좌클릭
            if (useMouseForFire && Input.GetMouseButton(0))
            {
                fireInput = true;
            }

            // Throw 버튼 (우클릭 또는 설정된 키)
            if (useThrowButtonForFire && input != null && input.@throw)
            {
                fireInput = true;
                input.@throw = false; // 한 번만 발사되도록
            }

            // 발사 시도
            if (fireInput)
            {
                _weaponController.TryFire();
            }

            // 재장전 (R 키)
            if (Input.GetKeyDown(KeyCode.R))
            {
                _weaponController.TryReload();
            }
        }

        /// <summary>
        /// 무기 교체 입력 처리
        /// </summary>
        private void HandleWeaponSwitching()
        {
            if (availableWeapons == null || availableWeapons.Length <= 1) return;

            // 숫자 키로 무기 선택 (1, 2, 3)
            if (Input.GetKeyDown(KeyCode.Alpha1) && availableWeapons.Length > 0)
            {
                EquipWeapon(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && availableWeapons.Length > 1)
            {
                EquipWeapon(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) && availableWeapons.Length > 2)
            {
                EquipWeapon(2);
            }

            // 마우스 휠로 무기 변경
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                // 다음 무기
                EquipWeapon((currentWeaponIndex + 1) % availableWeapons.Length);
            }
            else if (scroll < 0f)
            {
                // 이전 무기
                int prevIndex = currentWeaponIndex - 1;
                if (prevIndex < 0) prevIndex = availableWeapons.Length - 1;
                EquipWeapon(prevIndex);
            }
        }

        /// <summary>
        /// 무기 장착
        /// </summary>
        public void EquipWeapon(int index)
        {
            if (availableWeapons == null || index < 0 || index >= availableWeapons.Length)
            {
                Debug.LogWarning($"[PlayerWeaponHandler] 잘못된 무기 인덱스: {index}");
                return;
            }

            WeaponData weapon = availableWeapons[index];
            if (weapon == null)
            {
                Debug.LogWarning($"[PlayerWeaponHandler] 무기가 null입니다 (인덱스: {index})");
                return;
            }

            currentWeaponIndex = index;
            _weaponController.ChangeWeapon(weapon);

            Debug.Log($"[PlayerWeaponHandler] 무기 장착: {weapon.WeaponName} ({index + 1}/{availableWeapons.Length})");
        }

        /// <summary>
        /// 무기 추가
        /// </summary>
        public void AddWeapon(WeaponData weapon)
        {
            if (weapon == null) return;

            // 배열에 추가
            WeaponData[] newArray = new WeaponData[availableWeapons.Length + 1];
            for (int i = 0; i < availableWeapons.Length; i++)
            {
                newArray[i] = availableWeapons[i];
            }
            newArray[availableWeapons.Length] = weapon;
            availableWeapons = newArray;

            Debug.Log($"[PlayerWeaponHandler] 무기 추가: {weapon.WeaponName}");
        }

        /// <summary>
        /// 현재 무기 제거
        /// </summary>
        public void RemoveCurrentWeapon()
        {
            if (availableWeapons == null || availableWeapons.Length == 0) return;

            // 배열에서 제거
            WeaponData[] newArray = new WeaponData[availableWeapons.Length - 1];
            int newIndex = 0;
            for (int i = 0; i < availableWeapons.Length; i++)
            {
                if (i != currentWeaponIndex)
                {
                    newArray[newIndex] = availableWeapons[i];
                    newIndex++;
                }
            }
            availableWeapons = newArray;

            // 다음 무기로 전환
            if (availableWeapons.Length > 0)
            {
                currentWeaponIndex = Mathf.Min(currentWeaponIndex, availableWeapons.Length - 1);
                EquipWeapon(currentWeaponIndex);
            }
        }

        /// <summary>
        /// 디버그: 무기 목록 출력
        /// </summary>
        [ContextMenu("Print Available Weapons")]
        public void PrintAvailableWeapons()
        {
            if (availableWeapons == null || availableWeapons.Length == 0)
            {
                Debug.Log("[PlayerWeaponHandler] 사용 가능한 무기 없음");
                return;
            }

            Debug.Log($"=== 사용 가능한 무기 ({availableWeapons.Length}개) ===");
            for (int i = 0; i < availableWeapons.Length; i++)
            {
                string current = (i == currentWeaponIndex) ? " [현재]" : "";
                if (availableWeapons[i] != null)
                {
                    Debug.Log($"{i + 1}. {availableWeapons[i].WeaponName}{current}");
                }
                else
                {
                    Debug.Log($"{i + 1}. (null){current}");
                }
            }
        }
    }
}
