# 준원 인수인계 문서 - 2라운드 및 메뉴 UI

## 작성일: 2024-12-04
## 작성자: 위창

---

## 1. 프로젝트 현재 상태

### ✅ 완료된 것 (위창)
- **RoundManager.cs**: 라운드 시스템 (적 스폰, 처치 카운트, 씬 전환)
- **EnemyHealth.cs**: 적 체력 + 드랍 시스템 추가
- **RoundUI.cs**: 라운드 시작/클리어/게임오버 UI
- **전투 시스템**: PlayerHealth, WeaponController, 넉백
- **AI 시스템**: SimpleAIController (선생님), DroneAIController

### ❌ 준원이 할 것
1. **Round2 씬 제작** (복도 레벨)
2. **메인 메뉴 씬/UI**
3. **일시정지 메뉴 (ESC)**
4. **설정 메뉴**
5. **승리 화면**

---

## 2. Round2 씬 제작 가이드

### 2-1. 씬 생성
```
File → New Scene → Save As: Round2.unity
위치: Assets/Scenes/Round2.unity
```

### 2-2. RoundManager 설정
1. 빈 오브젝트 생성 → 이름: `RoundManager`
2. `RoundManager.cs` 스크립트 추가
3. Inspector 설정:
   - `Current Round`: 2
   - `Teacher Count`: 2 (또는 원하는 수)
   - `Drone Count`: 3 (또는 원하는 수)
   - `Next Scene Name`: "Victory" 또는 빈 문자열 (마지막 라운드)
   - `Use Scene Transition`: true (승리 화면으로 전환 시)

### 2-3. 스폰 포인트 설정
```
Hierarchy → Create Empty → 이름: SpawnPoint1, SpawnPoint2, ...
RoundManager의 Spawn Points 배열에 드래그
```

### 2-4. 적 프리팹 연결
위치: `Assets/_Project/Prefabs/`
- `TeacherPrefab` → 선생님 프리팹
- `DronePrefab` → 드론 프리팹

---

## 3. 메인 메뉴 제작 가이드

### 3-1. 씬 생성
```
File → New Scene → Save As: MainMenu.unity
위치: Assets/Scenes/MainMenu.unity
```

### 3-2. UI 구조
```
Canvas
├── BackgroundImage (screens/main-background1.png)
├── TitleText (TENADA 폰트)
├── ButtonPanel
│   ├── StartButton (버튼/게임시작.png)
│   ├── SettingsButton (버튼/설정.png)
│   └── QuitButton (버튼/나가기.png)
```

### 3-3. 버튼 스크립트
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnStartGame()
    {
        SceneManager.LoadScene("Playground"); // 1라운드 씬
    }

    public void OnSettings()
    {
        // 설정 패널 열기
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
```

---

## 4. 일시정지 메뉴 (ESC)

### 4-1. PauseMenuUI.cs
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YajaGame.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        private bool isPaused = false;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused) Resume();
                else Pause();
            }
        }

        public void Pause()
        {
            isPaused = true;
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            isPaused = false;
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
```

### 4-2. UI 구조
```
Canvas
└── PausePanel (기본 비활성화)
    ├── BackgroundImage (screens/pause-screen.png)
    ├── ResumeButton (버튼/다시하기.png)
    ├── SettingsButton (버튼/설정.png)
    └── MainMenuButton (버튼/나가기.png)
```

---

## 5. 설정 메뉴

### 5-1. SettingsUI.cs
```csharp
using UnityEngine;
using UnityEngine.UI;

namespace YajaGame.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        void Start()
        {
            // 저장된 설정 불러오기
            bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }

        public void OnBGMVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("BGMVolume", value);
            // AudioManager가 있으면 적용
        }

        public void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        public void SaveSettings()
        {
            PlayerPrefs.Save();
        }
    }
}
```

---

## 6. UI 에셋 위치

```
Assets/_Project/UI/
├── Jaro/                    # 영문 폰트
├── TENADA_font/             # 한글 폰트 (주로 사용)
├── hp&사운드바/              # HP바 이미지
├── 무기 슬롯/                # 무기 아이콘
├── 소음 경보 아이콘/          # 알림 아이콘
├── 버튼/                    # 메뉴 버튼들
│   ├── 게임시작.png
│   ├── 설정.png
│   ├── 나가기.png
│   ├── 다시하기.png
│   └── ...
├── screens/                 # 배경 화면
│   ├── main-background1.png
│   ├── main-background2.png
│   ├── pause-screen.png
│   └── setting-screen.png
├── drone-count/             # 적 카운트 이미지
├── 메세지/                   # 라운드 메시지
│   ├── 라운드1.png
│   ├── 라운드2.png
│   ├── 라운드3.png
│   ├── 게임오버 메세지.png
│   └── 탈출 성공 메세지.png
```

---

## 7. Build Settings 씬 순서

```
0. MainMenu
1. Playground (Round1)
2. Round2
3. Victory (또는 MainMenu로 복귀)
```

File → Build Settings → Scenes In Build에 추가

---

## 8. 참고할 기존 코드

| 파일 | 위치 | 참고 이유 |
|------|------|----------|
| RoundManager.cs | Scripts/Core/Gameplay/ | 라운드 시스템 패턴 |
| GameManager.cs | Scripts/Core/ | 싱글톤 패턴 |
| PlayerHealthUI.cs | Scripts/Core/UI/ | UI 업데이트 패턴 |
| InventoryUI.cs | Scripts/Core/UI/ | 슬롯 UI 패턴 |

---

## 9. 질문/연락

- 위창 카톡으로 연락
- 프로젝트 Discord 채널

---

## 10. 체크리스트

- [ ] MainMenu.unity 씬 생성
- [ ] Round2.unity 씬 생성
- [ ] 메인 메뉴 UI 제작
- [ ] 일시정지 메뉴 제작 (ESC)
- [ ] 설정 메뉴 제작
- [ ] 승리 화면 제작
- [ ] Build Settings에 씬 추가
- [ ] 테스트: 메인메뉴 → 라운드1 → 라운드2 → 승리
