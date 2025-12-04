using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace YajaGame.UI
{
    /// <summary>
    /// 일시정지 관리자
    /// ESC로 일시정지 → 이어서/설정/메인으로
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Pause Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Settings")]
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private bool isPaused = false;
        private bool isSettingsOpen = false;

        private void Start()
        {
            // 초기 상태: 패널 숨김
            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            // 버튼 이벤트 연결
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);

            if (closeSettingsButton != null)
                closeSettingsButton.onClick.AddListener(CloseSettings);

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

            Debug.Log("[PauseManager] 초기화 완료");
        }

        private void Update()
        {
            // ESC 키로 일시정지 토글
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isSettingsOpen)
                {
                    // 설정 패널이 열려있으면 닫기
                    CloseSettings();
                }
                else if (isPaused)
                {
                    // 일시정지 상태면 재개
                    Resume();
                }
                else
                {
                    // 게임 중이면 일시정지
                    Pause();
                }
            }
        }

        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;

            if (pausePanel != null)
                pausePanel.SetActive(true);

            // 커서 표시
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Debug.Log("[PauseManager] 일시정지");
        }

        /// <summary>
        /// 게임 재개
        /// </summary>
        public void Resume()
        {
            isPaused = false;
            isSettingsOpen = false;
            Time.timeScale = 1f;

            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            // 커서 숨김 (게임 설정에 따라 조정)
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Debug.Log("[PauseManager] 게임 재개");
        }

        /// <summary>
        /// 설정 패널 열기
        /// </summary>
        public void OpenSettings()
        {
            isSettingsOpen = true;

            if (settingsPanel != null)
                settingsPanel.SetActive(true);

            Debug.Log("[PauseManager] 설정 열기");
        }

        /// <summary>
        /// 설정 패널 닫기
        /// </summary>
        public void CloseSettings()
        {
            isSettingsOpen = false;

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            // 설정 저장
            PlayerPrefs.Save();

            Debug.Log("[PauseManager] 설정 닫기");
        }

        /// <summary>
        /// 메인 메뉴로 이동
        /// </summary>
        public void GoToMainMenu()
        {
            // 시간 복구
            Time.timeScale = 1f;
            isPaused = false;

            Debug.Log("[PauseManager] 메인 메뉴로 이동");
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// BGM 볼륨 변경
        /// </summary>
        private void OnBGMVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("BGMVolume", value);
            Debug.Log($"[PauseManager] BGM 볼륨: {value}");
        }

        /// <summary>
        /// SFX 볼륨 변경
        /// </summary>
        private void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            Debug.Log($"[PauseManager] SFX 볼륨: {value}");
        }

        /// <summary>
        /// 현재 일시정지 상태 반환
        /// </summary>
        public bool IsPaused => isPaused;
    }
}
