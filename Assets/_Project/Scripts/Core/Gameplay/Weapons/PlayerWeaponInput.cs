using UnityEngine;
using StarterAssets;

namespace YajaGame.Gameplay.Weapons
{
    /// <summary>
    /// 플레이어 무기 입력 처리
    /// </summary>
    public class PlayerWeaponInput : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryWeaponController weaponController;
        [SerializeField] private StarterAssetsInputs input;

        [Header("Input Settings")]
        [SerializeField] private bool useMouseInput = true; // 마우스 클릭 사용
        [SerializeField] private bool useInputAction = true; // StarterAssetsInputs 사용

        private void Awake()
        {
            // 자동 탐색
            if (weaponController == null)
            {
                weaponController = GetComponent<InventoryWeaponController>();
            }

            if (input == null)
            {
                input = GetComponent<StarterAssetsInputs>();
            }
        }

        private void Update()
        {
            bool fireInput = false;

            // 마우스 클릭 입력
            if (useMouseInput && Input.GetMouseButtonDown(0))
            {
                fireInput = true;
            }

            // StarterAssetsInputs 발사 입력
            if (useInputAction && input != null && input.fire)
            {
                fireInput = true;
                input.fire = false; // 입력 소모
            }

            // 발사 시도
            if (fireInput && weaponController != null)
            {
                weaponController.TryFire();
            }
        }

        /// <summary>
        /// 무기 컨트롤러 변경
        /// </summary>
        public void SetWeaponController(InventoryWeaponController controller)
        {
            weaponController = controller;
        }
    }
}
