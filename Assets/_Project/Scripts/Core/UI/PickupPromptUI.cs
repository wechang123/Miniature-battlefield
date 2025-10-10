using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YajaGame.Gameplay;

namespace YajaGame.UI
{
    /// <summary>
    /// 줍기 프롬프트 UI (E키 표시)
    /// </summary>
    public class PickupPromptUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI promptText = null;
        [SerializeField] private Image keyImage;

        [Header("Settings")]
        [SerializeField] private string pickupKeyText = "E";
        [SerializeField] private string promptFormat = "Press {0} to pick up";
        [SerializeField] private float fadeSpeed = 5f;

        [Header("Animation")]
        [SerializeField] private bool enableAnimation = true;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseScale = 1.1f;

        private CanvasGroup canvasGroup;
        private Vector3 originalScale;
        private bool isShowing = false;

        private void Awake()
        {
            canvasGroup = promptPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = promptPanel.AddComponent<CanvasGroup>();
            }

            originalScale = promptPanel.transform.localScale;

            // 초기 상태: 숨김
            Hide();

            // promptText 초기화
            if (promptText != null)
            {
                promptText.text = string.Format(promptFormat, pickupKeyText);
            }
        }

        private void Update()
        {
            if (isShowing && enableAnimation)
            {
                AnimatePrompt();
            }

            // 페이드 효과
            UpdateFade();
        }

        /// <summary>
        /// 프롬프트 표시
        /// </summary>
        public void Show(IPickupable item)
        {
            if (item == null) return;

            isShowing = true;
            promptPanel.SetActive(true);

            // 아이템 이름 표시 (영문)
            if (itemNameText != null)
            {
                WeaponPartItem weaponPart = item.Transform.GetComponent<WeaponPartItem>();
                if (weaponPart != null)
                {
                    itemNameText.text = GetWeaponPartNameEnglish(weaponPart.PartType);
                }
                else
                {
                    itemNameText.text = "Item";
                }
            }
        }

        /// <summary>
        /// 프롬프트 숨김
        /// </summary>
        public void Hide()
        {
            isShowing = false;
        }

        /// <summary>
        /// 애니메이션 처리
        /// </summary>
        private void AnimatePrompt()
        {
            float pulse = 1f + (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) * (pulseScale - 1f);
            promptPanel.transform.localScale = originalScale * pulse;
        }

        /// <summary>
        /// 페이드 효과
        /// </summary>
        private void UpdateFade()
        {
            float targetAlpha = isShowing ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

            if (!isShowing && canvasGroup.alpha < 0.01f)
            {
                promptPanel.SetActive(false);
                promptPanel.transform.localScale = originalScale;
            }
        }

        /// <summary>
        /// PlayerInteraction 이벤트 연결
        /// </summary>
        public void SubscribeToPlayerInteraction(PlayerInteraction playerInteraction)
        {
            if (playerInteraction == null)
            {
                Debug.LogWarning("[PickupPromptUI] PlayerInteraction이 null입니다!");
                return;
            }

            playerInteraction.OnItemInRange.AddListener(Show);
            playerInteraction.OnItemOutOfRange.AddListener(Hide);
        }

        /// <summary>
        /// PlayerInteraction 이벤트 해제
        /// </summary>
        public void UnsubscribeFromPlayerInteraction(PlayerInteraction playerInteraction)
        {
            if (playerInteraction == null) return;

            playerInteraction.OnItemInRange.RemoveListener(Show);
            playerInteraction.OnItemOutOfRange.RemoveListener(Hide);
        }

        /// <summary>
        /// 무기 부품 타입의 영문 이름 반환
        /// </summary>
        private string GetWeaponPartNameEnglish(WeaponPartType partType)
        {
            switch (partType)
            {
                case WeaponPartType.PencilSpear:
                    return "Pencil Spear Part";
                case WeaponPartType.EraserBomb:
                    return "Eraser Bomb Part";
                case WeaponPartType.RubberBandSling:
                    return "Rubber Band Part";
                default:
                    return "Weapon Part";
            }
        }
    }
}
