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

        [Header("UI Elements to Hide During Pause")]
        [SerializeField] private HeldWeaponUI heldWeaponUI;
        [SerializeField] private KeyCollectionUI keyCollectionUI;

        [Header("Pause Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button closePauseButton;

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

            // UI 요소들 자동 찾기
            if (heldWeaponUI == null)
                heldWeaponUI = FindObjectOfType<HeldWeaponUI>();

            if (keyCollectionUI == null)
                keyCollectionUI = FindObjectOfType<KeyCollectionUI>();

            // 버튼 이벤트 연결
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);

            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);

            if (closePauseButton != null)
                closePauseButton.onClick.AddListener(ClosePausePanel);

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
                else if (isPaused && pausePanel.activeInHierarchy)
                {
                    // 일시정지 패널이 열려있으면 재개
                    Resume();
                }
                else if (isPaused && !pausePanel.activeInHierarchy)
                {
                    // 일시정지 상태이지만 패널이 닫혀있으면 패널 다시 열기
                    ShowPausePanel();
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

            // 게임 UI 요소들 숨기기
            HideGameUI();

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

            // 게임 UI 요소들 다시 표시
            ShowGameUI();

            // 커서 숨김 (게임 설정에 따라 조정)
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Debug.Log("[PauseManager] 게임 재개");
        }

        /// <summary>
        /// 일시정지 패널 닫기 (게임은 계속 일시정지 상태 유지)
        /// </summary>
        public void ClosePausePanel()
        {
            // 패널만 닫고 일시정지 상태는 유지
            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            isSettingsOpen = false;
            // isPaused는 true로 유지, Time.timeScale도 0으로 유지
            // UI 요소들은 계속 숨김 상태 유지

            Debug.Log("[PauseManager] 일시정지 패널 닫기 (게임은 계속 정지 상태)");
        }

        /// <summary>
        /// 일시정지 패널 다시 열기
        /// </summary>
        public void ShowPausePanel()
        {
            if (pausePanel != null)
                pausePanel.SetActive(true);

            // 게임 UI 요소들 숨기기 (혹시 표시되어 있다면)
            HideGameUI();

            // 커서 표시
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Debug.Log("[PauseManager] 일시정지 패널 다시 열기");
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
        /// 게임 종료
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[PauseManager] 게임 종료");

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
        /// 게임 UI 요소들 숨기기
        /// </summary>
        private void HideGameUI()
        {
            // HeldWeaponUI (WeaponSlots) 숨기기
            if (heldWeaponUI != null && heldWeaponUI.gameObject != null)
            {
                heldWeaponUI.gameObject.SetActive(false);
                Debug.Log("[PauseManager] HeldWeaponUI 숨김");
            }

            // KeyCollectionUI 숨기기
            if (keyCollectionUI != null && keyCollectionUI.gameObject != null)
            {
                keyCollectionUI.gameObject.SetActive(false);
                Debug.Log("[PauseManager] KeyCollectionUI 숨김");
            }
        }

        /// <summary>
        /// 게임 UI 요소들 다시 표시
        /// </summary>
        private void ShowGameUI()
        {
            // HeldWeaponUI (WeaponSlots) 표시
            if (heldWeaponUI != null && heldWeaponUI.gameObject != null)
            {
                heldWeaponUI.gameObject.SetActive(true);
                Debug.Log("[PauseManager] HeldWeaponUI 표시");
            }

            // KeyCollectionUI 표시
            if (keyCollectionUI != null && keyCollectionUI.gameObject != null)
            {
                keyCollectionUI.gameObject.SetActive(true);
                Debug.Log("[PauseManager] KeyCollectionUI 표시");
            }
        }

        /// <summary>
        /// 현재 일시정지 상태 반환
        /// </summary>
        public bool IsPaused => isPaused;
    }
}
