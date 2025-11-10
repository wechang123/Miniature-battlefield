using UnityEngine;
using StarterAssets;
using YajaGame.Gameplay.Weapons.Melee;

namespace YajaGame.Gameplay.Weapons
{
    /// <summary>
    /// 플레이어의 근접 무기 입력 처리
    /// </summary>
    public class PlayerMeleeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StarterAssetsInputs input;
        [SerializeField] private MeleeWeaponBase meleeWeapon; // 현재 장착한 근접 무기

        [Header("Input Settings")]
        [SerializeField] private bool useMouseLeftClick = true; // 마우스 좌클릭으로 공격
        [SerializeField] private bool useAttackButton = true; // Fire 버튼으로도 공격

        private void Awake()
        {
            // 자동으로 컴포넌트 찾기
            if (input == null)
            {
                input = GetComponent<StarterAssetsInputs>();
            }

            if (meleeWeapon == null)
            {
                meleeWeapon = GetComponentInChildren<MeleeWeaponBase>();
            }
        }

        private void Update()
        {
            if (meleeWeapon == null) return;

            // 공격 입력 체크
            bool attackInput = false;

            if (useMouseLeftClick && Input.GetMouseButtonDown(0))
            {
                attackInput = true;
            }

            if (useAttackButton && input != null && input.shoot)
            {
                attackInput = true;
                input.shoot = false; // 입력 소비
            }

            // 공격 시도
            if (attackInput)
            {
                meleeWeapon.TryAttack();
            }
        }

        /// <summary>
        /// 근접 무기 변경
        /// </summary>
        public void SetMeleeWeapon(MeleeWeaponBase weapon)
        {
            meleeWeapon = weapon;
            Debug.Log($"[PlayerMeleeController] 근접 무기 변경: {weapon != null ? weapon.name : "None"}");
        }

        /// <summary>
        /// 현재 근접 무기
        /// </summary>
        public MeleeWeaponBase CurrentMeleeWeapon => meleeWeapon;
    }
}
