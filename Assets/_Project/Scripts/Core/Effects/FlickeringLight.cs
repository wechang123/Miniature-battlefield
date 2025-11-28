using UnityEngine;
using System.Collections;

namespace YajaGame.Effects
{
    /// <summary>
    /// 형광등 깜빡임 효과
    /// 학교 복도 분위기 연출
    /// </summary>
    public class FlickeringLight : MonoBehaviour
    {
        [Header("Light Reference")]
        [SerializeField] private Light targetLight;

        [Header("Flicker Settings")]
        [SerializeField] private bool enableFlicker = true;
        [SerializeField] private float minIntensity = 0.5f;
        [SerializeField] private float maxIntensity = 1.5f;
        [SerializeField] private float flickerSpeed = 0.1f; // 깜빡임 간격

        [Header("Flicker Pattern")]
        [SerializeField] private FlickerMode flickerMode = FlickerMode.Random;
        [SerializeField] private float normalOnTime = 3f; // 정상 켜짐 시간
        [SerializeField] private float flickerDuration = 0.5f; // 깜빡이는 시간

        [Header("Sound (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip flickerSound;
        [SerializeField] private float soundVolume = 0.3f;

        public enum FlickerMode
        {
            Random,         // 완전 랜덤
            Periodic,       // 주기적으로 깜빡임
            Broken          // 고장난 형광등 (계속 깜빡임)
        }

        private float _baseIntensity;
        private bool _isFlickering = false;
        private Coroutine _flickerCoroutine;

        private void Awake()
        {
            if (targetLight == null)
            {
                targetLight = GetComponent<Light>();
            }

            if (targetLight != null)
            {
                _baseIntensity = targetLight.intensity;
            }
        }

        private void Start()
        {
            if (enableFlicker && targetLight != null)
            {
                StartFlickering();
            }
        }

        private void StartFlickering()
        {
            if (_flickerCoroutine != null)
            {
                StopCoroutine(_flickerCoroutine);
            }

            switch (flickerMode)
            {
                case FlickerMode.Random:
                    _flickerCoroutine = StartCoroutine(RandomFlickerCoroutine());
                    break;
                case FlickerMode.Periodic:
                    _flickerCoroutine = StartCoroutine(PeriodicFlickerCoroutine());
                    break;
                case FlickerMode.Broken:
                    _flickerCoroutine = StartCoroutine(BrokenFlickerCoroutine());
                    break;
            }
        }

        /// <summary>
        /// 랜덤 깜빡임
        /// </summary>
        private IEnumerator RandomFlickerCoroutine()
        {
            while (true)
            {
                // 정상 상태 유지
                targetLight.intensity = _baseIntensity;
                yield return new WaitForSeconds(Random.Range(2f, 8f));

                // 깜빡임 시작
                PlayFlickerSound();
                float flickerTime = Random.Range(0.2f, 0.8f);
                float elapsed = 0f;

                while (elapsed < flickerTime)
                {
                    targetLight.intensity = Random.Range(minIntensity, maxIntensity);
                    yield return new WaitForSeconds(flickerSpeed);
                    elapsed += flickerSpeed;
                }

                // 잠깐 꺼짐
                if (Random.value > 0.7f)
                {
                    targetLight.intensity = 0f;
                    yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                }
            }
        }

        /// <summary>
        /// 주기적 깜빡임
        /// </summary>
        private IEnumerator PeriodicFlickerCoroutine()
        {
            while (true)
            {
                // 정상 켜짐
                targetLight.intensity = _baseIntensity;
                yield return new WaitForSeconds(normalOnTime);

                // 깜빡임
                PlayFlickerSound();
                float elapsed = 0f;
                while (elapsed < flickerDuration)
                {
                    targetLight.intensity = Random.Range(minIntensity, maxIntensity);
                    yield return new WaitForSeconds(flickerSpeed);
                    elapsed += flickerSpeed;
                }
            }
        }

        /// <summary>
        /// 고장난 형광등 (계속 깜빡임)
        /// </summary>
        private IEnumerator BrokenFlickerCoroutine()
        {
            while (true)
            {
                targetLight.intensity = Random.Range(minIntensity, maxIntensity);

                // 가끔 완전히 꺼짐
                if (Random.value > 0.9f)
                {
                    targetLight.intensity = 0f;
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
                }

                yield return new WaitForSeconds(flickerSpeed);
            }
        }

        private void PlayFlickerSound()
        {
            if (flickerSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(flickerSound, soundVolume);
            }
        }

        /// <summary>
        /// 깜빡임 켜기/끄기
        /// </summary>
        public void SetFlickerEnabled(bool enabled)
        {
            enableFlicker = enabled;

            if (enabled)
            {
                StartFlickering();
            }
            else
            {
                if (_flickerCoroutine != null)
                {
                    StopCoroutine(_flickerCoroutine);
                }
                if (targetLight != null)
                {
                    targetLight.intensity = _baseIntensity;
                }
            }
        }

        /// <summary>
        /// 완전히 끄기
        /// </summary>
        public void TurnOff()
        {
            SetFlickerEnabled(false);
            if (targetLight != null)
            {
                targetLight.intensity = 0f;
            }
        }

        /// <summary>
        /// 완전히 켜기
        /// </summary>
        public void TurnOn()
        {
            if (targetLight != null)
            {
                targetLight.intensity = _baseIntensity;
            }
        }

        private void OnDisable()
        {
            if (_flickerCoroutine != null)
            {
                StopCoroutine(_flickerCoroutine);
            }
        }
    }
}
