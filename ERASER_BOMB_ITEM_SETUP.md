# 지우개폭탄 아이템 설정 가이드

## 1단계: Items 폴더 생성

1. Unity 에디터에서 **Project 창** 열기
2. `Assets/_Project/Prefabs` 폴더 우클릭
3. **Create → Folder** 선택
4. 폴더 이름을 **`Items`** 로 설정

---

## 2단계: EraserBomb_Item 프리팹 생성

### 1. 빈 GameObject 생성
1. **Hierarchy 창**에서 우클릭
2. **Create Empty** 선택
3. 이름을 **`EraserBomb_Item`** 으로 변경

### 2. 3D 모델 추가
1. **Project 창**에서 `Assets/_Project/Models/Weapons/EraserBomb/EraserBomb.fbx` 찾기
2. `EraserBomb_Item` GameObject 위로 **드래그 앤 드롭** (자식으로 추가)
3. 모델의 크기/위치 조정:
   - **Position**: (0, 0, 0)
   - **Rotation**: (0, 0, 0)
   - **Scale**: (1, 1, 1) 또는 적절한 크기로 조정

### 3. Sphere Collider 추가 (Trigger)
1. `EraserBomb_Item` GameObject 선택
2. **Inspector 창**에서 **Add Component** 클릭
3. **Sphere Collider** 검색 후 추가
4. **Is Trigger** 체크박스 ✅ 활성화
5. **Radius** 조정 (예: 0.5 ~ 1.0, 플레이어가 쉽게 주울 수 있는 크기)

### 4. EraserBombItem 스크립트 추가
1. `EraserBomb_Item` GameObject 선택
2. **Inspector 창**에서 **Add Component** 클릭
3. **EraserBombItem** 검색 후 추가
4. 스크립트 설정:
   - **Item Name**: "지우개폭탄"
   - **Item Type**: WeaponPart
   - **Weapon Part Type**: EraserBomb
   - **Is Pickable**: ✅ 체크
   - **Pickup Range**: 2.0
   - **Bob Height**: 0.3
   - **Bob Speed**: 2.0
   - **Rotation Speed**: 50.0
   - **Bomb Count**: 1
   - **Pickup Sound**: (선택사항) 획득 사운드 클립
   - **Pickup Effect**: (선택사항) 획득 이펙트 프리팹

### 5. Layer 설정
1. `EraserBomb_Item` GameObject 선택
2. **Inspector 창** 상단의 **Layer** 드롭다운 클릭
3. **Item** 레이어 선택 (없으면 **Add Layer**로 생성)

### 6. 프리팹으로 저장
1. **Hierarchy 창**에서 `EraserBomb_Item` GameObject를 **Project 창**의 `Assets/_Project/Prefabs/Items` 폴더로 드래그
2. Hierarchy에서 `EraserBomb_Item` 삭제 (씬에서 제거, 프리팹만 남김)

---

## 3단계: ItemSpawner GameObject 설정

### 1. GameObject 생성
1. **Hierarchy 창**에서 우클릭
2. **Create Empty** 선택
3. 이름을 **`EraserBombSpawner`** 로 변경

### 2. ItemSpawner 스크립트 추가
1. `EraserBombSpawner` GameObject 선택
2. **Inspector 창**에서 **Add Component** 클릭
3. **ItemSpawner** 검색 후 추가

### 3. Spawner 설정
**Spawn Settings:**
- **Item Prefab**: `EraserBomb_Item` 프리팹을 드래그 앤 드롭
- **Initial Spawn Count**: 5 (게임 시작 시 스폰 개수)
- **Spawn Interval**: 10.0 (재스폰 간격, 초)
- **Max Items In Scene**: 10 (맵에 동시 존재 가능한 최대 개수)

**Spawn Area:**
- **Spawn Area Center**: (0, 0, 0) - 교실 중심 좌표
- **Spawn Area Size**: (20, 0, 20) - 교실 크기 (X와 Z 값 조정)
- **Spawn Height**: 1.0 (바닥에서 얼마나 위에 스폰할지)
- **Ground Layer**: Ground 레이어 선택

**Debug:**
- **Show Spawn Area**: ✅ 체크 (기즈모로 스폰 영역 시각화)

### 4. 스폰 영역 조정
1. **Scene 뷰**에서 녹색 와이어프레임 큐브가 보임 (기즈모)
2. `EraserBombSpawner` GameObject 선택한 상태로:
   - **Spawn Area Center** 값 조정 → 교실 바닥 중심으로 이동
   - **Spawn Area Size** 값 조정 → 교실 범위에 맞게 크기 조정
3. 녹색 영역이 교실 바닥 전체를 덮도록 설정

---

## 4단계: 플레이어 태그 확인

EraserBombItem 스크립트가 `GameObject.FindGameObjectWithTag("Player")`를 사용하므로:

1. **Hierarchy 창**에서 플레이어 GameObject 선택
2. **Inspector 창** 상단의 **Tag** 드롭다운 확인
3. **Player** 태그가 설정되어 있는지 확인
4. 없으면 **Player** 태그로 설정

---

## 5단계: 테스트

1. **Play 버튼** 클릭
2. 확인 사항:
   - ✅ 교실에 지우개폭탄 5개가 랜덤 위치에 스폰됨
   - ✅ 아이템이 둥둥 떠다니며 회전함 (Bob & Rotate 애니메이션)
   - ✅ 플레이어가 가까이 가면 주울 수 있음
   - ✅ 주우면 Console에 "[EraserBombItem] 지우개폭탄 1개 획득!" 메시지
   - ✅ 주운 아이템은 사라짐
   - ✅ 10초마다 새로운 아이템 스폰 (최대 10개까지)

---

## 문제 해결

### 아이템이 바닥 아래로 떨어짐
- **Ground Layer**가 올바르게 설정되었는지 확인
- 바닥 GameObject의 Layer가 Ground인지 확인
- **Spawn Height** 값을 더 크게 조정 (예: 2.0)

### 아이템을 주울 수 없음
- 플레이어에 **Player** 태그가 설정되었는지 확인
- `EraserBomb_Item`의 **Is Pickable**이 체크되었는지 확인
- Sphere Collider의 **Is Trigger**가 체크되었는지 확인
- **Pickup Range** 값을 더 크게 조정

### 스폰 영역이 안 보임
- **Show Spawn Area**가 체크되었는지 확인
- **Scene 뷰**에서 **Gizmos** 버튼이 활성화되었는지 확인 (Scene 창 상단 우측)

### InventoryManager 경고 메시지
```
[EraserBombItem] 플레이어에게 InventoryManager가 없습니다!
```
- 정상입니다! InventoryManager가 아직 구현되지 않았기 때문
- 나중에 인벤토리 시스템 추가 시 자동으로 연동됨
