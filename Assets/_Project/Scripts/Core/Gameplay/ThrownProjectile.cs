using UnityEngine;
using UnityEngine.Events;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 던져진 아이템의 물리 및 충돌 처리
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ThrownProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float impactForce = 5f;

        [Header("Ground Detection")]
        [SerializeField] private float groundStopVelocity = 0.5f; // 이 속도 이하면 바닥에 정착
        [SerializeField] private float settleDelay = 2f; // 던진 후 최소 이 시간 후 정착 가능

        [Header("Audio")]
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private AudioClip groundHitSound;

        [Header("Events")]
        public UnityEvent<GameObject> OnEnemyHit;
        public UnityEvent OnGroundHit;

        private Rigidbody _rb;
        private ItemBase _itemBase;
        private bool _hasLaunched = false;
        private float _launchTime;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _itemBase = GetComponent<ItemBase>();
        }

        /// <summary>
        /// 발사
        /// </summary>
        public void Launch(Vector3 velocity)
        {
            if (_rb == null)
            {
                Debug.LogError("[ThrownProjectile] Rigidbody가 없습니다!");
                return;
            }

            // Collider 활성화
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = true;
                col.isTrigger = false;
            }

            // Rigidbody 활성화 - 중요!
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // 속도 설정
            _rb.linearVelocity = velocity;

            // 회전 추가 (날아가는 느낌)
            _rb.angularVelocity = Random.insideUnitSphere * 3f;

            _hasLaunched = true;
            _launchTime = Time.time;

            // ItemBase 비활성화 (떠다니는 애니메이션 정지)
            if (_itemBase != null)
            {
                _itemBase.enabled = false;
            }

            Debug.Log($"[ThrownProjectile] 발사! velocity={velocity.magnitude:F2}, isKinematic={_rb.isKinematic}, useGravity={_rb.useGravity}");
        }

        private void FixedUpdate()
        {
            if (!_hasLaunched) return;

            // 일정 시간 후 속도가 매우 낮으면 정착
            if (Time.time - _launchTime > settleDelay)
            {
                if (_rb.linearVelocity.magnitude < groundStopVelocity)
                {
                    SettleOnGround();
                }
            }
        }

        /// <summary>
        /// 바닥에 정착
        /// </summary>
        private void SettleOnGround()
        {
            _hasLaunched = false;

            // Rigidbody 정지
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;

            // Collider 활성화 유지 (다시 주울 수 있게)
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = true;
            }

            // ItemBase는 활성화하지 않음 (떠다니는 애니메이션 방지)
            // PlayerInteraction은 Collider로 감지하므로 문제없음

            OnGroundHit?.Invoke();

            // 이 컴포넌트 제거
            Destroy(this);

            Debug.Log("[ThrownProjectile] 바닥에 정착! ItemBase는 비활성 상태 유지");
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_hasLaunched) return;

            // 적과 충돌
            if (collision.gameObject.CompareTag("Enemy"))
            {
                HandleEnemyHit(collision.gameObject);
            }
            // 바닥/벽과 충돌 - 자연스럽게 튕기도록 속도 조절 안함
            else
            {
                // 충돌음만 재생
                if (groundHitSound != null && _rb.linearVelocity.magnitude > 2f)
                {
                    AudioSource.PlayClipAtPoint(groundHitSound, transform.position, 0.3f);
                }
            }
        }

        /// <summary>
        /// 적과 충돌 처리
        /// </summary>
        private void HandleEnemyHit(GameObject enemy)
        {
            Debug.Log($"[ThrownProjectile] 적에게 명중! {enemy.name}");

            // 충격력 적용
            Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 forceDirection = (enemy.transform.position - transform.position).normalized;
                enemyRb.AddForce(forceDirection * impactForce, ForceMode.Impulse);
            }

            // 사운드 재생
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position);
            }

            OnEnemyHit?.Invoke(enemy);
        }
    }
}
