using UnityEngine;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 던지는 궤적을 미리 보여주는 시스템
    /// LineRenderer를 사용하여 포물선을 그림
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryPredictor : MonoBehaviour
    {
        [Header("Trajectory Settings")]
        [SerializeField] private int pointCount = 30;
        [SerializeField] private float timeStep = 0.1f;
        [SerializeField] private float maxDistance = 20f;

        [Header("Visualization")]
        [SerializeField] private Gradient trajectoryGradient;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private Material lineMaterial;

        [Header("Ground Check")]
        [SerializeField] private bool stopAtGround = true;
        [SerializeField] private LayerMask groundLayer = -1;

        private LineRenderer _lineRenderer;
        private Vector3[] _points;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _points = new Vector3[pointCount];

            SetupLineRenderer();
            HideTrajectory();
        }

        /// <summary>
        /// LineRenderer 초기 설정
        /// </summary>
        private void SetupLineRenderer()
        {
            _lineRenderer.positionCount = pointCount;
            _lineRenderer.startWidth = lineWidth;
            _lineRenderer.endWidth = lineWidth * 0.5f;
            _lineRenderer.useWorldSpace = true;

            // 그라디언트 설정 (빨강 -> 주황)
            if (trajectoryGradient == null || trajectoryGradient.colorKeys.Length == 0)
            {
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[]
                    {
                        new GradientColorKey(Color.red, 0.0f),
                        new GradientColorKey(new Color(1f, 0.5f, 0f), 1.0f) // 주황색
                    },
                    new GradientAlphaKey[]
                    {
                        new GradientAlphaKey(1.0f, 0.0f),
                        new GradientAlphaKey(0.8f, 1.0f)
                    }
                );
                trajectoryGradient = gradient;
            }

            _lineRenderer.colorGradient = trajectoryGradient;

            // Material 설정
            if (lineMaterial != null)
            {
                _lineRenderer.material = lineMaterial;
            }
            else
            {
                // 기본 Material 생성
                _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                _lineRenderer.material.color = Color.red;
            }
        }

        /// <summary>
        /// 궤적 표시
        /// </summary>
        public void ShowTrajectory(Vector3 startPosition, Vector3 initialVelocity)
        {
            _lineRenderer.enabled = true;

            // 궤적 점 계산
            CalculateTrajectoryPoints(startPosition, initialVelocity);

            // LineRenderer에 점 설정
            _lineRenderer.SetPositions(_points);
        }

        /// <summary>
        /// 궤적 숨기기
        /// </summary>
        public void HideTrajectory()
        {
            _lineRenderer.enabled = false;
        }

        /// <summary>
        /// 물리 기반 궤적 점 계산
        /// </summary>
        private void CalculateTrajectoryPoints(Vector3 startPosition, Vector3 initialVelocity)
        {
            Vector3 currentPosition = startPosition;
            Vector3 currentVelocity = initialVelocity;
            float totalDistance = 0f;

            for (int i = 0; i < pointCount; i++)
            {
                _points[i] = currentPosition;

                // 다음 위치 예측
                Vector3 nextVelocity = currentVelocity + Physics.gravity * timeStep;
                Vector3 nextPosition = currentPosition + currentVelocity * timeStep;

                // 거리 체크
                totalDistance += Vector3.Distance(currentPosition, nextPosition);
                if (totalDistance > maxDistance)
                {
                    // 최대 거리 도달 시 남은 점들은 마지막 위치로 설정
                    for (int j = i + 1; j < pointCount; j++)
                    {
                        _points[j] = currentPosition;
                    }
                    break;
                }

                // 지면 충돌 체크
                if (stopAtGround)
                {
                    Vector3 direction = nextPosition - currentPosition;
                    float distance = direction.magnitude;

                    if (Physics.Raycast(currentPosition, direction.normalized, out RaycastHit hit, distance, groundLayer))
                    {
                        // 지면 충돌 시 충돌 지점에서 멈춤
                        for (int j = i + 1; j < pointCount; j++)
                        {
                            _points[j] = hit.point;
                        }
                        break;
                    }
                }

                currentPosition = nextPosition;
                currentVelocity = nextVelocity;
            }
        }

        /// <summary>
        /// 예상 낙하 지점 계산
        /// </summary>
        public Vector3 GetPredictedLandingPoint(Vector3 startPosition, Vector3 initialVelocity)
        {
            CalculateTrajectoryPoints(startPosition, initialVelocity);
            return _points[pointCount - 1];
        }

        /// <summary>
        /// 궤적이 표시 중인지 확인
        /// </summary>
        public bool IsShowing => _lineRenderer.enabled;

        // Inspector에서 설정 변경 시 실시간 업데이트
        private void OnValidate()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = pointCount;
                _lineRenderer.startWidth = lineWidth;
                _lineRenderer.endWidth = lineWidth * 0.5f;

                if (trajectoryGradient != null)
                {
                    _lineRenderer.colorGradient = trajectoryGradient;
                }
            }
        }
    }
}
