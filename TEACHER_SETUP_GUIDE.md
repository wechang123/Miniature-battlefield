# 선생님 순찰 AI 설정 가이드

협업자2가 추가한 선생님 캐릭터를 교실 구간을 순찰하도록 설정하는 방법입니다.

## 1. NavMesh 베이킹 (교실 바닥 설정)

1. Unity 에디터에서 `Window > AI > Navigation` 메뉴 열기
2. 교실 바닥 오브젝트를 선택
3. Inspector에서 `Navigation Static` 체크박스 활성화
4. Navigation 윈도우에서 `Bake` 탭으로 이동
5. `Bake` 버튼 클릭하여 NavMesh 생성
   - 파란색 영역이 선생님이 걸어다닐 수 있는 구역입니다

## 2. 순찰 포인트 생성

1. Hierarchy에서 빈 GameObject 생성 (이름: `PatrolPoints`)
2. `PatrolPoints` 하위에 빈 GameObject들을 생성:
   - `PatrolPoint1`
   - `PatrolPoint2`
   - `PatrolPoint3`
   - `PatrolPoint4`
   (원하는 만큼 추가 가능)

3. Scene 뷰에서 각 포인트를 선생님이 지나가길 원하는 위치로 이동
   - 교실 앞
   - 복도
   - 교실 뒤
   - 등등...

## 3. 선생님 캐릭터 설정

### 3-1. 씬에 선생님 추가
1. `Assets/scary-teacher/source/Scary_Teacher-1.fbx`를 Hierarchy에 드래그
2. 이름을 `Teacher`로 변경

### 3-2. 필요한 컴포넌트 추가

선생님 GameObject를 선택하고 다음 컴포넌트들을 추가:

#### A. Nav Mesh Agent
- `Add Component > Nav Mesh Agent`
- 설정:
  - Speed: `1.5`
  - Angular Speed: `120`
  - Acceleration: `8`
  - Stopping Distance: `0.5`
  - Radius: `0.5`
  - Height: `2` (선생님 캐릭터 키에 맞게 조정)

#### B. Animator
- `Add Component > Animator`
- 설정:
  - Controller: `Assets/StarterAssets/ThirdPersonController/Character/Animations/StarterAssetsThirdPerson.controller`
  - Avatar: Scary_Teacher-1 모델의 Avatar 자동 설정됨
  - Apply Root Motion: 체크 해제

#### C. Teacher Patrol AI 스크립트
- `Add Component > Teacher Patrol AI`
- 설정:
  - Patrol Points 배열 크기 설정 (예: 4)
  - 각 슬롯에 위에서 만든 PatrolPoint1, 2, 3, 4를 드래그
  - Wait Time At Point: `2` (각 포인트에서 2초 대기)
  - Walk Speed: `1.5`

## 4. 테스트

1. Play 버튼 클릭
2. 선생님이 설정한 순찰 포인트들을 순서대로 이동하는지 확인
3. 걷는 애니메이션이 재생되는지 확인

## 5. 선생님 프리팹으로 저장 (선택사항)

설정이 완료되면:
1. Hierarchy의 Teacher GameObject를 선택
2. `Assets/StarterAssets/ThirdPersonController/Prefabs/` 폴더로 드래그
3. 프리팹 이름: `TeacherPatrol.prefab`

## 문제 해결

### 선생님이 움직이지 않는 경우
- NavMesh가 제대로 베이킹되었는지 확인 (파란색 영역)
- 순찰 포인트들이 NavMesh 위에 있는지 확인
- Console에 에러 메시지가 있는지 확인

### 애니메이션이 재생되지 않는 경우
- Animator Controller가 제대로 설정되었는지 확인
- Avatar가 선생님 모델에 맞게 설정되었는지 확인
- Apply Root Motion이 체크 해제되어 있는지 확인

### 선생님이 벽을 통과하는 경우
- 벽 오브젝트에 Collider가 있는지 확인
- NavMesh 베이킹 시 벽이 Obstacle로 인식되도록 설정

## 추가 기능 구현 아이디어

- 플레이어 발견 시 추격 시스템
- 시야각(Field of View) 구현
- 순찰 패턴 랜덤화
- 선생님 여러 명 배치
- 플레이어와 충돌 시 게임오버

## 참고 스크립트

선생님 AI 스크립트: `Assets/StarterAssets/ThirdPersonController/Scripts/TeacherPatrolAI.cs`
