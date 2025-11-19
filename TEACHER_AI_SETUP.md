# 선생님 AI + 손전등 + Game Over UI 설정 가이드

> 📝 **Git 머지 완료!** 이제 Unity 에디터에서 아래 단계를 따라 설정하세요.

---

## ✅ 체크리스트

- [ ] 1단계: Playground 씬 열기
- [ ] 2단계: GameManager 추가
- [ ] 3단계: Game Over UI 추가
- [ ] 4단계: 선생님 프리팹 배치
- [ ] 5단계: AIController 설정
- [ ] 6단계: 손전등 시스템 설정
- [ ] 7단계: NavMesh 재베이킹
- [ ] 8단계: Player 레이어/태그 설정
- [ ] 9단계: GameManager-GameUI 연결
- [ ] 10단계: 테스트

---

## 1단계: Playground 씬 열기

1. Unity 에디터 열기
2. Project 창에서 **Assets/StarterAssets/ThirdPersonController/Scenes/Playground.unity** 더블클릭
3. 씬이 열렸는지 확인

---

## 2단계: GameManager 추가

### A. 프리팹 배치

1. Project 창에서 **Assets/goschool/GameManager.prefab** 찾기
2. Hierarchy 창으로 드래그 앤 드롭
3. Inspector에서 Transform 확인:
   - Position: (0, 0, 0)
   - Rotation: (0, 0, 0)
   - Scale: (1, 1, 1)

### B. GameManager 컴포넌트 확인

1. Hierarchy에서 **GameManager** 선택
2. Inspector에서 확인:
   - ✅ GameManager (Script) 컴포넌트 있음
   - gameOverText: **None (나중에 연결)**
   - restartDelay: **3**

---

## 3단계: Game Over UI 추가

### A. Canvas 확인

1. Hierarchy에서 **Canvas** 찾기 (main UI)
2. 없으면:
   - 우클릭 → UI → Canvas
   - EventSystem도 자동 생성됨

### B. GameUI 추가

**방법 1: 프리팹 사용 (권장)**
1. Project 창에서 **Assets/goschool/GameUI.prefab** 찾기
2. **Canvas** 오브젝트로 드래그 (Canvas 하위로 배치)

**방법 2: 수동 생성**
1. Canvas 우클릭 → UI → Text - TextMeshPro
2. 이름: **GameOverText**
3. Rect Transform:
   - Anchor: Center-Center
   - Position: (0, 0, 0)
   - Width: 800, Height: 200
4. TextMeshPro - Text:
   - Text: **GAME OVER**
   - Font Size: **120**
   - Color: **White**
   - Alignment: **Center & Middle**
   - Wrapping: **Disabled**
5. 초기 상태: Inspector 왼쪽 위 체크박스 **OFF** (비활성화)

### C. Canvas 구조 확인

```
Canvas
├── CarryItemPanel (main UI - 있으면 유지)
├── InventoryPanel (main UI - 있으면 유지)
├── PickupPrompt (main UI - 있으면 유지)
└── GameUI 또는 GameOverText (방금 추가)
```

---

## 4단계: 선생님 프리팹 배치

### A. 프리팹 찾기

1. Project 창에서 **Assets/goschool/Scary_Teacher-1.prefab** 찾기
2. Scene 뷰에서 **교탁 앞 위치**로 드래그
3. 위치 조정:
   - Position 예시: (0, 0, 5) - 교탁 앞
   - Rotation: (0, 180, 0) - Player 방향 바라보게
   - Scale: (1, 1, 1)

### B. 기존 선생님 오브젝트 삭제 (있다면)

1. Hierarchy에서 **교탁 앞에 서있던 기존 선생님** 찾기
2. 우클릭 → Delete

---

## 5단계: AIController 설정

### A. 컴포넌트 확인

1. Hierarchy에서 **Scary_Teacher-1** 선택
2. Inspector에서 확인:
   - ✅ Nav Mesh Agent
   - ✅ Animator
   - ⚠️ **SimpleAIController 또는 AIController 중 하나 있어야 함**
   - ✅ ItemHolder

### B. SimpleAIController 제거 (손전등 사용 시)

**손전등을 게임 시작부터 사용한다면:**

1. SimpleAIController 컴포넌트 우클릭 → Remove Component
2. Add Component → 검색: **AIController**
3. AIController 추가됨

### C. AIController 설정

```
Movement Settings:
- Area Center: (0, 0, 0) - 교실 중심
- Area Radius: 15 - 순찰 범위
- Min Move Distance: 5
- Max Move Distance: 15
- Wait Time Min: 0.5
- Wait Time Max: 2

Vision Settings:
- Normal View Angle: 120
- Flashlight View Angle: 60
- Normal View Distance: 8
- Flashlight View Distance: 15
- Target Layer: Player (나중에 설정)

Item System:
- Start With Flashlight: ✅ true (게임 시작부터 손전등 착용)
```

### D. NavMeshAgent 설정

```
- Speed: 2
- Angular Speed: 360
- Acceleration: 15
- Stopping Distance: 0.2
- Auto Braking: ✅
- Radius: 0.5
- Height: 2
- Base Offset: 0
```

### E. Animator 설정

```
- Controller: New Animator Controller (자동 할당됨)
- Apply Root Motion: ❌
- Update Mode: Normal
- Culling Mode: Always Animate
```

---

## 6단계: 손전등 시스템 설정

### A. ItemHolder 컴포넌트 설정

1. Scary_Teacher-1 선택
2. Inspector에서 **ItemHolder (Script)** 찾기
3. 설정:
   - Right Hand Transform: **None (자동 찾기)**
   - Left Hand Transform: **None (사용 안 함)**
   - Flashlight Prefab: **Assets/NewFlashlightPrefab** 드래그

### B. RightHand Transform 찾기

**자동으로 안 찾아지면 수동 설정:**

1. Hierarchy에서 Scary_Teacher-1 펼치기 (▶ 클릭)
2. 본 구조에서 **RightHand** 찾기:
   ```
   Scary_Teacher-1
   └── mixamorig:Hips
       └── mixamorig:Spine
           └── ...
               └── mixamorig:RightHand (이것 찾기!)
   ```
3. ItemHolder → Right Hand Transform에 **mixamorig:RightHand** 드래그

### C. NewFlashlightPrefab 할당

1. Project 창에서 **Assets/NewFlashlightPrefab.prefab** 찾기
2. ItemHolder → Flashlight Prefab 필드에 드래그

---

## 7단계: NavMesh 재베이킹

### A. Navigation Static 설정

1. Hierarchy에서 **학교 바닥 오브젝트** 선택
   - 이름 예시: School, Ground, Floor 등
2. Inspector 오른쪽 위 **Static** 체크박스 클릭
3. **Navigation Static** 체크
4. 모든 자식 오브젝트에도 적용하겠냐고 물으면 **Yes**

### B. NavMesh 베이킹

1. 메뉴: **Window > AI > Navigation**
2. **Bake** 탭 선택
3. 설정:
   ```
   Agent Radius: 0.5
   Agent Height: 2
   Max Slope: 45
   Step Height: 0.4
   ```
4. **Bake** 버튼 클릭
5. Scene 뷰에서 **파란색 메쉬** 나타나면 성공!

### C. NavMesh 확인

1. Scene 뷰에서 교실 바닥이 파란색으로 표시되는지 확인
2. 선생님이 서있는 위치도 파란색 위인지 확인
3. 파란색 = 이동 가능 영역

---

## 8단계: Player 레이어/태그 설정

### A. Player 태그 확인

1. Hierarchy에서 **Player** 오브젝트 선택
   - 이름 예시: PlayerArmature, PlayerCapsule 등
2. Inspector 상단에서 **Tag** 확인
3. **Player** 선택 (없으면 Add Tag... → Tags → + → "Player" 생성)

### B. Player 레이어 생성

1. 메뉴: **Edit > Project Settings**
2. 좌측에서 **Tags and Layers** 선택
3. **Layers** 섹션에서 빈 슬롯 찾기 (User Layer 6~31)
4. 이름: **Player** 입력
5. 창 닫기

### C. Player 오브젝트 레이어 할당

1. Hierarchy에서 **Player** 오브젝트 선택
2. Inspector 상단에서 **Layer** 선택
3. **Player** 선택
4. "Change layer for all child objects?"라고 물으면 **Yes**

### D. AIController에 레이어 연결

1. Hierarchy에서 **Scary_Teacher-1** 선택
2. Inspector에서 **AIController (Script)** 찾기
3. **Target Layer** 필드에서 **Player** 체크 ✅

---

## 9단계: GameManager-GameUI 연결

### A. GameUI 오브젝트 찾기

1. Hierarchy에서 **Canvas > GameUI** 또는 **Canvas > GameOverText** 찾기

### B. GameManager에 연결

1. Hierarchy에서 **GameManager** 선택
2. Inspector에서 **GameManager (Script)** 찾기
3. **Game Over Text** 필드로 Hierarchy에서 **Canvas > GameUI (또는 GameOverText)** 드래그
4. 연결되면 필드에 **GameUI (GameObject)** 표시됨

### C. 연결 확인

- GameManager → Game Over Text: **GameUI** 연결됨 ✅
- restartDelay: **3** 확인

---

## 10단계: 테스트

### A. 플레이 모드 진입

1. Unity 상단에서 **Play 버튼 (▶)** 클릭
2. 게임 시작됨

### B. 체크리스트

#### 🎮 기본 기능
- [ ] Player 이동 (WASD)
- [ ] Player 점프 (Space)
- [ ] 카메라 회전 (마우스)

#### 🤖 선생님 AI
- [ ] 선생님이 교실 내부 순찰 (랜덤하게 걸어다님)
- [ ] 손전등 불빛이 켜져있음 (Spot Light)
- [ ] Player 가까이 가면 선생님이 쳐다봄
- [ ] Player를 발견하면 추격 시작 (빨리 달려옴)
- [ ] 숨으면 2초 후 추격 해제하고 순찰 복귀
- [ ] 애니메이션 전환 (Idle → Walk → Run)

#### 🎯 Game Over 시스템
- [ ] 선생님이 Player 1.5m 이내 도달
- [ ] 화면 중앙에 **"GAME OVER"** 텍스트 표시
- [ ] Player 조작 불가능
- [ ] 3초 후 씬 재시작

#### 💡 손전등
- [ ] 선생님 오른손에 손전등
- [ ] Spot Light 빛 표시됨
- [ ] 시야각이 좁아짐 (120° → 60°)
- [ ] 시야 거리 증가 (8m → 15m)

#### 🎨 UI
- [ ] main UI 정상 작동 (Carry, Inventory, Pickup)
- [ ] Game Over UI 정상 작동
- [ ] UI 충돌 없음

### C. 문제 해결

#### 선생님이 움직이지 않음
1. NavMesh가 베이킹되었는지 확인 (Scene 뷰에서 파란색)
2. NavMeshAgent 컴포넌트 활성화 확인
3. AIController 컴포넌트 활성화 확인
4. Console에서 에러 메시지 확인

#### 손전등이 안 보임
1. ItemHolder → Flashlight Prefab 할당 확인
2. Start With Flashlight: true 확인
3. Right Hand Transform 할당 확인
4. Console에서 "손전등 장착 완료" 메시지 확인

#### Player를 감지 못 함
1. Player 태그 설정 확인 (Tag: Player)
2. Player 레이어 설정 확인 (Layer: Player)
3. AIController → Target Layer: Player 체크 확인
4. View Distance 범위 확인 (15m)

#### Game Over UI가 안 나타남
1. GameManager → Game Over Text 연결 확인
2. GameUI 오브젝트 활성화 확인
3. Console에서 "플레이어 아웃!" 메시지 확인

#### Console 에러 확인
- "ItemHolder 컴포넌트가 필요합니다!" → ItemHolder 추가
- "손 Transform이 설정되지 않았습니다!" → Right Hand Transform 할당
- "GameManager.Instance가 null입니다!" → GameManager 오브젝트 확인

---

## 🎉 완료!

모든 체크리스트를 완료하셨다면 선생님 AI 시스템이 정상 작동합니다!

### 다음 단계 (선택사항)

1. **손전등 줍기 시스템** 구현
   - 손전등을 바닥에 배치
   - PickableObject 컴포넌트 추가
   - 선생님이 주울 수 있게 AI 확장

2. **난이도 조정**
   - chaseSpeed 증가/감소
   - viewDistance 조정
   - catchDistance 조정

3. **사운드 추가**
   - 발소리
   - 추격 시 음악
   - Game Over 사운드

4. **UI 개선**
   - 체력 바
   - 발견 경고
   - 미니맵

---

## 📞 문제 발생 시

1. Console 창 확인 (Ctrl+Shift+C)
2. 에러 메시지 읽기
3. 이 가이드 단계별 재확인
4. Unity 재시작

**파일 위치:**
- GameManager.prefab: `Assets/goschool/GameManager.prefab`
- GameUI.prefab: `Assets/goschool/GameUI.prefab`
- Scary_Teacher-1.prefab: `Assets/goschool/Scary_Teacher-1.prefab`
- NewFlashlightPrefab: `Assets/NewFlashlightPrefab.prefab`
- AIController.cs: `Assets/AIController.cs`
- GameManager.cs: `Assets/GameManager.cs`
