# 선생님 AI 시스템 설정 가이드

이 가이드는 선생님 AI 추적, 손전등, Game Over 기능을 Playground 씬에 추가하는 방법을 설명합니다.

## 추출된 파일 목록

### 스크립트
- `Assets/SimpleAIController.cs` - 선생님 AI (추적/순찰/감지)
- `Assets/AIController.cs` - 고급 AI (손전등 연동)
- `Assets/GameManager.cs` - Game Over 로직
- `Assets/ItemHolder.cs` - 손전등 장착 시스템

### 프리팹 & 모델
- `Assets/NewFlashlightPrefab.prefab` - 손전등
- `Assets/goschool/Scary_Teacher-1.prefab` - 선생님 캐릭터
- `Assets/goschool/GameUI.prefab` - Game Over UI
- `Assets/goschool/GameManager.prefab` - GameManager 오브젝트
- `Assets/Scary_Teacher-1@Walking.fbx` - 선생님 모델 + 걷기 애니메이션

### 애니메이션
- `Assets/New Animator Controller.controller` - 애니메이션 컨트롤러

### NavMesh
- `Assets/Scenes/SampleScene/NavMesh-School.asset` - NavMesh 데이터

---

## Unity 설정 단계

### 1단계: Tag 설정

1. Unity 상단 메뉴: **Edit > Project Settings > Tags and Layers**
2. **Tags** 섹션에서 **+** 버튼 클릭
3. 새 태그 추가: `Player`
4. Hierarchy에서 **Player** 오브젝트 선택
5. Inspector 상단의 **Tag** 드롭다운에서 `Player` 선택

### 2단계: Layer 설정

1. **Edit > Project Settings > Tags and Layers**
2. **Layers** 섹션에서 빈 레이어 찾기
3. 다음 레이어 추가:
   - Layer 6: `Ground`
   - Layer 7: `Player` (선택사항)

4. Hierarchy에서 **바닥/교실 오브젝트** 선택
5. Inspector 상단의 **Layer** 드롭다운에서 `Ground` 선택
   - "Change children layers too?" → **Yes, change children** 클릭

### 3단계: GameManager 설정

1. **Project 창**에서 `Assets/goschool/GameManager.prefab` 찾기
2. Hierarchy의 **루트 레벨**로 드래그 앤 드롭
3. Hierarchy에 **GameManager** 오브젝트가 생성됨

### 4단계: Game Over UI 설정

1. **Hierarchy**에서 기존 **Canvas** 찾기 (없으면 GameObject > UI > Canvas로 생성)
2. **Project 창**에서 `Assets/goschool/GameUI.prefab` 찾기
3. **Canvas의 자식으로** 드래그 앤 드롭

4. **GameManager와 UI 연결**:
   - Hierarchy에서 **GameManager** 선택
   - Inspector에서 **GameManager (Script)** 컴포넌트 찾기
   - **Game Over Text** 필드에 `Canvas/GameUI/GameOverText` 드래그 앤 드롭

### 5단계: 선생님 캐릭터 배치

1. **Project 창**에서 `Assets/goschool/Scary_Teacher-1.prefab` 찾기
2. **Scene 뷰**에서 교실 강단(podium) 위치로 드래그
3. Transform 설정 예시:
   - Position: (0, 0, 0) - 교실 중앙 또는 원하는 위치
   - Rotation: (0, 0, 0)
   - Scale: (1, 1, 1)

### 6단계: 선생님 AI 컴포넌트 추가

선생님 프리팹에 **SimpleAIController** 또는 **AIController** 중 하나 추가:

#### 옵션 A: SimpleAIController (기본, 손전등 없음)

1. Hierarchy에서 **Scary_Teacher-1** 선택
2. Inspector 하단 **Add Component** 클릭
3. `SimpleAIController` 입력 후 선택
4. **컴포넌트 설정**:
   - **Player Layer**: `Player` 레이어 선택
   - **View Angle**: 60 (기본값)
   - **View Distance**: 15 (기본값)
   - **Chase Speed**: 5
   - **Movement Speed**: 2
   - **Catch Distance**: 1.5

#### 옵션 B: AIController (고급, 손전등 지원)

1. Hierarchy에서 **Scary_Teacher-1** 선택
2. Inspector 하단 **Add Component** 클릭
3. `AIController` 입력 후 선택
4. **Add Component** 다시 클릭 → `ItemHolder` 추가
5. **ItemHolder 설정**:
   - **Right Hand**: Scary_Teacher-1 프리팹의 RightHand 본 드래그
   - **Flashlight Prefab**: `Assets/NewFlashlightPrefab.prefab` 드래그
6. **AIController 설정**:
   - **Player**: Hierarchy의 `Player` 오브젝트 드래그
   - **View Angle**: 60
   - **View Distance**: 15
   - **Chase Speed**: 5

### 7단계: NavMeshAgent 추가

1. Hierarchy에서 **Scary_Teacher-1** 선택
2. Inspector 하단 **Add Component** 클릭
3. `NavMeshAgent` 입력 후 선택
4. **설정**:
   - **Speed**: 2 (걷기 속도)
   - **Angular Speed**: 120
   - **Acceleration**: 8
   - **Stopping Distance**: 0.5
   - **Auto Braking**: ✅ 체크
   - **Radius**: 0.5
   - **Height**: 2

### 8단계: NavMesh 베이킹

1. **Window > AI > Navigation** 메뉴 열기
2. **Bake** 탭 선택
3. **Agent Radius**: 0.5
4. **Agent Height**: 2
5. **Max Slope**: 45
6. **Step Height**: 0.4

7. Hierarchy에서 **바닥/교실 오브젝트** 선택
8. Inspector 우측 상단 **Static** 체크박스 옆 드롭다운 클릭
9. **Navigation Static** 체크
10. Navigation 창에서 **Bake** 버튼 클릭

> Scene 뷰에서 파란색 영역이 NavMesh입니다.

### 9단계: Player 충돌 감지 설정

1. Hierarchy에서 **Player** 오브젝트 선택
2. Inspector에서 **Collider** 컴포넌트 확인 (Capsule Collider 또는 Character Controller)
3. **Is Trigger** 체크 해제 (물리 충돌 활성화)
4. **Tag**가 `Player`인지 확인
5. **Layer**를 `Player`로 설정 (선택사항)

### 10단계: 테스트

1. Unity 상단 **재생 버튼** (▶) 클릭
2. **확인 사항**:
   - 선생님이 교실을 순찰하는가?
   - Player가 선생님 시야에 들어오면 추격하는가?
   - 선생님이 Player를 잡으면 "GAME OVER" 표시되는가?
   - 3초 후 씬이 재시작되는가?

---

## 선택사항: 손전등 게임 시작 시 장착

**AIController**를 사용하는 경우, 손전등을 자동으로 장착하려면:

1. Hierarchy에서 **Scary_Teacher-1** 선택
2. Inspector에서 **ItemHolder (Script)** 찾기
3. 스크립트 편집기에서 `ItemHolder.cs` 열기
4. `Start()` 메서드에 다음 추가:

```csharp
void Start()
{
    animator = GetComponent<Animator>();
    EquipFlashlight(); // 게임 시작 시 손전등 자동 장착
}
```

---

## 주요 파라미터 설명

### SimpleAIController
- **View Angle**: 시야 각도 (60° = 좌우 30°씩)
- **View Distance**: 시야 거리 (미터)
- **Chase Speed**: 추격 속도
- **Movement Speed**: 순찰 속도
- **Catch Distance**: Player를 잡는 거리
- **Lose Player Time**: Player를 놓친 후 포기하는 시간

### AIController
- 위 설정 + 손전등 연동
- **Flashlight Range**: 손전등 조명 범위
- **With Flashlight View Angle**: 손전등 켜졌을 때 시야각

### GameManager
- **Game Over Text**: Game Over 메시지 UI
- **Restart Delay**: 재시작까지 대기 시간 (3초)

---

## 문제 해결

### 선생님이 움직이지 않음
- NavMesh가 베이킹되었는지 확인 (Scene 뷰에서 파란색 영역)
- NavMeshAgent 컴포넌트가 추가되었는지 확인
- 선생님이 NavMesh 위에 배치되었는지 확인

### Player를 감지하지 못함
- Player Tag가 설정되었는지 확인
- Player Layer가 올바른지 확인
- AIController의 Player 레퍼런스가 연결되었는지 확인

### Game Over가 작동하지 않음
- GameManager가 Hierarchy에 있는지 확인
- GameManager의 Game Over Text 필드가 연결되었는지 확인
- Player에 Character Controller가 있는지 확인

### 손전등이 보이지 않음
- ItemHolder의 Right Hand 필드가 연결되었는지 확인
- Flashlight Prefab이 연결되었는지 확인
- AIController를 사용하는지 확인 (SimpleAIController는 손전등 미지원)

---

## 완료!

모든 설정이 완료되면 게임을 플레이하여 선생님 AI가 정상적으로 작동하는지 확인하세요.
