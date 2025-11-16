using UnityEngine;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 던지기 궤적을 미리 보여주는 시스템
    /// </summary>
    public class TrajectoryPredictor : MonoBehaviour
    {
        [Header("Line Settings")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private int lineSegments = 30;
        [SerializeField] private float timeStep = 0.1f;
        
        [Header("Visual")]
        [SerializeField] private bool showTrajectory = true;
        [SerializeField] private Color trajectoryColor = Color.yellow;
        [SerializeField] private float lineWidth = 0.05f;
        
        private void Awake()
        {
            // LineRenderer 자동 생성
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                SetupLineRenderer();
            }
        }
        
        private void SetupLineRenderer()
        {
            lineRenderer.positionCount = lineSegments;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = trajectoryColor;
            lineRenderer.endColor = trajectoryColor;
            lineRenderer.enabled = false;
        }
        
        /// <summary>
        /// 궤적 표시
        /// </summary>
        public void ShowTrajectory(Vector3 startPosition, Vector3 initialVelocity)
        {
            if (!showTrajectory || lineRenderer == null)
                return;
            
            lineRenderer.enabled = true;
            
            Vector3[] points = new Vector3[lineSegments];
            
            for (int i = 0; i < lineSegments; i++)
            {
                float time = i * timeStep;
                points[i] = CalculatePositionAtTime(startPosition, initialVelocity, time);
            }
            
            lineRenderer.positionCount = lineSegments;
            lineRenderer.SetPositions(points);
        }
        
        /// <summary>
        /// 궤적 숨기기
        /// </summary>
        public void HideTrajectory()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }
        
        /// <summary>
        /// 특정 시간에서의 위치 계산
        /// </summary>
        private Vector3 CalculatePositionAtTime(Vector3 startPos, Vector3 velocity, float time)
        {
            Vector3 gravity = Physics.gravity;
            return startPos + velocity * time + 0.5f * gravity * time * time;
        }
        
        /// <summary>
        /// 궤적 색상 변경
        /// </summary>
        public void SetTrajectoryColor(Color color)
        {
            trajectoryColor = color;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
        }
        
        /// <summary>
        /// 궤적 표시 토글
        /// </summary>
        public void ToggleTrajectory(bool show)
        {
            showTrajectory = show;
            if (!show)
            {
                HideTrajectory();
            }
        }
    }
}