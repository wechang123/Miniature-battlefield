# NavMesh 간단 설정법 (Object 탭이 없을 때)

## Navigation 윈도우에 Agents, Areas만 있는 경우

현재 상황: Navigation 윈도우에 **Agents**와 **Areas** 탭만 보임

### 해결방법 1: Inspector에서 직접 설정 (가장 간단!)

#### 1단계: 교실 바닥을 Navigation Static으로 설정

1. **Hierarchy에서 교실 바닥 오브젝트 선택**
   - 예: School 오브젝트 또는 Floor, Ground 등

2. **Inspector 창 확인** (오른쪽)
   - 맨 위에 오브젝트 이름이 보임
   - 그 아래에 **Static** 체크박스와 드롭다운이 있음

3. **Static 드롭다운 클릭** (▼ 화살표)
   - 여러 옵션이 나타남:
     ```
     ☐ Nothing
     ☐ Everything
     ☐ Contribute GI
     ☐ Occluder Static
     ☐ Occludee Static
     ☑ Navigation Static  ← 이것만 체크!
     ☐ Off Mesh Link Generation
     ☐ Reflection Probe Static
     ```

4. **☑ Navigation Static 체크**
   - 다른 것은 건드리지 않아도 됨

5. **자식 오브젝트에도 적용하겠냐는 팝업이 뜨면**
   - **Yes, change children** 클릭 (전체 바닥에 적용)

#### 2단계: Bake 탭 찾기

Navigation 윈도우를 다시 확인해보세요:
- **Agents | Areas | Bake** 탭이 있어야 함
- Bake 탭이 보이지 않으면 다음 방법 시도:

**방법 A: 윈도우 크기 조정**
- Navigation 윈도우를 넓게 늘려보기
- Bake 탭이 숨어있을 수 있음

**방법 B: Navigation 윈도우 재열기**
1. Navigation 윈도우 닫기
2. 상단 메뉴: **Window > AI > Navigation**
3. 다시 열면 Bake 탭이 나타날 수 있음

**방법 C: AI Navigation 패키지 확인**
1. **Window > Package Manager**
2. 왼쪽 상단 드롭다운에서 **Unity Registry** 선택
3. 검색: `AI Navigation`
4. **AI Navigation** 패키지 찾기
5. 설치되어 있지 않으면 **Install** 클릭
6. 설치되어 있으면 최신 버전인지 확인

#### 3단계: Bake 실행

Bake 탭을 찾았다면:

1. **Bake 탭 클릭**
2. 설정 확인:
   ```
   Agent Radius: 0.5
   Agent Height: 2.0
   Max Slope: 45
   ```
3. **화면 하단의 Bake 버튼 클릭**
4. 잠시 대기 (처리 중...)
5. **Scene 뷰에 파란색 영역 나타나면 성공!**

### 해결방법 2: Bake 탭이 정말 없는 경우

구버전 Unity이거나 다른 Navigation 시스템을 사용하는 경우:

#### 대체 방법: 메뉴에서 직접 베이킹

1. **교실 바닥 오브젝트를 Navigation Static으로 설정** (위의 1단계)

2. **상단 메뉴에서:**
   - **Window > AI > Navigation**
   - 또는
   - **Window > Navigation (Obsolete)** (구버전)

3. **Navigation 윈도우가 다르게 보일 수 있음:**
   - Object, Bake, Areas 탭이 있는 버전
   - 이 경우 **Bake 탭으로 이동해서 Bake 버튼 클릭**

## 3단계: NavMesh 확인

베이킹이 완료되면:

1. **Scene 뷰 (가운데 3D 화면) 확인**
2. **파란색 반투명 영역**이 교실 바닥에 나타남
3. 이것이 NavMesh = 선생님이 걸을 수 있는 구역

### NavMesh가 안 보이는 경우

**Scene 뷰 상단 메뉴 확인:**
- **Shading** 버튼 옆에 **Gizmos** 버튼 있음
- Gizmos 체크되어 있는지 확인
- Gizmos 드롭다운 클릭 > **Navigation** 또는 **NavMesh** 항목 체크

## 간단 요약

### ✅ 필수 3단계만 기억하세요:

1. **바닥 선택 → Inspector → Static 드롭다운 → Navigation Static 체크**
2. **Navigation 윈도우 → Bake 탭 → Bake 버튼**
3. **Scene 뷰에 파란색 영역 확인**

이것만 되면 50% 완료!

---

## 다음 단계: 순찰 포인트와 선생님 설정

NavMesh 베이킹이 완료되면:
1. 순찰 포인트 생성 (Empty GameObject 4개)
2. 선생님 FBX를 씬에 추가
3. 컴포넌트 3개 추가 (NavMeshAgent, Animator, TeacherPatrolAI)

자세한 내용은 `NavMesh_베이킹_상세가이드.md` 파일의 6단계부터 참고하세요!

## 스크린샷으로 보는 위치

### Inspector에서 Static 설정 위치:
```
Inspector (오른쪽 창)
┌─────────────────────────┐
│ 🔵 GameObject 이름       │
│ ☑ Static ▼    Tag      │  ← 여기!
│    └─ Navigation Static │
│                         │
│ Transform               │
│   Position  (0,0,0)     │
│   ...                   │
└─────────────────────────┘
```

### Navigation 윈도우 구조:
```
┌─ Navigation ─────────┐
│ Agents | Areas | Bake │ ← Bake 탭 찾기
└──────────────────────┘
```

---

막히는 부분이 있으면 스크린샷 찍어서 보여주세요!
