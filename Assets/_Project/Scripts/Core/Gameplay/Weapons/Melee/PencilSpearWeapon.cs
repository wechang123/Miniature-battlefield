using UnityEngine;

namespace YajaGame.Gameplay.Weapons.Melee
{
    /// <summary>
    /// 연필창 무기
    /// 베기 공격을 하는 근접 무기
    /// </summary>
    public class PencilSpearWeapon : MeleeWeaponBase
    {
        [Header("Pencil Spear Settings")]
        [SerializeField] private float slashRange = 2.5f; // 베기 범위
        [SerializeField] private float slashAngle = 90f; // 베기 각도

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("[PencilSpear] 연필창 초기화!");
        }

        protected override void StartAttack()
        {
            base.StartAttack();
            Debug.Log("[PencilSpear] 베기 공격 시작!");
        }

        protected override void EndAttack()
        {
            base.EndAttack();
            Debug.Log("[PencilSpear] 베기 공격 종료!");
        }

        private void OnDrawGizmosSelected()
        {
            // 베기 범위 표시
            Gizmos.color = Color.yellow;
            Vector3 forward = transform.forward;

            // 부채꼴 모양으로 범위 표시
            for (int i = 0; i <= 10; i++)
            {
                float angle = -slashAngle / 2 + (slashAngle / 10) * i;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
                Gizmos.DrawLine(transform.position, transform.position + direction * slashRange);
            }
        }
    }
}
