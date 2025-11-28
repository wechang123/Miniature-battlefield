using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YajaGame.Gameplay.Combat;

namespace YajaGame.UI
{
    /// <summary>
    /// 플레이어 HP바 UI
    /// </summary>
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Colors")]
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color damagedColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField] private float damagedThreshold = 0.5f;  // 50% 이하
        [SerializeField] private float criticalThreshold = 0.25f; // 25% 이하

        [Header("Animation")]
        [SerializeField] private bool animateChanges = true;
        [SerializeField] private float animationSpeed = 5f;

        private float _targetValue = 1f;

        private void Start()
        {
            // PlayerHealth 자동 찾기
            if (playerHealth == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerHealth = player.GetComponent<PlayerHealth>();
                    Debug.Log("[PlayerHealthUI] PlayerHealth 자동 연결 성공");
                }
            }

            // 이벤트 구독
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged.AddListener(OnHealthChanged);

                // 초기값 설정
                UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
                Debug.Log("[PlayerHealthUI] 초기화 완료");
            }
            else
            {
                Debug.LogError("[PlayerHealthUI] PlayerHealth를 찾을 수 없습니다! Player 태그 확인 필요");
            }

            // 연결 상태 확인
            if (healthSlider == null)
                Debug.LogError("[PlayerHealthUI] healthSlider가 연결되지 않았습니다!");
            if (healthText == null)
                Debug.LogError("[PlayerHealthUI] healthText가 연결되지 않았습니다! Inspector에서 연결해주세요.");
            if (fillImage == null)
                Debug.LogWarning("[PlayerHealthUI] fillImage가 연결되지 않았습니다.");

            // 슬라이더 초기 설정
            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
                healthSlider.value = 1f;
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged.RemoveListener(OnHealthChanged);
            }
        }

        private void Update()
        {
            // 부드러운 애니메이션
            if (animateChanges && healthSlider != null)
            {
                if (Mathf.Abs(healthSlider.value - _targetValue) > 0.001f)
                {
                    healthSlider.value = Mathf.Lerp(healthSlider.value, _targetValue, Time.deltaTime * animationSpeed);
                }
            }
        }

        /// <summary>
        /// HP 변경 이벤트 핸들러
        /// </summary>
        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            UpdateHealthBar(currentHealth, maxHealth);
        }

        /// <summary>
        /// HP바 업데이트
        /// </summary>
        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            float healthPercent = maxHealth > 0 ? currentHealth / maxHealth : 0f;
            _targetValue = healthPercent;

            // 애니메이션 없이 즉시 적용
            if (!animateChanges && healthSlider != null)
            {
                healthSlider.value = healthPercent;
            }

            // 텍스트 업데이트
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
            }

            // 색상 업데이트
            UpdateHealthColor(healthPercent);
        }

        /// <summary>
        /// HP 비율에 따른 색상 변경
        /// </summary>
        private void UpdateHealthColor(float healthPercent)
        {
            if (fillImage == null) return;

            if (healthPercent <= criticalThreshold)
            {
                fillImage.color = criticalColor;
            }
            else if (healthPercent <= damagedThreshold)
            {
                fillImage.color = damagedColor;
            }
            else
            {
                fillImage.color = healthyColor;
            }
        }

        #region Debug

        [ContextMenu("Test Low Health")]
        public void TestLowHealth()
        {
            UpdateHealthBar(20f, 100f);
        }

        [ContextMenu("Test Full Health")]
        public void TestFullHealth()
        {
            UpdateHealthBar(100f, 100f);
        }

        #endregion
    }
}
