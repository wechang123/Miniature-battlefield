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

**문제**: Playground 씬에 숨겨진 `PencilSpear_Item` 오브젝트가 존재합니다.

**해결**:
1. **Hierarchy** 창 상단 검색창에 `PencilSpear_Item` 입력
2. 찾은 오브젝트를 선택
3. **Delete** 키 또는 우클릭 → **Delete** 클릭
4. **File > Save** (Ctrl+S) 저장

---

### 2. 책상 콜라이더 추가 (선생님 AI 통과 방지)

**문제**: 선생님이 책상 서랍/상판을 뚫고 지나갑니다.

**해결**:

#### 방법 A: Box Collider 추가 (권장)

1. **Hierarchy**에서 School 확장
2. 책상 본체(desk body/drawer) 메시 선택
3. **Inspector** 하단 **Add Component** 클릭
4. `Box Collider` 검색 후 추가
5. **Edit Collider** 버튼 클릭
6. 초록색 핸들로 콜라이더 크기 조절 (책상 본체 전체 덮게)
7. 씬의 모든 책상에 반복

#### 방법 B: NavMesh Obstacle 추가 (대안)

1. 책상 본체 선택
2. **Add Component** → `Nav Mesh Obstacle`
3. **Carve** 체크박스 활성화
4. **Size**: 책상 크기에 맞게 조절
5. **Center**: 책상 중심에 맞게 조절

---

### 3. NavMesh 재베이킹

책상 콜라이더 추가 후:

1. **Window > AI > Navigation** 열기
2. **Agents** 탭에서 설정 확인:
   - Agent Radius: **0.5**
   - Agent Height: **2.0**
   - Max Slope: **45**
   - Step Height: **0.4**

3. **Areas** 탭:
   - Default = Walkable (확인)

4. **Hierarchy**에서 **바닥/교실 오브젝트** 선택
   - Inspector 우측 상단 **Static** 드롭다운 클릭
   - **Navigation Static** 체크
   - "Change children too?" → **Yes, change children**

5. Navigation 창에서 **Bake** 버튼 클릭

6. **Scene 뷰에서 확인**:
   - 책상 주변 NavMesh(파란색)가 사라졌는지 확인
   - 책상을 피해서 경로가 생성되는지 확인

---

## 🎮 테스트 체크리스트

### 문제 2+3: 연필창 줍기
- [ ] 게임 시작 시 E 키를 눌러도 아무 일도 일어나지 않음
- [ ] 바닥에 생성된 연필창 근처에서 E 키로 줍기 가능
- [ ] 지우개폭탄도 정상적으로 줍기 가능

### 문제 4: 아이템 높이
- [ ] 아이템이 바닥 0.4m 정도 높이에 떠있음 (이전보다 낮음)
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

### 연필창이 여전히 허공에서 주워짐
→ Hierarchy에서 `PencilSpear`로 검색하여 모든 오브젝트 확인 후 삭제

### 선생님이 여전히 책상을 통과함
→ 책상 콜라이더가 제대로 설정되었는지 확인 (Scene 뷰에서 초록색 윤곽선)
→ NavMesh 재베이킹 확인 (파란색 영역이 책상 주변에 없어야 함)

### UI가 표시되지 않음
→ Canvas 설정 확인:
  - Canvas > Render Mode: **Screen Space - Camera**
  - Canvas > Render Camera: **Main Camera**로 설정

### 아이템이 여전히 높이 떠있음
→ Unity에서 기존 ItemSpawner/RandomItemSpawner 오브젝트의 Inspector 확인:
  - Spawn Height: 0.3으로 변경
→ ItemBase 프리팹의 Inspector 확인:
  - Bob Height: 0.1로 변경

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
