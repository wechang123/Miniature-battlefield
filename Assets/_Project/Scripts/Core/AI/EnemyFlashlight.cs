using UnityEngine;

namespace YajaGame.AI
{
    /// <summary>
    /// 적(선생님) 손전등
    /// 추적 중에는 더 밝게, 순찰 중에는 약하게
    /// </summary>
    public class EnemyFlashlight : MonoBehaviour
    {
        [Header("Light Reference")]
        [SerializeField] private Light spotLight;

        [Header("Light Settings - Patrol")]
        [SerializeField] private float patrolIntensity = 1.5f;
        [SerializeField] private float patrolRange = 15f;
        [SerializeField] private float patrolAngle = 50f;
        [SerializeField] private Color patrolColor = new Color(1f, 0.95f, 0.8f); // 따뜻한 흰색

        [Header("Light Settings - Chase")]
        [SerializeField] private float chaseIntensity = 3f;
        [SerializeField] private float chaseRange = 25f;
        [SerializeField] private float chaseAngle = 40f;
        [SerializeField] private Color chaseColor = new Color(1f, 0.9f, 0.9f); // 약간 붉은 흰색

        [Header("Flicker Effect")]
        [SerializeField] private bool enableFlicker = true;
        [SerializeField] private float flickerSpeed = 10f;
        [SerializeField] private float flickerAmount = 0.1f;

        [Header("Position Offset")]
        [SerializeField] private Vector3 lightOffset = new Vector3(0f, 1.5f, 0.3f);

        private bool _isChasing = false;
        private float _baseIntensity;
        private float _targetIntensity;
        private float _targetRange;
        private float _targetAngle;
        private Color _targetColor;

        private void Awake()
        {
            // Spot Light 자동 생성 (없으면)
            if (spotLight == null)
            {
                GameObject lightObj = new GameObject("Flashlight");
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = lightOffset;
                lightObj.transform.localRotation = Quaternion.identity;

                spotLight = lightObj.AddComponent<Light>();
                spotLight.type = LightType.Spot;
                spotLight.shadows = LightShadows.Soft;
            }

            SetPatrolMode();
        }

        private void Update()
        {
            // 부드러운 전환
            spotLight.intensity = Mathf.Lerp(spotLight.intensity, _targetIntensity, Time.deltaTime * 3f);
            spotLight.range = Mathf.Lerp(spotLight.range, _targetRange, Time.deltaTime * 3f);
            spotLight.spotAngle = Mathf.Lerp(spotLight.spotAngle, _targetAngle, Time.deltaTime * 3f);
            spotLight.color = Color.Lerp(spotLight.color, _targetColor, Time.deltaTime * 3f);

            // 깜빡임 효과
            if (enableFlicker)
            {
                float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) * flickerAmount;
                spotLight.intensity = _baseIntensity + flicker;
            }

            // 손전등이 앞을 향하도록
            if (spotLight.transform.parent == transform)
            {
                spotLight.transform.localPosition = lightOffset;
                spotLight.transform.forward = transform.forward;
            }
        }

        /// <summary>
        /// 순찰 모드 (약한 빛)
        /// </summary>
        public void SetPatrolMode()
        {
            _isChasing = false;
            _targetIntensity = patrolIntensity;
            _targetRange = patrolRange;
            _targetAngle = patrolAngle;
            _targetColor = patrolColor;
            _baseIntensity = patrolIntensity;

            Debug.Log("[EnemyFlashlight] 순찰 모드");
        }

        /// <summary>
        /// 추적 모드 (강한 빛)
        /// </summary>
        public void SetChaseMode()
        {
            _isChasing = true;
            _targetIntensity = chaseIntensity;
            _targetRange = chaseRange;
            _targetAngle = chaseAngle;
            _targetColor = chaseColor;
            _baseIntensity = chaseIntensity;

            Debug.Log("[EnemyFlashlight] 추적 모드!");
        }

        /// <summary>
        /// 손전등 ON/OFF
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (spotLight != null)
            {
                spotLight.enabled = enabled;
            }
        }

        public bool IsChasing => _isChasing;

        private void OnDrawGizmosSelected()
        {
            // 손전등 범위 시각화
            Gizmos.color = _isChasing ? Color.red : Color.yellow;
            Vector3 lightPos = transform.position + transform.TransformDirection(lightOffset);
            Gizmos.DrawWireSphere(lightPos, 0.1f);
            Gizmos.DrawRay(lightPos, transform.forward * (_isChasing ? chaseRange : patrolRange) * 0.5f);
        }
    }
}
