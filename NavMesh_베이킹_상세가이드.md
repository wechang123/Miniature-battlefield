# NavMesh 베이킹 상세 가이드

## 현재 상태
Navigation 윈도우가 열려있고 Agents 탭이 선택되어 있습니다.

## 단계별 설정 방법

### 1단계: Areas 탭으로 이동
현재 **Agents** 탭에서 → **Areas** 탭으로 전환
- 특별한 설정 필요 없음 (기본값 사용)

### 2단계: Object 탭에서 교실 바닥 설정

1. **Navigation 윈도우 상단의 Object 탭 클릭**
   (Agents, Areas 옆에 있어야 함. 없다면 Window > AI > Navigation > Object)

2. **Hierarchy에서 교실 바닥 오브젝트 선택**
   - 학교 환경에서 바닥/Floor 오브젝트를 찾기
   - 예: `School > Floor`, `Ground`, `Classroom_Floor` 등

3. **Inspector 또는 Navigation Object 탭에서 설정**
   - ☑️ **Navigation Static** 체크박스 활성화
   - Navigation Area: **Walkable** 선택 (기본값)

4. **여러 바닥이 있다면 모두 선택해서 설정**
   - Shift 또는 Ctrl 키로 여러 오브젝트 선택
   - 모두 Navigation Static 활성화

### 3단계: Bake 탭에서 베이킹 설정

1. **Navigation 윈도우의 Bake 탭 클릭**

2. **설정 확인** (선생님 캐릭터에 맞게):
   ```
   Agent Radius: 0.5
   Agent Height: 2.0
   Max Slope: 45
   Step Height: 0.75
   ```
   이 값들은 현재 Agents 탭에서 보이는 Humanoid 설정과 일치해야 합니다.

3. **Bake 버튼 클릭** (탭 하단)
   - 처리 시간: 몇 초 ~ 수십 초 (씬 크기에 따라)
   - 완료되면 Scene 뷰에 **파란색 영역**이 나타남
   - 이 파란색 영역이 선생님이 걸을 수 있는 구역

### 4단계: NavMesh 확인

1. **Scene 뷰에서 확인**
   - 파란색으로 표시된 영역 = NavMesh (걸을 수 있는 곳)
   - 교실 바닥 전체가 파란색이어야 함
   - 벽이나 장애물 주변은 자동으로 제외됨

2. **NavMesh가 안 보이면**
   - Scene 뷰 상단 메뉴에서 Gizmos가 켜져있는지 확인
   - Gizmos 드롭다운 > Navigation (NavMesh) 체크

### 5단계: 문제 해결

#### 파란색 영역이 안 보이는 경우
- Bake 탭에서 다시 Bake 버튼 클릭
- 교실 바닥 오브젝트가 Navigation Static인지 재확인
- Console에 에러 메시지 확인

#### 일부 구역만 파란색인 경우
- 해당 구역의 바닥 오브젝트가 Navigation Static이 아닐 수 있음
- Hierarchy에서 바닥 오브젝트를 모두 찾아서 설정

#### 벽을 통과하는 NavMesh가 생성된 경우
- 벽 오브젝트를 선택
- Navigation Static 체크
- Navigation Area를 **Not Walkable**로 설정
- 다시 Bake

## 6단계: 순찰 포인트 생성

NavMesh 베이킹이 완료되면 순찰 포인트를 만듭니다:

1. **Hierarchy 우클릭 > Create Empty**
   - 이름: `PatrolPoints`

2. **PatrolPoints 오브젝트 선택 후, 다시 우클릭 > Create Empty**
   - 자식 오브젝트들 생성:
     - `PatrolPoint1`
     - `PatrolPoint2`
     - `PatrolPoint3`
     - `PatrolPoint4`

3. **Scene 뷰에서 각 포인트 위치 조정**
   - 각 PatrolPoint를 선택
   - Move Tool (W 키)로 이동
   - **반드시 파란색 NavMesh 위에 배치**
   - 예시 위치:
     - Point1: 교실 앞쪽 (칠판 근처)
     - Point2: 교실 중앙 (책상 사이)
     - Point3: 교실 뒤쪽
     - Point4: 교실 문 근처

4. **포인트들이 NavMesh 위에 있는지 확인**
   - Scene 뷰를 위에서 내려다보기 (Numpad 7 또는 마우스로 회전)
   - 포인트가 파란색 영역 안에 있어야 함

## 7단계: 선생님 캐릭터 추가

1. **Project 윈도우에서 FBX 찾기**
   ```
   Assets/scary-teacher/source/Scary_Teacher-1.fbx
   ```

2. **Hierarchy로 드래그**
   - Scary_Teacher-1 오브젝트가 생성됨
   - 이름을 `Teacher`로 변경

3. **위치 조정**
   - Inspector에서 Transform > Position
   - 교실 바닥 위에 배치 (Y 좌표 조정)

## 8단계: 선생님 컴포넌트 설정

### Teacher 오브젝트를 선택한 상태에서:

#### A. Nav Mesh Agent 추가
1. Inspector 하단 **Add Component** 클릭
2. 검색: `Nav Mesh Agent`
3. 선택하여 추가
4. 설정:
   - **Agent Type**: Humanoid
   - **Base Offset**: 0
   - **Speed**: 1.5
   - **Angular Speed**: 120
   - **Acceleration**: 8
   - **Stopping Distance**: 0.5
   - **Auto Braking**: ☑️ 체크
   - **Radius**: 0.5
   - **Height**: 2

#### B. Animator 추가
1. **Add Component** > `Animator`
2. 설정:
   - **Controller**:
     - 드롭다운 클릭 또는 오른쪽 ⊙ 버튼 클릭
     - 검색: `StarterAssetsThirdPerson`
     - `Assets/StarterAssets/ThirdPersonController/Character/Animations/StarterAssetsThirdPerson.controller` 선택
   - **Avatar**:
     - 자동으로 설정됨 (Scary_Teacher-1Avatar)
   - **Apply Root Motion**: ☐ 체크 해제
   - **Update Mode**: Normal
   - **Culling Mode**: Always Animate

#### C. Teacher Patrol AI 스크립트 추가
1. **Add Component** 클릭
2. 검색: `Teacher Patrol AI`
3. 선택하여 추가
4. 설정:
   - **Patrol Points**:
     - Size를 `4`로 설정 (순찰 포인트 개수)
     - Element 0: Hierarchy에서 PatrolPoint1을 드래그
     - Element 1: PatrolPoint2 드래그
     - Element 2: PatrolPoint3 드래그
     - Element 3: PatrolPoint4 드래그
   - **Wait Time At Point**: 2
   - **Walk Speed**: 1.5
   - **Speed Parameter Name**: Speed
   - **Motion Speed Parameter Name**: MotionSpeed

## 9단계: 테스트

1. **Play 버튼 (▶) 클릭**

2. **확인사항**:
   - ✅ 선생님이 PatrolPoint1로 걷기 시작
   - ✅ 걷는 애니메이션이 재생됨
   - ✅ Point1 도착 후 2초 대기
   - ✅ Point2로 이동
   - ✅ Point3, Point4를 거쳐 다시 Point1로 순환

3. **문제가 있다면**:
   - Console 창 확인 (Window > General > Console)
   - 에러 메시지 읽기

## 10단계: 프리팹으로 저장 (선택사항)

설정이 완료되면 재사용을 위해 프리팹으로 저장:

1. Hierarchy에서 **Teacher** 오브젝트 선택
2. Project 윈도우의 `Assets/StarterAssets/ThirdPersonController/Prefabs/` 폴더 열기
3. Teacher를 Prefabs 폴더로 **드래그**
4. 프리팹 생성됨: `Teacher.prefab`

## 완료!

이제 선생님이 교실을 순찰합니다!

## 추가 팁

### 순찰 경로 시각화
- Hierarchy에서 Teacher 선택
- Scene 뷰에서 노란색 선으로 순찰 경로가 표시됨

### 속도 조정
- Teacher 선택 > Teacher Patrol AI 컴포넌트
- Walk Speed 값 변경 (1.0 = 느리게, 2.0 = 빠르게)

### 순찰 포인트 추가
- PatrolPoints 하위에 새 Empty GameObject 생성
- 이름: PatrolPoint5
- Teacher Patrol AI > Patrol Points Size 증가
- 새 포인트 연결

### 대기 시간 조정
- Wait Time At Point 값 변경
- 0 = 대기 없음, 5 = 5초 대기
