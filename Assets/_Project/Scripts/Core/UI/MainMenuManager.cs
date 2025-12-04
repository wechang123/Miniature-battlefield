using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace YajaGame.UI
{
    /// <summary>
    /// 메인 메뉴 관리자
    /// 인트로 → 메인 메뉴 → 로딩 → 게임 흐름 관리
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject introPanel;
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject loadingPanel;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button closeSettingsButton;

        [Header("Loading")]
        [SerializeField] private Slider loadingBar;
        [SerializeField] private string gameSceneName = "Playground";

        [Header("Settings")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private CanvasGroup introCanvasGroup;

        private bool isIntroShowing = true;

        private void Start()
        {
            // 초기 상태: 인트로 표시
            ShowIntro();

            // 버튼 이벤트 연결
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonClicked);

            if (closeSettingsButton != null)
                closeSettingsButton.onClick.AddListener(OnCloseSettingsClicked);

            // 슬라이더 초기값
            if (bgmSlider != null)
            {
                bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            // 커서 표시
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Debug.Log("[MainMenuManager] 초기화 완료");
        }

        private void Update()
        {
            // 인트로에서 아무 키나 누르면 메인 메뉴로
            if (isIntroShowing && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
            {
                ShowMainMenu();
            }
        }

        /// <summary>
        /// 인트로 화면 표시
        /// </summary>
        private void ShowIntro()
        {
            isIntroShowing = true;

            if (introPanel != null)
                introPanel.SetActive(true);

            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            Debug.Log("[MainMenuManager] 인트로 표시");
        }

        /// <summary>
        /// 메인 메뉴 표시
        /// </summary>
        private void ShowMainMenu()
        {
            isIntroShowing = false;

            // 페이드 아웃 효과
            if (introCanvasGroup != null)
            {
                StartCoroutine(FadeOutIntro());
            }
            else
            {
                if (introPanel != null)
                    introPanel.SetActive(false);

                if (mainMenuPanel != null)
                    mainMenuPanel.SetActive(true);
            }

            Debug.Log("[MainMenuManager] 메인 메뉴 표시");
        }

        /// <summary>
        /// 인트로 페이드 아웃
        /// </summary>
        private IEnumerator FadeOutIntro()
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                if (introCanvasGroup != null)
                    introCanvasGroup.alpha = 1f - (elapsed / fadeDuration);
                yield return null;
            }

            if (introPanel != null)
                introPanel.SetActive(false);

            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }

        /// <summary>
        /// 게임 시작 버튼
        /// </summary>
        public void OnPlayButtonClicked()
        {
            Debug.Log("[MainMenuManager] 게임 시작!");
            StartCoroutine(LoadGameScene());
        }

        /// <summary>
        /// 씬 로드 (로딩바 표시)
        /// </summary>
        private IEnumerator LoadGameScene()
        {
            // 로딩 패널 표시
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (loadingPanel != null)
                loadingPanel.SetActive(true);

            if (loadingBar != null)
                loadingBar.value = 0f;

            // 최소 로딩 시간 (1.5초)
            float minLoadTime = 1.5f;
            float elapsed = 0f;

            // 비동기 씬 로드
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                elapsed += Time.deltaTime;

                // 실제 로딩 진행률 (0.9까지만 올라감)
                float realProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                // 시간 기반 진행률 (최소 로딩 시간 동안 부드럽게)
                float timeProgress = Mathf.Clamp01(elapsed / minLoadTime);

                // 둘 중 작은 값 사용 (로딩바가 천천히 차오르도록)
                float displayProgress = Mathf.Min(realProgress, timeProgress);

                if (loadingBar != null)
                    loadingBar.value = displayProgress;

                // 로드 완료 + 최소 시간 경과
                if (asyncLoad.progress >= 0.9f && elapsed >= minLoadTime)
                {
                    if (loadingBar != null)
                        loadingBar.value = 1f;

                    yield return new WaitForSeconds(0.3f);
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        /// <summary>
        /// 설정 열기
        /// </summary>
        public void OnSettingsButtonClicked()
        {
            Debug.Log("[MainMenuManager] 설정 열기");

            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        /// <summary>
        /// 설정 닫기
        /// </summary>
        public void OnCloseSettingsClicked()
        {
            Debug.Log("[MainMenuManager] 설정 닫기");

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            // 설정 저장
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 종료 버튼
        /// </summary>
        public void OnQuitButtonClicked()
        {
            Debug.Log("[MainMenuManager] 게임 종료");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// BGM 볼륨 변경
        /// </summary>
        private void OnBGMVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("BGMVolume", value);
            // AudioManager가 있다면 여기서 볼륨 적용
            Debug.Log($"[MainMenuManager] BGM 볼륨: {value}");
        }

        /// <summary>
        /// SFX 볼륨 변경
        /// </summary>
        private void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            // AudioManager가 있다면 여기서 볼륨 적용
            Debug.Log($"[MainMenuManager] SFX 볼륨: {value}");
        }
    }
}
