using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YajaGame.Gameplay;

namespace YajaGame.UI
{
    /// <summary>
    /// 들고 있는 아이템을 화면에 표시하는 UI
    /// </summary>
    public class CarryItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject carryPanel;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI instructionText;

        [Header("Settings")]
        [SerializeField] private string throwInstruction = "Left Click to Throw";

        [Header("Animation")]
        [SerializeField] private bool enablePulseAnimation = true;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseScale = 1.1f;

        private CanvasGroup _canvasGroup;
        private Vector3 _originalScale;

        private void Awake()
        {
            // CanvasGroup 추가
            if (carryPanel != null)
            {
                _canvasGroup = carryPanel.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = carryPanel.AddComponent<CanvasGroup>();
                }
                _originalScale = carryPanel.transform.localScale;
            }

            // instruction 텍스트 설정
            if (instructionText != null)
            {
                instructionText.text = throwInstruction;
            }

            // 초기 상태: 숨김
            HideCarryUI();
        }

        private void Update()
        {
            // 펄스 애니메이션
            if (enablePulseAnimation && carryPanel != null && carryPanel.activeSelf)
            {
                float pulse = 1f + (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) * (pulseScale - 1f);
                carryPanel.transform.localScale = _originalScale * pulse;
            }
        }

        /// <summary>
        /// 들고 있는 아이템 표시
        /// </summary>
        public void ShowCarryUI(ItemBase item)
        {
            if (item == null || carryPanel == null) return;

            // 아이템 아이콘 설정
            if (itemIcon != null && item.Icon != null)
            {
                itemIcon.sprite = item.Icon;
                itemIcon.enabled = true;
            }
            else if (itemIcon != null)
            {
                itemIcon.enabled = false;
            }

            // 아이템 이름 설정 (영문)
            if (itemNameText != null)
            {
                WeaponPartItem weaponPart = item.GetComponent<WeaponPartItem>();
                if (weaponPart != null)
                {
                    itemNameText.text = GetWeaponPartNameEnglish(weaponPart.PartType);
                }
                else
                {
                    itemNameText.text = item.ItemName;
                }
            }

            // 패널 표시
            carryPanel.SetActive(true);

            // Fade In
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }

            Debug.Log($"[CarryItemUI] {item.ItemName} 표시");
        }

        /// <summary>
        /// 들고 있는 아이템 UI 숨기기
        /// </summary>
        public void HideCarryUI()
        {
            if (carryPanel != null)
            {
                carryPanel.SetActive(false);
            }

            // 스케일 복원
            if (carryPanel != null)
            {
                carryPanel.transform.localScale = _originalScale;
            }

            Debug.Log("[CarryItemUI] 숨김");
        }

        /// <summary>
        /// ItemCarrySystem 이벤트 연결
        /// </summary>
        public void SubscribeToCarrySystem(ItemCarrySystem carrySystem)
        {
            if (carrySystem == null)
            {
                Debug.LogWarning("[CarryItemUI] ItemCarrySystem이 null입니다!");
                return;
            }

            carrySystem.OnItemPickedUp.AddListener(ShowCarryUI);
            carrySystem.OnItemDropped.AddListener((item) => HideCarryUI());
        }

        /// <summary>
        /// 무기 부품 타입의 영문 이름 반환
        /// </summary>
        private string GetWeaponPartNameEnglish(WeaponPartType partType)
        {
            switch (partType)
            {
                case WeaponPartType.PencilSpear:
                    return "Pencil Spear";
                case WeaponPartType.EraserBomb:
                    return "Eraser Bomb";
                case WeaponPartType.RubberBandSling:
                    return "Rubber Band";
                default:
                    return "Item";
            }
        }
    }
}
