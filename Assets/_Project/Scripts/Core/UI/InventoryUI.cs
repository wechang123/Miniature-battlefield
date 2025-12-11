using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YajaGame.Gameplay;

namespace YajaGame.UI
{
    /// <summary>
    /// 인벤토리 UI 표시 (무기 부품 개수 등)
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Weapon Part UI")]
        [SerializeField] private TextMeshProUGUI pencilSpearCountText;
        [SerializeField] private TextMeshProUGUI eraserBombCountText;
        [SerializeField] private TextMeshProUGUI rubberBandSlingCountText;

        [Header("Notification")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationDuration = 2f;

        [Header("Notification")]
        [SerializeField] private string pickupNotificationFormat = "+{0} {1}";

        private float notificationTimer = 0f;
        private CanvasGroup notificationCanvasGroup;

        private void Awake()
        {
            if (notificationPanel != null)
            {
                notificationCanvasGroup = notificationPanel.GetComponent<CanvasGroup>();
                if (notificationCanvasGroup == null)
                {
                    notificationCanvasGroup = notificationPanel.AddComponent<CanvasGroup>();
                }
                notificationPanel.SetActive(false);
            }
        }

        private void Start()
        {
            // InventoryManager 이벤트 구독
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged.AddListener(OnInventoryChanged);
                InventoryManager.Instance.OnThrowCountChanged.AddListener(OnThrowCountChanged);
                UpdateAllCounts();
            }
            else
            {
                Debug.LogWarning("[InventoryUI] InventoryManager를 찾을 수 없습니다!");
            }
        }

        private void Update()
        {
            // 알림 페이드아웃 - 활성화된 경우에만 처리
            if (notificationTimer > 0f && notificationPanel != null && notificationPanel.activeSelf)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationCanvasGroup != null)
                {
                    notificationCanvasGroup.alpha = Mathf.Clamp01(notificationTimer / notificationDuration);
                }

                if (notificationTimer <= 0f)
                {
                    notificationPanel.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 인벤토리 변경 시 호출
        /// </summary>
        private void OnInventoryChanged(WeaponPartType partType, int newCount)
        {
            UpdatePartCount(partType);
            ShowPickupNotification(partType, 1); // 기본적으로 1개씩 추가되는 것으로 가정
        }

        /// <summary>
        /// 던진 횟수 변경 시 호출
        /// </summary>
        private void OnThrowCountChanged(WeaponPartType partType, int newCount)
        {
            UpdatePartCount(partType);
        }

        /// <summary>
        /// 특정 무기 부품 개수 업데이트
        /// </summary>
        private void UpdatePartCount(WeaponPartType partType)
        {
            if (InventoryManager.Instance == null) return;

            TextMeshProUGUI targetText = GetTextForPartType(partType);
            if (targetText != null)
            {
                string label = GetPartLabelEnglish(partType);
                int count = InventoryManager.Instance.GetWeaponPartCount(partType);
                int throws = InventoryManager.Instance.GetThrowCount(partType);
                targetText.text = $"{label}: {count} (Thrown: {throws})";
            }
        }

        /// <summary>
        /// 모든 부품 개수 업데이트
        /// </summary>
        private void UpdateAllCounts()
        {
            if (InventoryManager.Instance == null) return;

            UpdatePartCount(WeaponPartType.PencilSpear);
            UpdatePartCount(WeaponPartType.EraserBomb);
            UpdatePartCount(WeaponPartType.RubberBandSling);
        }

        /// <summary>
        /// 부품 획득 알림 표시
        /// </summary>
        private void ShowPickupNotification(WeaponPartType partType, int amount)
        {
            if (notificationPanel == null || notificationText == null) return;

            string partName = GetPartName(partType);
            notificationText.text = string.Format(pickupNotificationFormat, amount, partName);

            notificationPanel.SetActive(true);
            notificationTimer = notificationDuration;
            if (notificationCanvasGroup != null)
            {
                notificationCanvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// 부품 타입에 맞는 텍스트 UI 반환
        /// </summary>
        private TextMeshProUGUI GetTextForPartType(WeaponPartType partType)
        {
            switch (partType)
            {
                case WeaponPartType.PencilSpear:
                    return pencilSpearCountText;
                case WeaponPartType.EraserBomb:
                    return eraserBombCountText;
                case WeaponPartType.RubberBandSling:
                    return rubberBandSlingCountText;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 부품 타입 라벨 반환 (영문)
        /// </summary>
        private string GetPartLabelEnglish(WeaponPartType partType)
        {
            switch (partType)
            {
                case WeaponPartType.PencilSpear:
                    return "Pencil";
                case WeaponPartType.EraserBomb:
                    return "Eraser";
                case WeaponPartType.RubberBandSling:
                    return "Rubber";
                default:
                    return "Item";
            }
        }

        /// <summary>
        /// 부품 타입 이름 반환
        /// </summary>
        private string GetPartName(WeaponPartType partType)
        {
            switch (partType)
            {
                case WeaponPartType.PencilSpear:
                    return "연필창 부품";
                case WeaponPartType.EraserBomb:
                    return "지우개폭탄 부품";
                case WeaponPartType.RubberBandSling:
                    return "고무줄 슬링 부품";
                default:
                    return "부품";
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged.RemoveListener(OnInventoryChanged);
                InventoryManager.Instance.OnThrowCountChanged.RemoveListener(OnThrowCountChanged);
            }
        }
    }
}
