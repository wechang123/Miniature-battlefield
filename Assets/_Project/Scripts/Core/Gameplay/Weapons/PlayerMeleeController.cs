using UnityEngine;
using StarterAssets;
using YajaGame.Gameplay.Weapons.Melee;

namespace YajaGame.Gameplay.Weapons
{
    /// <summary>
    /// 플레이어의 근접 무기 입력 처리
    /// ItemCarrySystem과 연동하여 근접 무기를 들고 있을 때만 작동
    /// </summary>
    public class PlayerMeleeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StarterAssetsInputs input;
        [SerializeField] private ItemCarrySystem carrySystem; // 아이템 운반 시스템
        [SerializeField] private Animator playerAnimator; // 플레이어 애니메이터
        [SerializeField] private MeleeWeaponBase meleeWeapon; // 현재 장착한 근접 무기

        [Header("Input Settings")]
        [SerializeField] private bool useMouseLeftClick = true; // 마우스 좌클릭으로 공격
        [SerializeField] private bool useAttackButton = true; // Fire 버튼으로도 공격

        [Header("Control")]
        [SerializeField] private bool isEnabled = true; // 컨트롤러 활성화 상태

        private MeleeWeaponItem _currentWeaponItem; // 현재 들고 있는 무기 아이템

        private void Awake()
        {
            // 자동으로 컴포넌트 찾기
            if (input == null)
            {
                input = GetComponent<StarterAssetsInputs>();
            }

            if (carrySystem == null)
            {
                carrySystem = GetComponent<ItemCarrySystem>();
            }

            if (playerAnimator == null)
            {
                playerAnimator = GetComponent<Animator>();
            }
        }

        private void Start()
        {
            // ItemCarrySystem 이벤트 연결
            if (carrySystem != null)
            {
                carrySystem.OnItemPickedUp.AddListener(OnItemPickedUp);
                carrySystem.OnItemDropped.AddListener(OnItemDropped);
            }
        }

        private void Update()
        {
            // 컨트롤러가 비활성화되어 있거나 무기가 없으면 리턴
            if (!isEnabled || meleeWeapon == null) return;

            // 공격 입력 체크
            bool attackInput = false;

            if (useMouseLeftClick && Input.GetMouseButtonDown(0))
            {
                attackInput = true;
                Debug.Log("[PlayerMeleeController] 좌클릭 감지 - 근접 공격 시도!");
            }

            if (useAttackButton && input != null && input.@throw)
            {
                attackInput = true;
                input.@throw = false; // 입력 소비
            }

            // 공격 시도
            if (attackInput)
            {
                bool attackSuccess = meleeWeapon.TryAttack();
                Debug.Log($"[PlayerMeleeController] 공격 시도 결과: {(attackSuccess ? "성공" : "실패")} (쿨다운 체크 등)");
            }
        }

        /// <summary>
        /// 아이템을 주웠을 때
        /// </summary>
        private void OnItemPickedUp(ItemBase item)
        {
            Debug.Log($"[PlayerMeleeController] 아이템 주움: {item.ItemName}");

            // 근접 무기 아이템인지 확인
            MeleeWeaponItem weaponItem = item.GetComponent<MeleeWeaponItem>();
            if (weaponItem != null)
            {
                // 근접 무기 장착
                _currentWeaponItem = weaponItem;
                meleeWeapon = weaponItem.GetWeapon();

                // 무기 활성화 이전에 Animator를 먼저 설정해야 함
                if (meleeWeapon != null)
                {
                    if (playerAnimator != null)
                    {
                        meleeWeapon.SetAnimator(playerAnimator);
                        Debug.Log($"[PlayerMeleeController] Animator 설정 완료: {playerAnimator.name}");
                    }
                    else
                    {
                        Debug.LogError($"[PlayerMeleeController] playerAnimator가 null입니다! 공격 애니메이션이 작동하지 않습니다.");
                    }
                }

                // 무기 활성화
                weaponItem.OnEquipped();

                // 컨트롤러 활성화
                isEnabled = true;

                Debug.Log($"[PlayerMeleeController] 근접 무기 장착: {item.ItemName}");
            }
            else
            {
                // 근접 무기가 아니면 컨트롤러 비활성화
                isEnabled = false;
                Debug.Log($"[PlayerMeleeController] 근접 무기가 아님 - 컨트롤러 비활성화");
            }
        }

        /// <summary>
        /// 아이템을 떨어뜨렸을 때
        /// </summary>
        private void OnItemDropped(ItemBase item)
        {
            Debug.Log($"[PlayerMeleeController] 아이템 떨어뜨림: {item.ItemName}");

            // 현재 무기를 떨어뜨린 경우
            if (_currentWeaponItem != null && item == _currentWeaponItem.GetComponent<ItemBase>())
            {
                // 무기 비활성화
                _currentWeaponItem.OnUnequipped();

                // 참조 제거
                _currentWeaponItem = null;
                meleeWeapon = null;

                // 컨트롤러 비활성화
                isEnabled = false;

                Debug.Log($"[PlayerMeleeController] 근접 무기 해제");
            }
        }

        /// <summary>
        /// 근접 무기 변경 (외부에서 직접 설정)
        /// </summary>
        public void SetMeleeWeapon(MeleeWeaponBase weapon)
        {
            meleeWeapon = weapon;
            isEnabled = (weapon != null);
            Debug.Log($"[PlayerMeleeController] 근접 무기 변경: {(weapon != null ? weapon.name : "None")}");
        }

        /// <summary>
        /// 컨트롤러 활성화/비활성화
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            Debug.Log($"[PlayerMeleeController] 컨트롤러 {(enabled ? "활성화" : "비활성화")}");
        }

        /// <summary>
        /// 현재 근접 무기
        /// </summary>
        public MeleeWeaponBase CurrentMeleeWeapon => meleeWeapon;

        /// <summary>
        /// 컨트롤러 활성화 상태
        /// </summary>
        public bool IsEnabled => isEnabled;
    }
}
