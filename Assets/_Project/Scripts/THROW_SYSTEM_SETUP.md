# 아이템 들기/던지기 시스템 설정 가이드

이 가이드는 아이템을 들고 던지는 시스템을 Unity 에디터에서 설정하는 방법을 안내합니다.

---

## 📋 목차
1. [Player 설정](#1-player-설정)
2. [Canvas UI 설정](#2-canvas-ui-설정)
3. [아이템 설정](#3-아이템-설정)
4. [테스트](#4-테스트)

---

## 1. Player 설정

### 1-1. Player에 컴포넌트 추가

**Hierarchy에서 Player 오브젝트 선택 후 Inspector에서:**

1. **ItemCarrySystem 추가**
   - `Add Component` → 검색: `ItemCarrySystem`
   - 설정:
     - `Auto Find Carry Position`: ✅ 체크
     - `Carry Position Name`: `CarryPosition`
     - `Carry Offset`: `(0, 0, 0)`
     - `Carry Rotation`: `(0, 0, 0)`
     - `Carry Scale`: `1`

2. **ItemThrowSystem 추가**
   - `Add Component` → 검색: `ItemThrowSystem`
   - 설정:
     - `Throw Force`: `15` (던지는 힘)
     - `Throw Angle`: `30` (위로 던지는 각도)
     - `Use Mouse Aim`: ✅ 체크
     - `Auto Find Camera`: ✅ 체크

3. **TrajectoryPredictor 추가**
   - `Add Component` → 검색: `TrajectoryPredictor`
   - `Add Component` → 검색: `Line Renderer` (자동 추가됨)
   - 설정:
     - `Point Count`: `30`
     - `Time Step`: `0.1`
     - `Max Distance`: `20`
     - `Line Width`: `0.1`

### 1-2. PlayerInteraction 확인

**Player에 이미 있는 `PlayerInteraction` 컴포넌트를 확인:**
- `ItemCarrySystem`이 자동으로 연결되어 있어야 합니다

---

## 2. Canvas UI 설정

### 2-1. CarryItemPanel 생성 (들고 있는 아이템 표시)

1. **Hierarchy에서 Canvas 아래에 Empty Object 생성**
   - 우클릭 → `Create Empty`
   - 이름: `CarryItemPanel`

2. **CarryItemPanel 위치 설정 (Inspector → Rect Transform)**
   - `Anchors`: **Bottom Center** (아래 중앙)
   - `Pos X`: `0`
   - `Pos Y`: `100`
   - `Width`: `300`
   - `Height`: `120`

3. **CarryItemPanel에 자식 추가**

   **a) ItemIcon (Image)**
   - Hierarchy에서 CarryItemPanel 우클릭 → `UI` → `Image`
   - 이름: `ItemIcon`
   - Rect Transform:
     - Anchors: Top Stretch
     - Pos Y: `-30`
     - Height: `60`

   **b) ItemNameText (TextMeshPro)**
   - Hierarchy에서 CarryItemPanel 우클릭 → `UI` → `Text - TextMeshPro`
   - 이름: `ItemNameText`
   - Rect Transform:
     - Anchors: Top Stretch
     - Pos Y: `-95`
     - Height: `30`
   - Text: `Item Name`
   - Font Size: `18`
   - Alignment: Center
   - Color: White

   **c) InstructionText (TextMeshPro)**
   - Hierarchy에서 CarryItemPanel 우클릭 → `UI` → `Text - TextMeshPro`
   - 이름: `InstructionText`
   - Rect Transform:
     - Anchors: Bottom Stretch
     - Pos Y: `10`
     - Height: `25`
   - Text: `Left Click to Throw`
   - Font Size: `16`
   - Alignment: Center
   - Color: Yellow `#FFFF00`

4. **CarryItemPanel에 CarryItemUI 스크립트 추가**
   - Inspector → `Add Component` → `CarryItemUI`
   - 설정:
     - `Carry Panel`: CarryItemPanel (자기 자신)
     - `Item Icon`: ItemIcon
     - `Item Name Text`: ItemNameText
     - `Instruction Text`: InstructionText
     - `Throw Instruction`: `Left Click to Throw`

### 2-2. InventoryPanel 업데이트

**이미 있는 InventoryPanel의 텍스트가 이제 다음 형식으로 표시됩니다:**
- `Pencil: 0 (Thrown: 0)`
- `Eraser: 0 (Thrown: 0)`
- `Rubber: 0 (Thrown: 0)`

---

## 3. 아이템 설정

### 3-1. WeaponPartItem에 Rigidbody 추가

**Hierarchy에서 아이템 오브젝트 선택 (예: PencilSpearPart):**

1. **Rigidbody 추가** (던지기 위해 필요)
   - `Add Component` → `Rigidbody`
   - 설정:
     - `Mass`: `1`
     - `Drag`: `0.5`
     - `Angular Drag`: `0.5`
     - `Use Gravity`: ✅ 체크
     - `Is Kinematic`: ✅ 체크 (처음에는 kinematic)

2. **Collider 확인**
   - 아이템에 `Collider`가 있는지 확인 (Box, Sphere, Mesh Collider 등)
   - `Is Trigger`: ✅ 체크

3. **Layer 설정**
   - Inspector 상단 → `Layer` → `UI` 선택
   - 또는 새 Layer 생성: `Item`

### 3-2. WeaponPartItem 설정 확인

**Inspector에서 `WeaponPartItem` 스크립트 확인:**
- `Item Name`: 영문으로 설정 (예: `Pencil Spear Part`)
- `Item Type`: `Weapon Part`
- `Weapon Part Type`: `Pencil Spear` / `Eraser Bomb` / `Rubber Band Sling`
- `Item Value`: `1`
- `Pickup Range`: `2`
- `Is Pickable`: ✅ 체크

---

## 4. 이벤트 연결

### 4-1. Player와 UI 연결

1. **Player 선택 → Inspector**

2. **ItemCarrySystem 컴포넌트**
   - `On Item Picked Up` 이벤트:
     - `+` 버튼 클릭
     - Object: `CarryItemPanel`
     - Function: `CarryItemUI.ShowCarryUI`

   - `On Item Dropped` 이벤트:
     - `+` 버튼 클릭
     - Object: `CarryItemPanel`
     - Function: `CarryItemUI.HideCarryUI`

3. **PlayerInteraction 컴포넌트 (이미 설정되어 있음)**
   - `On Item In Range`: PickupPromptUI.Show
   - `On Item Out Of Range`: PickupPromptUI.Hide

---

## 5. 테스트

### 5-1. 씬 실행

1. **Play 버튼 클릭**

2. **아이템 근처로 이동**
   - "Press E to pick up" + "Pencil Spear Part" 표시됨
   - 포물선 궤적이 빨간/주황 라인으로 표시됨

3. **E키로 아이템 줍기**
   - 화면 하단 중앙에 아이템 정보 표시
   - "Left Click to Throw" 안내
   - 아이템이 플레이어 손/머리 위에 부착됨

4. **마우스 좌클릭으로 던지기**
   - 포물선으로 날아감
   - 바닥에 떨어짐
   - 다시 줍기 가능

5. **통계 확인 (우측 상단)**
   - `Pencil: 0 (Thrown: 1)`
   - 던질 때마다 카운트 증가

---

## 6. 문제 해결

### 문제 1: 아이템이 손에 부착되지 않음
**해결:**
- Player의 ItemCarrySystem 확인
- `Auto Find Carry Position`이 체크되어 있는지 확인
- 자동으로 `CarryPosition` 오브젝트가 생성되어야 함

### 문제 2: 던지기가 작동하지 않음
**해결:**
- StarterAssets.inputactions 파일이 제대로 저장되었는지 확인
- Unity 재시작
- Input System 패키지가 설치되어 있는지 확인

### 문제 3: 궤적이 표시되지 않음
**해결:**
- Player에 TrajectoryPredictor + LineRenderer가 있는지 확인
- LineRenderer Material 설정 확인
- Camera가 제대로 연결되어 있는지 확인

### 문제 4: UI가 표시되지 않음
**해결:**
- Canvas가 Screen Space - Overlay로 설정되어 있는지 확인
- InventoryUI 스크립트의 Text 참조가 모두 연결되어 있는지 확인
- CarryItemUI 스크립트의 참조가 모두 연결되어 있는지 확인

### 문제 5: 아이템이 바닥에 떨어진 후 다시 줍기가 안됨
**해결:**
- 아이템의 Rigidbody가 제대로 설정되어 있는지 확인
- ThrownProjectile 스크립트의 `Ground Stop Velocity` 값 조정 (기본: 1)

---

## 7. 커스터마이징

### 던지는 힘 조정
**Player → ItemThrowSystem:**
- `Throw Force`: 값을 높이면 더 멀리 날아감 (기본: 15)
- `Throw Angle`: 각도를 높이면 더 높이 던져짐 (기본: 30)

### 궤적 라인 색상 변경
**Player → TrajectoryPredictor → LineRenderer:**
- Color Gradient를 수정해서 원하는 색상으로 변경

### UI 위치 변경
**Canvas → CarryItemPanel:**
- Rect Transform의 Pos Y 값을 변경해서 위치 조정

---

## 8. 협업자를 위한 정보

### 적(Enemy) 추가 방법

**적 오브젝트에 다음 설정 추가:**
1. Tag를 `Enemy`로 설정
2. Collider 추가
3. Rigidbody 추가 (옵션, 충격력 받으려면 필요)
4. 데미지 시스템 추가 (예: EnemyHealth 스크립트)

**ThrownProjectile이 적을 감지하면:**
- OnEnemyHit 이벤트 발생
- 데미지 적용 (EnemyHealth가 있으면)
- 충격력 적용 (Rigidbody가 있으면)

---

## 9. 요약 체크리스트

- [ ] Player에 ItemCarrySystem 추가
- [ ] Player에 ItemThrowSystem 추가
- [ ] Player에 TrajectoryPredictor + LineRenderer 추가
- [ ] Canvas에 CarryItemPanel 생성
- [ ] CarryItemPanel에 ItemIcon, ItemNameText, InstructionText 추가
- [ ] CarryItemPanel에 CarryItemUI 스크립트 추가
- [ ] Player와 UI 이벤트 연결
- [ ] 아이템에 Rigidbody 추가
- [ ] 테스트: E키로 줍기 → 마우스 좌클릭으로 던지기

---

**완료! 이제 게임을 실행해서 테스트해보세요! 🎮**
