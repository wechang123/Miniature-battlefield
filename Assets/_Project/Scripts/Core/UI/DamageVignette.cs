using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using YajaGame.Gameplay.Combat;

namespace YajaGame.UI
{
    /// <summary>
    /// 피격 시 빨간 화면 효과 (Vignette)
    /// </summary>
    public class DamageVignette : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image vignetteImage;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Settings")]
        [SerializeField] private Color damageColor = new Color(1f, 0f, 0f, 0.5f);
        [SerializeField] private float flashDuration = 0.3f;
        [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        private Coroutine _flashCoroutine;

        private void Start()
        {
            // PlayerHealth 자동 찾기
            if (playerHealth == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerHealth = player.GetComponent<PlayerHealth>();
                    Debug.Log("[DamageVignette] PlayerHealth 자동 연결 성공");
                }
            }

            // 연결 상태 확인
            if (vignetteImage == null)
            {
                Debug.LogError("[DamageVignette] vignetteImage가 연결되지 않았습니다! Inspector에서 Image를 연결해주세요.");
            }

            // 이벤트 구독
            if (playerHealth != null)
            {
                playerHealth.OnDamageTaken.AddListener(OnPlayerDamaged);
                Debug.Log("[DamageVignette] 초기화 완료 - 피격 이벤트 구독됨");
            }
            else
            {
                Debug.LogError("[DamageVignette] PlayerHealth를 찾을 수 없습니다! Player 태그 확인 필요");
            }

            // 초기 상태: 투명
            if (vignetteImage != null)
            {
                SetAlpha(0f);
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (playerHealth != null)
            {
                playerHealth.OnDamageTaken.RemoveListener(OnPlayerDamaged);
            }
        }

        /// <summary>
        /// 플레이어가 데미지를 받았을 때
        /// </summary>
        private void OnPlayerDamaged(DamageInfo damageInfo)
        {
            Debug.Log("[DamageVignette] 피격 감지! 빨간 화면 효과 시작");
            Flash();
        }

        /// <summary>
        /// 빨간 화면 효과 재생
        /// </summary>
        public void Flash()
        {
            if (vignetteImage == null) return;

            // 기존 코루틴 중지
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                float alpha = flashCurve.Evaluate(t) * damageColor.a;
                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(0f);
            _flashCoroutine = null;
        }

        private void SetAlpha(float alpha)
        {
            if (vignetteImage != null)
            {
                Color color = damageColor;
                color.a = alpha;
                vignetteImage.color = color;
            }
        }

        #region Debug

        [ContextMenu("Test Flash")]
        public void TestFlash()
        {
            Flash();
        }

        #endregion
    }
}
