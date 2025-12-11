using UnityEngine;
using System.Collections;

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
        private Coroutine _transitionCoroutine;

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

        private void LateUpdate()
        {
            // 깜빡임 효과만 Update에서 처리 (부드러운 효과를 위해)
            if (enableFlicker)
            {
                float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) * flickerAmount;
                spotLight.intensity = _baseIntensity + flicker;
            }

            // 손전등 위치/방향 업데이트 (LateUpdate에서 처리하여 성능 향상)
            if (spotLight.transform.parent == transform)
            {
                spotLight.transform.localPosition = lightOffset;
                spotLight.transform.forward = transform.forward;
            }
        }

        /// <summary>
        /// 라이트 속성 부드럽게 전환하는 코루틴
        /// </summary>
        private IEnumerator TransitionLightProperties(float targetIntensity, float targetRange, float targetAngle, Color targetColor)
        {
            float startIntensity = spotLight.intensity;
            float startRange = spotLight.range;
            float startAngle = spotLight.spotAngle;
            Color startColor = spotLight.color;

            float transitionTime = 0f;
            float duration = 0.5f; // 전환 시간

            while (transitionTime < duration)
            {
                transitionTime += Time.deltaTime;
                float t = transitionTime / duration;

                spotLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
                spotLight.range = Mathf.Lerp(startRange, targetRange, t);
                spotLight.spotAngle = Mathf.Lerp(startAngle, targetAngle, t);
                spotLight.color = Color.Lerp(startColor, targetColor, t);

                yield return null;
            }

            // 최종 값 설정
            spotLight.intensity = targetIntensity;
            spotLight.range = targetRange;
            spotLight.spotAngle = targetAngle;
            spotLight.color = targetColor;
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

            // 기존 전환 코루틴 중지하고 새로운 전환 시작
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);
            
            _transitionCoroutine = StartCoroutine(TransitionLightProperties(patrolIntensity, patrolRange, patrolAngle, patrolColor));

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

            // 기존 전환 코루틴 중지하고 새로운 전환 시작
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);
            
            _transitionCoroutine = StartCoroutine(TransitionLightProperties(chaseIntensity, chaseRange, chaseAngle, chaseColor));

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
