using UnityEngine;
using System.Collections;

namespace YajaGame.Effects
{
    /// <summary>
    /// 카메라 쉐이크 효과
    /// 싱글톤으로 어디서든 호출 가능
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [Header("Default Settings")]
        [SerializeField] private float defaultDuration = 0.2f;
        [SerializeField] private float defaultMagnitude = 0.1f;

        private Vector3 _originalPosition;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _originalPosition = transform.localPosition;
        }

        /// <summary>
        /// 기본 설정으로 쉐이크
        /// </summary>
        public void Shake()
        {
            Shake(defaultDuration, defaultMagnitude);
        }

        /// <summary>
        /// 커스텀 설정으로 쉐이크
        /// </summary>
        public void Shake(float duration, float magnitude)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }
            _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        /// <summary>
        /// 타격 쉐이크 (약함)
        /// </summary>
        public void ShakeOnHit()
        {
            Shake(0.1f, 0.05f);
        }

        /// <summary>
        /// 폭발 쉐이크 (강함)
        /// </summary>
        public void ShakeOnExplosion()
        {
            Shake(0.3f, 0.2f);
        }

        /// <summary>
        /// 피격 쉐이크 (중간)
        /// </summary>
        public void ShakeOnDamage()
        {
            Shake(0.15f, 0.1f);
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = _originalPosition + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = _originalPosition;
            _shakeCoroutine = null;
        }

        [ContextMenu("Test Shake")]
        public void TestShake()
        {
            Shake();
        }

        [ContextMenu("Test Explosion Shake")]
        public void TestExplosionShake()
        {
            ShakeOnExplosion();
        }
    }
}
