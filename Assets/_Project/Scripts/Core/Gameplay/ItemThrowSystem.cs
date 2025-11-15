using UnityEngine;
using UnityEngine.Events;
using StarterAssets;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 들고 있는 아이템을 던지는 시스템
    /// </summary>
    [RequireComponent(typeof(ItemCarrySystem))]
    [RequireComponent(typeof(StarterAssetsInputs))]
    public class ItemThrowSystem : MonoBehaviour
    {
        [Header("Throw Settings")]
        [SerializeField] private float throwForce = 15f;
        [SerializeField] private float throwAngle = 30f; // 위로 던지는 각도
        [SerializeField] private bool useMouseAim = true;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private bool autoFindCamera = true;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private float throwReleaseTime = 1.5f; // 애니메이션 중 물체를 놓는 타이밍 (던지기 모션 중간)

        [Header("Audio")]
        [SerializeField] private AudioClip throwSound; // 던지기 사운드
        [SerializeField] private float throwVolume = 0.5f; // 던지기 사운드 볼륨

        [Header("Weapon Projectiles")]
        [SerializeField] private GameObject eraserBombProjectilePrefab; // 지우개폭탄 발사체 프리팹

        [Header("Throw Statistics")]
        [SerializeField] private int totalThrowCount = 0;

        [Header("Events")]
        public UnityEvent<ItemBase> OnItemThrown;

        [Header("Control")]
        [SerializeField] private bool isEnabled = true; // 컨트롤러 활성화 상태

        private ItemCarrySystem _carrySystem;
        private StarterAssetsInputs _input;
        private TrajectoryPredictor _trajectoryPredictor;
        private bool _isThrowingInProgress = false;

        private void Awake()
        {
            Debug.Log("[ItemThrowSystem] ========== Awake 호출! ==========");
            _carrySystem = GetComponent<ItemCarrySystem>();
            _input = GetComponent<StarterAssetsInputs>();
            _trajectoryPredictor = GetComponent<TrajectoryPredictor>();

            Debug.Log($"[ItemThrowSystem] _carrySystem={_carrySystem != null}, _input={_input != null}, eraserBombPrefab={eraserBombProjectilePrefab != null}");

            // Animator 자동 찾기 (Inspector에서 할당 안 했으면)
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                {
                    Debug.LogWarning("[ItemThrowSystem] Animator를 찾을 수 없습니다!");
                }
            }

            if (autoFindCamera && cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
                if (cameraTransform == null)
                {
                    Debug.LogWarning("[ItemThrowSystem] 카메라를 찾을 수 없습니다!");
                }
            }
        }

        private void Start()
        {
            // ItemCarrySystem 이벤트 연결
            if (_carrySystem != null)
            {
                _carrySystem.OnItemPickedUp.AddListener(OnItemPickedUp);
                _carrySystem.OnItemDropped.AddListener(OnItemDropped);
            }
        }

        /// <summary>
        /// 아이템을 주웠을 때 - 근접 무기면 던지기 비활성화
        /// </summary>
        private void OnItemPickedUp(ItemBase item)
        {
            // 근접 무기 아이템인지 확인
            MeleeWeaponItem meleeWeapon = item.GetComponent<MeleeWeaponItem>();
            if (meleeWeapon != null)
            {
                // 근접 무기는 던지기 불가
                isEnabled = false;
                Debug.Log("[ItemThrowSystem] 근접 무기 감지 - 던지기 시스템 비활성화");
            }
            else
            {
                // 던질 수 있는 아이템
                isEnabled = true;
                Debug.Log("[ItemThrowSystem] 던질 수 있는 아이템 - 던지기 시스템 활성화");
            }
        }

        /// <summary>
        /// 아이템을 떨어뜨렸을 때
        /// </summary>
        private void OnItemDropped(ItemBase item)
        {
            // 아이템을 떨어뜨리면 던지기 시스템 비활성화
            isEnabled = false;
            Debug.Log("[ItemThrowSystem] 아이템 떨어뜨림 - 던지기 시스템 비활성화");
        }

        private float _debugTimer = 0f;

        private void Update()
        {
            // 던지기 시스템이 비활성화되어 있으면 리턴
            if (!isEnabled) return;

            // 디버그: 5초마다 로그
            _debugTimer += Time.deltaTime;
            if (_debugTimer >= 5f)
            {
                Debug.Log("[ItemThrowSystem] Update 호출 중! (5초마다)");
                _debugTimer = 0f;
            }

            // 던지기 입력 처리 (마우스 왼쪽 클릭)
            if (Input.GetMouseButtonDown(0)) // 마우스 좌클릭
            {
                Debug.Log("[ItemThrowSystem] 마우스 좌클릭 감지 - 던지기 시도!");
                TryThrowItem();
            }

            // 궤적 미리보기 업데이트
            if (_trajectoryPredictor != null && _carrySystem.IsCarryingItem)
            {
                Vector3 throwDirection = CalculateThrowDirection();
                Vector3 throwVelocity = throwDirection * throwForce;
                _trajectoryPredictor.ShowTrajectory(_carrySystem.CurrentItem.Transform.position, throwVelocity);
            }
            else if (_trajectoryPredictor != null)
            {
                _trajectoryPredictor.HideTrajectory();
            }
        }

        /// <summary>
        /// 아이템 던지기 시도
        /// </summary>
        private void TryThrowItem()
        {
            Debug.Log("[ItemThrowSystem] TryThrowItem 호출!");

            if (!isEnabled)
            {
                Debug.Log("[ItemThrowSystem] 던지기 시스템이 비활성화되어 있습니다!");
                return;
            }

            if (!_carrySystem.IsCarryingItem)
            {
                Debug.Log("[ItemThrowSystem] 들고 있는 아이템이 없습니다!");
                return;
            }

            if (_isThrowingInProgress)
            {
                Debug.Log("[ItemThrowSystem] 이미 던지는 중입니다!");
                return;
            }

            // 코루틴 시작
            Debug.Log("[ItemThrowSystem] ThrowWithAnimation 코루틴 시작!");
            StartCoroutine(ThrowWithAnimation());
        }

        /// <summary>
        /// 애니메이션과 함께 던지기 실행 (애니메이션 중간에 물체 던짐)
        /// </summary>
        private System.Collections.IEnumerator ThrowWithAnimation()
        {
            _isThrowingInProgress = true;

            // 던지기 애니메이션 트리거
            if (animator != null)
            {
                animator.SetTrigger("Throw");
                Debug.Log($"[ItemThrowSystem] 던지기 애니메이션 시작! {throwReleaseTime}초 후 물체 던짐");
            }

            // 애니메이션 진행 중 물체를 놓는 타이밍까지 대기
            yield return new WaitForSeconds(throwReleaseTime);

            // 실제 던지기 실행 (애니메이션 중간)
            PerformThrow();

            _isThrowingInProgress = false;
        }

        /// <summary>
        /// 실제 던지기 실행
        /// </summary>
        private void PerformThrow()
        {
            if (!_carrySystem.IsCarryingItem)
            {
                Debug.Log("[ItemThrowSystem] 던질 아이템이 없습니다!");
                return;
            }

            Debug.Log("[ItemThrowSystem] PerformThrow 호출! 이제 물체를 던집니다!");

            // 던지기 사운드 재생
            if (throwSound != null)
            {
                AudioSource.PlayClipAtPoint(throwSound, transform.position, throwVolume);
            }

            // 아이템 놓기
            ItemBase itemToThrow = _carrySystem.ReleaseItem();

            // 궤적선 즉시 숨기기
            if (_trajectoryPredictor != null)
            {
                _trajectoryPredictor.HideTrajectory();
            }

            // 던지는 방향 계산
            Vector3 throwDirection = CalculateThrowDirection();

            // 지우개폭탄인지 확인
            WeaponPartItem weaponPart = itemToThrow.GetComponent<WeaponPartItem>();
            bool isEraserBomb = weaponPart != null && weaponPart.PartType == WeaponPartType.EraserBomb;

            Debug.Log($"[ItemThrowSystem] weaponPart={weaponPart != null}, isEraserBomb={isEraserBomb}, prefab={eraserBombProjectilePrefab != null}");
            if (weaponPart != null) Debug.Log($"[ItemThrowSystem] PartType={weaponPart.PartType}");

            if (isEraserBomb && eraserBombProjectilePrefab != null)
            {
                // 지우개폭탄 발사체 생성
                Vector3 spawnPosition = itemToThrow.transform.position;
                Debug.Log($"[ItemThrowSystem] 지우개폭탄 프리팹 생성 시작! 위치: {spawnPosition}");

                GameObject projectileObj = Instantiate(eraserBombProjectilePrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"[ItemThrowSystem] 프리팹 생성 완료! 오브젝트: {projectileObj.name}");

                // Rigidbody에 힘 적용
                Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = throwDirection * throwForce;
                    Debug.Log($"[ItemThrowSystem] 속도 설정: {rb.linearVelocity}");
                }

                // 원래 아이템 제거
                Destroy(itemToThrow.gameObject);

                Debug.Log($"[ItemThrowSystem] 지우개폭탄 발사! 방향: {throwDirection}, 힘: {throwForce}");
            }
            else
            {
                // 일반 아이템: ThrownProjectile 컴포넌트 추가
                ThrownProjectile projectile = itemToThrow.GetComponent<ThrownProjectile>();
                if (projectile == null)
                {
                    projectile = itemToThrow.gameObject.AddComponent<ThrownProjectile>();
                }

                // 던지기
                projectile.Launch(throwDirection * throwForce);
            }

            // 통계 업데이트
            totalThrowCount++;

            // InventoryManager에 통계 업데이트 (weaponPart는 위에서 이미 선언됨)
            if (InventoryManager.Instance != null && weaponPart != null)
            {
                InventoryManager.Instance.AddThrowCount(weaponPart.PartType, 1);
                Debug.Log($"[ItemThrowSystem] {weaponPart.PartType} 던지기! (총 {totalThrowCount}회)");
            }
            else
            {
                Debug.Log($"[ItemThrowSystem] 아이템 던지기! (총 {totalThrowCount}회)");
            }

            OnItemThrown?.Invoke(itemToThrow);
        }

        /// <summary>
        /// 던지는 방향 계산
        /// </summary>
        private Vector3 CalculateThrowDirection()
        {
            Vector3 direction;

            if (useMouseAim && cameraTransform != null)
            {
                // 카메라가 바라보는 방향
                direction = cameraTransform.forward;
            }
            else
            {
                // 플레이어가 바라보는 방향
                direction = transform.forward;
            }

            // 위로 각도 추가 (포물선)
            direction = Quaternion.AngleAxis(-throwAngle, cameraTransform != null ? cameraTransform.right : transform.right) * direction;

            return direction.normalized;
        }

        /// <summary>
        /// 던지는 힘 설정
        /// </summary>
        public void SetThrowForce(float force)
        {
            throwForce = Mathf.Max(1f, force);
        }

        /// <summary>
        /// 던지는 각도 설정
        /// </summary>
        public void SetThrowAngle(float angle)
        {
            throwAngle = Mathf.Clamp(angle, 0f, 89f);
        }

        /// <summary>
        /// 총 던진 횟수 반환
        /// </summary>
        public int GetTotalThrowCount()
        {
            return totalThrowCount;
        }

        /// <summary>
        /// 애니메이션 이벤트에서 호출됨 (던지기 애니메이션 중 아이템을 놓는 타이밍)
        /// </summary>
        public void OnThrowAnimationEvent()
        {
            // 애니메이션 이벤트로 던지기가 호출된 경우도 throwReleaseTime만큼 딜레이 적용
            if (_carrySystem.IsCarryingItem && !_isThrowingInProgress)
            {
                StartCoroutine(DelayedThrow());
            }
        }

        /// <summary>
        /// 딜레이 후 던지기
        /// </summary>
        private System.Collections.IEnumerator DelayedThrow()
        {
            Debug.Log($"[ItemThrowSystem] DelayedThrow 시작! {throwReleaseTime}초 대기");
            _isThrowingInProgress = true;
            yield return new WaitForSeconds(throwReleaseTime);

            Debug.Log("[ItemThrowSystem] 대기 완료, PerformThrow 호출!");
            if (_carrySystem.IsCarryingItem)
            {
                PerformThrow();
            }
            else
            {
                Debug.Log("[ItemThrowSystem] 아이템이 없어서 던지기 취소!");
            }

            _isThrowingInProgress = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (_carrySystem != null && _carrySystem.IsCarryingItem && cameraTransform != null)
            {
                // 던지는 방향 표시
                Vector3 throwDirection = CalculateThrowDirection();
                Vector3 startPos = _carrySystem.CurrentItem.Transform.position;
                Gizmos.color = Color.red;
                Gizmos.DrawRay(startPos, throwDirection * 3f);
            }
        }
    }
}
