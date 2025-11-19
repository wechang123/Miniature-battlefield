# 게임 버그 수정 Unity 작업 가이드

코드 수정은 완료되었습니다. Unity 에디터에서 다음 작업을 수행하세요.

---

## ✅ 완료된 코드 수정

- ✅ 연필창 유령 줍기 버그
- ✅ 스폰된 아이템 줍기 불가 버그
- ✅ 아이템 부유 높이 (1.3m → 0.4m)
- ✅ UI 위치 추적 (아이템 위 0.5m)

---

## 🔧 Unity 작업 필요 (필수)

### 1. 유령 연필창 아이템 제거 ⚠️ 중요!

**문제**: Player의 CarryPosition 안에 `PencilSpear_Item`이 숨어있어서 게임 시작 시 "E 줍기" UI가 표시됩니다.

**해결**:
1. **Hierarchy**에서 **Player** 오브젝트 확장
2. 자식 오브젝트들을 펼쳐서 **CarryPosition** 찾기
   - 경로 예시: Player > [RightHand bone] > CarryPosition
3. **CarryPosition** 안에 **PencilSpear_Item**이 있으면 선택
4. **Delete** 키 또는 우클릭 → **Delete** 클릭
5. **File > Save** (Ctrl+S) 저장

**확인 방법**:
- CarryPosition은 비어있어야 합니다 (자식 오브젝트 없음)
- 게임 실행 시 "E 줍기" UI가 즉시 뜨지 않아야 함

---

### 2. 책상 콜라이더 추가 (선생님 AI 통과 방지)

**문제**: 선생님이 책상 서랍/상판을 뚫고 지나갑니다.

**중요**: Box Collider만 추가하면 안 됩니다! **반드시 NavMesh를 다시 Bake해야** AI가 책상을 피합니다!

**해결**:

#### ⚠️ 방법 1: 책상을 Navigation Static으로 설정 (가장 확실함)

1. **Hierarchy**에서 School > 책상 본체(desk body/drawer) 선택
2. **Inspector** 우측 상단 **Static** 드롭다운 클릭
3. **Navigation Static** 체크박스 활성화
4. "Change children too?" 나오면 → **Yes, change children**
5. 모든 책상 본체에 반복
6. **아래 "3. NavMesh 재베이킹" 섹션으로 이동하여 NavMesh를 다시 Bake!**

#### 방법 2: NavMesh Obstacle 추가 (런타임 동적 장애물용)

이 방법은 게임 중에 움직이는 장애물에 사용합니다. 책상은 고정 오브젝트이므로 **방법 1 권장**.

1. 책상 본체 선택
2. **Add Component** → `Nav Mesh Obstacle`
3. **Carve** 체크박스 활성화
4. **Size**: 책상 크기에 맞게 조절 (X, Y, Z)
5. **Center**: 책상 중심에 맞게 조절

⚠️ **주의**: Box Collider는 물리 충돌용이지, NavMesh용이 아닙니다! AI는 NavMesh만 보고 이동합니다.

---

### 3. NavMesh 재베이킹 ⚠️ 필수!

**이 단계를 건너뛰면 선생님이 여전히 책상을 통과합니다!**

책상을 Navigation Static으로 설정한 후:

1. **Window > AI > Navigation** 열기

2. **Agents** 탭에서 설정 확인:
   - Agent Radius: **0.5**
   - Agent Height: **2.0**
   - Max Slope: **45**
   - Step Height: **0.4**

3. **Areas** 탭:
   - Default = Walkable (확인)

4. **Bake** 탭으로 이동

5. **Bake** 버튼 클릭 (또는 **Clear** → **Bake**)
   - 진행 바가 나타나면서 NavMesh가 재계산됩니다
   - 완료되면 "Done baking NavMesh" 메시지 표시

6. **Scene 뷰에서 확인** (중요!):
   - Scene 뷰 상단 도구바에서 **Shaded** 모드 확인
   - 바닥에 **파란색(cyan) NavMesh** 영역이 보여야 함
   - **책상 본체 주변 파란색이 사라졌는지 확인**
   - 책상 다리 사이는 파란색이 있어도 됨 (통과 가능)
   - 책상 본체(서랍/상판)는 파란색이 없어야 함 (통과 불가)

7. **테스트**:
   - **Play** 버튼 눌러서 게임 실행
   - 선생님이 책상을 피해서 이동하는지 확인
   - Scene 뷰에서 선생님 경로 관찰 (초록색 선)

---

## 🎮 테스트 체크리스트

### 유령 아이템 UI 버그
- [ ] 게임 시작 시 "E 줍기" UI가 표시되지 않음
- [ ] 아이템 근처에 갈 때만 UI가 표시됨

### 아이템 줍기 시스템
- [ ] 연필창 부품(WeaponPartItem): 주우면 즉시 사라지고 인벤토리에 추가됨 (손에 들지 않음)
- [ ] 지우개 부품(EraserBombItem): 주우면 즉시 사라지고 인벤토리에 추가됨 (손에 들지 않음)
- [ ] 연필창 완성품(MeleeWeaponItem): 손에 들고 다님, 버릴 수 있음
- [ ] 버린 연필창 완성품을 다시 줍기 가능

### 아이템 배치
- [ ] 아이템이 바닥에 배치되어 있음 (떠다니지 않음)
- [ ] 줍기 애니메이션이 자연스러움

### 문제 5: UI 위치
- [ ] 아이템 근처에 가면 아이템 위에 "E 줍기" UI 표시
- [ ] 플레이어 이동 시 UI가 아이템을 따라다님
- [ ] 카메라 각도 변경 시에도 UI가 아이템 위치 유지

### 문제 1: NavMesh
- [ ] 선생님이 책상을 뚫고 지나가지 않음
- [ ] 선생님이 책상을 피해서 이동함
- [ ] 선생님이 길을 잃지 않음

---

## 🔍 문제 해결

### 게임 시작 시 "E 줍기" UI가 뜸
→ **Player > CarryPosition** 안에 PencilSpear_Item이 있는지 확인 후 삭제
→ Hierarchy에서 Player 확장 → CarryPosition 확인 → 자식 오브젝트 모두 삭제

### 선생님이 여전히 책상을 통과함

**가장 흔한 원인**: NavMesh를 재베이킹하지 않았거나, 책상이 Navigation Static이 아님

**해결 순서**:
1. **책상 본체 선택** → Inspector에서 **Static** 확인
   - Navigation Static 체크박스가 **체크되어 있어야 함**
   - 안 되어 있으면 체크하고 다음 단계로

2. **Window > AI > Navigation** → **Bake** 탭
   - **Clear** 버튼 클릭 (기존 NavMesh 삭제)
   - **Bake** 버튼 클릭 (새로 생성)

3. **Scene 뷰에서 확인**:
   - 책상 본체 위/주변에 **파란색 NavMesh가 없어야 함**
   - 만약 여전히 파란색이 있다면:
     - 책상이 Navigation Static이 아닐 가능성
     - 책상 메시가 너무 작아서 NavMesh가 뚫고 지나갈 수 있음
     - → **NavMesh Obstacle** (Carve 활성화)로 변경 시도

4. **SimpleAIController 설정 확인**:
   - Hierarchy에서 Teacher_Character 선택
   - NavMesh Agent 컴포넌트 확인:
     - Obstacle Avoidance Type: **High Quality**
     - Avoidance Priority: **50** (기본값)

5. **최후의 수단**: NavMesh Obstacle 사용
   - 책상 본체에 **Nav Mesh Obstacle** 컴포넌트 추가
   - **Carve** 체크박스 활성화
   - Size를 책상 본체보다 약간 크게 설정
   - NavMesh를 다시 Bake하지 **않아도** 작동함 (동적 장애물)

### UI가 표시되지 않음
→ Canvas 설정 확인:
  - Canvas > Render Mode: **Screen Space - Camera**
  - Canvas > Render Camera: **Main Camera**로 설정

### 아이템이 여전히 떠있음
→ 코드가 업데이트되었지만 기존 프리팹 설정이 남아있을 수 있음
→ Unity에서 ItemSpawner/RandomItemSpawner Inspector 확인:
  - Spawn Height: **0**으로 변경
→ ItemBase 프리팹들 (PencilSpear_Item, EraserBomb_Item 등) Inspector 확인:
  - Bob Height: **0**으로 변경

### 무기 부품을 주웠는데 손에 들고 다님 (사라지지 않음)
→ 코드 업데이트 후 Unity 재시작 필요
→ Play Mode 종료 후 다시 시작
→ 여전히 문제면: Build Settings > Player Settings > Scripting Backend 확인

---

## 완료!

모든 작업이 완료되면:
1. **File > Save** (Ctrl+S)로 씬 저장
2. **Play** 버튼 눌러서 테스트
3. 문제 없으면 커밋!

```bash
git add Assets/StarterAssets/ThirdPersonController/Scenes/Playground.unity
git commit -m "fix: Unity 씬 수정 - 유령 아이템 제거, 책상 콜라이더 추가"
```
