using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YajaGame.Gameplay;

namespace YajaGame.UI
{
    /// <summary>
    /// 라운드 관련 UI 관리
    /// - 라운드 시작 메시지
    /// - 남은 적 카운트
    /// - 라운드 클리어 메시지
    /// - 게임오버 메시지
    /// </summary>
    public class RoundUI : MonoBehaviour
    {
        [Header("Round Start UI")]
        [SerializeField] private GameObject roundStartPanel;
        [SerializeField] private Image roundStartImage;
        [SerializeField] private float roundStartDisplayTime = 2f;

        [Header("Round Images (이온님 에셋)")]
        [SerializeField] private Sprite round1Sprite;  // 메세지/라운드1.png
        [SerializeField] private Sprite round2Sprite;  // 메세지/라운드2.png
        [SerializeField] private Sprite round3Sprite;  // 메세지/라운드3.png

        [Header("Round Clear UI")]
        [SerializeField] private GameObject roundClearPanel;
        [SerializeField] private Image roundClearImage;

        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Image gameOverImage;  // 메세지/게임오버 메세지.png

        [Header("Victory UI")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Image victoryImage;  // 메세지/탈출 성공 메세지.png

        [Header("Enemy Count UI")]
        [SerializeField] private GameObject enemyCountPanel;
        [SerializeField] private Image droneIconImage;  // drone-count/드론 아이콘.png
        [SerializeField] private TextMeshProUGUI enemyCountText;
        [SerializeField] private Image[] droneCountImages;  // drone-count/드론-N.png (0~10)
        [SerializeField] private Image enemyCountImage;  // 단일 이미지 (간단 버전)
        [SerializeField] private Sprite[] enemyCountSprites;  // 드론-0 ~ 드론-10 스프라이트 배열

        [Header("Warning UI")]
        [SerializeField] private GameObject warningPanel;
        [SerializeField] private Image warningImage;  // 메세지/경고메세지.png 또는 발각위험.png

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;

        private CanvasGroup _roundStartCanvasGroup;
        private CanvasGroup _roundClearCanvasGroup;
        private CanvasGroup _gameOverCanvasGroup;

        private void Awake()
        {
            // CanvasGroup 추가 (페이드 효과용)
            if (roundStartPanel != null)
            {
                _roundStartCanvasGroup = roundStartPanel.GetComponent<CanvasGroup>();
                if (_roundStartCanvasGroup == null)
                    _roundStartCanvasGroup = roundStartPanel.AddComponent<CanvasGroup>();
            }

            if (roundClearPanel != null)
            {
                _roundClearCanvasGroup = roundClearPanel.GetComponent<CanvasGroup>();
                if (_roundClearCanvasGroup == null)
                    _roundClearCanvasGroup = roundClearPanel.AddComponent<CanvasGroup>();
            }

            if (gameOverPanel != null)
            {
                _gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
                if (_gameOverCanvasGroup == null)
                    _gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            // 모든 패널 숨기기
            HideAllPanels();

            // RoundManager 이벤트 연결
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.OnRoundStart.AddListener(OnRoundStart);
                RoundManager.Instance.OnRoundClear.AddListener(OnRoundClear);
                RoundManager.Instance.OnDroneCountChanged.AddListener(UpdateDroneCount);  // 드론만 카운트
                Debug.Log("[RoundUI] RoundManager 이벤트 연결 완료!");

                // 초기 드론 수 표시
                UpdateDroneCount(RoundManager.Instance.RemainingDrones);
            }
            else
            {
                Debug.LogWarning("[RoundUI] RoundManager.Instance가 null입니다!");
                StartCoroutine(DelayedInit());
            }
        }

        private IEnumerator DelayedInit()
        {
            yield return new WaitForSeconds(0.5f);
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.OnRoundStart.AddListener(OnRoundStart);
                RoundManager.Instance.OnRoundClear.AddListener(OnRoundClear);
                RoundManager.Instance.OnDroneCountChanged.AddListener(UpdateDroneCount);
                UpdateDroneCount(RoundManager.Instance.RemainingDrones);
                Debug.Log("[RoundUI] 지연 초기화 완료!");
            }
        }

        private void OnDestroy()
        {
            // 이벤트 해제
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.OnRoundStart.RemoveListener(OnRoundStart);
                RoundManager.Instance.OnRoundClear.RemoveListener(OnRoundClear);
                RoundManager.Instance.OnDroneCountChanged.RemoveListener(UpdateDroneCount);
            }
        }

        /// <summary>
        /// 모든 패널 숨기기
        /// </summary>
        private void HideAllPanels()
        {
            if (roundStartPanel != null) roundStartPanel.SetActive(false);
            if (roundClearPanel != null) roundClearPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (warningPanel != null) warningPanel.SetActive(false);
        }

        /// <summary>
        /// 라운드 시작 시 호출
        /// </summary>
        private void OnRoundStart()
        {
            StartCoroutine(ShowRoundStartMessage());
        }

        /// <summary>
        /// 라운드 시작 메시지 표시
        /// </summary>
        private IEnumerator ShowRoundStartMessage()
        {
            if (roundStartPanel == null) yield break;

            // 라운드에 맞는 이미지 설정
            if (roundStartImage != null && RoundManager.Instance != null)
            {
                int round = RoundManager.Instance.CurrentRound;
                switch (round)
                {
                    case 1:
                        if (round1Sprite != null) roundStartImage.sprite = round1Sprite;
                        break;
                    case 2:
                        if (round2Sprite != null) roundStartImage.sprite = round2Sprite;
                        break;
                    case 3:
                        if (round3Sprite != null) roundStartImage.sprite = round3Sprite;
                        break;
                }
            }

            // 페이드 인
            roundStartPanel.SetActive(true);
            yield return StartCoroutine(FadeIn(_roundStartCanvasGroup));

            // 표시 시간
            yield return new WaitForSeconds(roundStartDisplayTime);

            // 페이드 아웃
            yield return StartCoroutine(FadeOut(_roundStartCanvasGroup));
            roundStartPanel.SetActive(false);
        }

        /// <summary>
        /// 라운드 클리어 시 호출
        /// </summary>
        private void OnRoundClear()
        {
            StartCoroutine(ShowRoundClearMessage());
        }

        /// <summary>
        /// 라운드 클리어 메시지 표시
        /// </summary>
        private IEnumerator ShowRoundClearMessage()
        {
            if (roundClearPanel == null) yield break;

            roundClearPanel.SetActive(true);
            yield return StartCoroutine(FadeIn(_roundClearCanvasGroup));
        }

        /// <summary>
        /// 게임오버 표시
        /// </summary>
        public void ShowGameOver()
        {
            StartCoroutine(ShowGameOverMessage());
        }

        private IEnumerator ShowGameOverMessage()
        {
            if (gameOverPanel == null) yield break;

            gameOverPanel.SetActive(true);
            yield return StartCoroutine(FadeIn(_gameOverCanvasGroup));
        }

        /// <summary>
        /// 승리 화면 표시
        /// </summary>
        public void ShowVictory()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 남은 드론 수 업데이트
        /// </summary>
        private void UpdateDroneCount(int count)
        {
            Debug.Log($"[RoundUI] UpdateDroneCount 호출: {count}");

            // 텍스트로 표시
            if (enemyCountText != null)
            {
                enemyCountText.text = count.ToString();
            }

            // 단일 이미지 + 스프라이트 배열 방식 (간단 버전)
            if (enemyCountImage != null && enemyCountSprites != null && enemyCountSprites.Length > 0)
            {
                int index = Mathf.Clamp(count, 0, enemyCountSprites.Length - 1);
                Debug.Log($"[RoundUI] 스프라이트 변경: index={index}, 배열크기={enemyCountSprites.Length}");
                if (enemyCountSprites[index] != null)
                {
                    enemyCountImage.sprite = enemyCountSprites[index];
                    Debug.Log($"[RoundUI] 스프라이트 적용: {enemyCountSprites[index].name}");
                }
            }
            else
            {
                Debug.LogWarning($"[RoundUI] enemyCountImage={enemyCountImage != null}, sprites={enemyCountSprites?.Length ?? 0}");
            }

            // 이미지로 표시 (드론 카운트 이미지 사용) - 기존 방식
            if (droneCountImages != null && droneCountImages.Length > 0)
            {
                for (int i = 0; i < droneCountImages.Length; i++)
                {
                    if (droneCountImages[i] != null)
                    {
                        // 인덱스가 count와 같은 이미지만 활성화
                        droneCountImages[i].gameObject.SetActive(i == count);
                    }
                }
            }
        }

        /// <summary>
        /// 경고 메시지 표시 (발각 위험 등)
        /// </summary>
        public void ShowWarning(float duration = 2f)
        {
            StartCoroutine(ShowWarningMessage(duration));
        }

        private IEnumerator ShowWarningMessage(float duration)
        {
            if (warningPanel == null) yield break;

            warningPanel.SetActive(true);
            yield return new WaitForSeconds(duration);
            warningPanel.SetActive(false);
        }

        /// <summary>
        /// 페이드 인 효과
        /// </summary>
        private IEnumerator FadeIn(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;
            canvasGroup.alpha = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 페이드 아웃 효과
        /// </summary>
        private IEnumerator FadeOut(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;
            canvasGroup.alpha = 1f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }
    }
}
