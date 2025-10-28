using UnityEngine;
using UnityEngine.AI;

namespace StarterAssets
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class TeacherPatrolAI : MonoBehaviour
    {
        [Header("Patrol Settings")]
        [Tooltip("순찰 포인트들 - 이 순서대로 이동합니다")]
        public Transform[] patrolPoints;

        [Tooltip("각 포인트에서 대기하는 시간 (초)")]
        public float waitTimeAtPoint = 2.0f;

        [Tooltip("걷는 속도")]
        public float walkSpeed = 1.5f;

        [Header("Animation")]
        [Tooltip("걷기 속도 파라미터 이름")]
        public string speedParameterName = "Speed";

        [Tooltip("이동 속도 파라미터 이름")]
        public string motionSpeedParameterName = "MotionSpeed";

        private NavMeshAgent agent;
        private Animator animator;
        private int currentPatrolIndex = 0;
        private float waitTimer = 0f;
        private bool isWaiting = false;

        // Animator 파라미터 해시 (성능 최적화)
        private int animIDSpeed;
        private int animIDMotionSpeed;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            // Animator 파라미터 해시 생성
            animIDSpeed = Animator.StringToHash(speedParameterName);
            animIDMotionSpeed = Animator.StringToHash(motionSpeedParameterName);
        }

        private void Start()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                Debug.LogError("TeacherPatrolAI: 순찰 포인트가 설정되지 않았습니다!");
                enabled = false;
                return;
            }

            // NavMeshAgent 설정
            agent.speed = walkSpeed;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;

            // 첫 번째 포인트로 이동 시작
            MoveToNextPatrolPoint();
        }

        private void Update()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            // 대기 중이라면
            if (isWaiting)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTimeAtPoint)
                {
                    isWaiting = false;
                    waitTimer = 0f;
                    MoveToNextPatrolPoint();
                }

                // 대기 중에는 애니메이션 정지
                UpdateAnimation(0f);
                return;
            }

            // 목적지에 도착했는지 확인
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    // 도착했으면 대기 시작
                    isWaiting = true;
                    UpdateAnimation(0f);
                }
            }
            else
            {
                // 이동 중이면 애니메이션 업데이트
                UpdateAnimation(agent.velocity.magnitude);
            }
        }

        private void MoveToNextPatrolPoint()
        {
            if (patrolPoints.Length == 0) return;

            // 다음 순찰 포인트로 이동
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);

            // 다음 인덱스로 이동 (순환)
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        private void UpdateAnimation(float speed)
        {
            // 애니메이터에 속도 전달
            animator.SetFloat(animIDSpeed, speed);
            animator.SetFloat(animIDMotionSpeed, 1f);
        }

        private void OnDrawGizmosSelected()
        {
            // 에디터에서 순찰 경로 시각화
            if (patrolPoints == null || patrolPoints.Length < 2) return;

            Gizmos.color = Color.yellow;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;

                // 순찰 포인트 그리기
                Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);

                // 다음 포인트로 선 그리기
                int nextIndex = (i + 1) % patrolPoints.Length;
                if (patrolPoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                }
            }
        }
    }
}
