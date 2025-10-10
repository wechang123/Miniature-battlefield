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
        [SerializeField] private float damage = 10f;
        [SerializeField] private float impactForce = 5f;
        [SerializeField] private bool destroyOnImpact = false;

        [Header("Ground Detection")]
        [SerializeField] private float groundStopVelocity = 1f; // 이 속도 이하면 바닥에 정착
        [SerializeField] private float groundCheckDelay = 0.5f; // 던진 후 지면 체크까지 딜레이

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

            // Rigidbody 설정
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // 속도 설정
            _rb.velocity = velocity;

            // 회전 추가 (날아가는 느낌)
            _rb.angularVelocity = Random.insideUnitSphere * 5f;

            _hasLaunched = true;
            _launchTime = Time.time;

            // ItemBase 비활성화 (떠다니는 애니메이션 정지)
            if (_itemBase != null)
            {
                _itemBase.enabled = false;
            }

            Debug.Log($"[ThrownProjectile] 발사! 속도: {velocity}");
        }

        private void FixedUpdate()
        {
            if (!_hasLaunched) return;

            // 바닥에 정착 체크
            if (Time.time - _launchTime > groundCheckDelay)
            {
                CheckGroundSettle();
            }
        }

        /// <summary>
        /// 바닥에 정착했는지 체크
        /// </summary>
        private void CheckGroundSettle()
        {
            if (_rb.velocity.magnitude < groundStopVelocity)
            {
                // 바닥에 정착
                SettleOnGround();
            }
        }

        /// <summary>
        /// 바닥에 정착
        /// </summary>
        private void SettleOnGround()
        {
            _hasLaunched = false;

            // Rigidbody 정지
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;

            // ItemBase 다시 활성화 (다시 주울 수 있게)
            if (_itemBase != null)
            {
                _itemBase.enabled = true;
            }

            // 사운드 재생
            if (groundHitSound != null)
            {
                AudioSource.PlayClipAtPoint(groundHitSound, transform.position);
            }

            OnGroundHit?.Invoke();

            // 이 컴포넌트 제거
            Destroy(this);

            Debug.Log("[ThrownProjectile] 바닥에 정착!");
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_hasLaunched) return;

            // 적과 충돌
            if (collision.gameObject.CompareTag("Enemy"))
            {
                HandleEnemyHit(collision.gameObject);
            }
            // 바닥/벽과 충돌
            else
            {
                HandleGroundHit(collision);
            }
        }

        /// <summary>
        /// 적과 충돌 처리
        /// </summary>
        private void HandleEnemyHit(GameObject enemy)
        {
            Debug.Log($"[ThrownProjectile] 적에게 명중! {enemy.name}");

            // 데미지 적용 (적이 데미지를 받는 컴포넌트가 있다면)
            // 예시: enemy.GetComponent<EnemyHealth>()?.TakeDamage(damage);

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

            // 파괴 또는 바닥에 떨어뜨리기
            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
            else
            {
                // 속도 줄이기 (바닥에 떨어지도록)
                _rb.velocity *= 0.3f;
            }
        }

        /// <summary>
        /// 바닥/벽 충돌 처리
        /// </summary>
        private void HandleGroundHit(Collision collision)
        {
            // 사운드 재생
            if (groundHitSound != null && _rb.velocity.magnitude > 3f)
            {
                AudioSource.PlayClipAtPoint(groundHitSound, transform.position, 0.5f);
            }

            // 속도가 충분히 낮으면 정착
            if (_rb.velocity.magnitude < groundStopVelocity * 2f)
            {
                SettleOnGround();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_hasLaunched && _rb != null)
            {
                // 속도 방향 표시
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(transform.position, _rb.velocity);
            }
        }
    }
}
