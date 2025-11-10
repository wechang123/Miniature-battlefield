# 무기 시스템 구현 완료 가이드

## 📋 목차
1. [구현된 기능](#구현된-기능)
2. [파일 구조](#파일-구조)
3. [Unity에서 설정하기](#unity에서-설정하기)
4. [WeaponData 생성하기](#weapondata-생성하기)
5. [발사체 프리팹 만들기](#발사체-프리팹-만들기)
6. [플레이어에 적용하기](#플레이어에-적용하기)
7. [적에게 적용하기](#적에게-적용하기)
8. [테스트하기](#테스트하기)

---

## 구현된 기능

### ✅ Combat 시스템
- **IDamageable** - 데미지를 받을 수 있는 인터페이스
- **DamageInfo** - 데미지 정보 구조체 (데미지, 타입, 넉백 등)
- **EnemyHealth** - 적 체력 시스템 (데미지, 회복, 죽음 처리)

### ✅ 무기 시스템
- **WeaponData** (ScriptableObject) - 무기 데이터 (데미지, 발사속도, 탄약 등)
- **WeaponController** - 무기 발사 관리 (발사, 재장전, 탄약 관리)
- **PlayerWeaponHandler** - 플레이어 입력 처리 (마우스 클릭, 무기 교체)

### ✅ 발사체 3종
1. **PencilSpearProjectile** (연필창)
   - 특징: 적을 관통하며 날아감 (최대 3회)
   - 관통할수록 데미지 감소 (80%)

2. **EraserBombProjectile** (지우개폭탄)
   - 특징: 충돌 시 범위 폭발 데미지
   - 폭발 반경 3m, 거리에 따라 데미지 감소

3. **RubberBandProjectile** (고무줄 슬링)
   - 특징: 빠른 원거리 공격 + 적 슬로우 효과
   - 슬로우 2초 지속, 50% 감속

---

## 파일 구조

```
Assets/_Project/Scripts/Core/Gameplay/
├── Combat/
│   ├── IDamageable.cs          - 데미지 받는 인터페이스
│   ├── DamageInfo.cs           - 데미지 정보 구조체
│   └── EnemyHealth.cs          - 적 체력 시스템
├── Weapons/
│   ├── WeaponData.cs           - 무기 데이터 (ScriptableObject)
│   ├── WeaponController.cs     - 무기 발사 관리
│   └── PlayerWeaponHandler.cs  - 플레이어 입력 처리
└── Projectiles/
    ├── WeaponProjectileBase.cs     - 발사체 기본 클래스
    ├── PencilSpearProjectile.cs    - 연필창 발사체
    ├── EraserBombProjectile.cs     - 지우개폭탄 발사체
    └── RubberBandProjectile.cs     - 고무줄 슬링 발사체
```

---

## Unity에서 설정하기

### 1. WeaponData 생성하기

1. **Project 창**에서 우클릭 → `Create → YajaGame → Weapons → Weapon Data`
2. 3개의 WeaponData 에셋 생성:
   - `PencilSpear_Data`
   - `EraserBomb_Data`
   - `RubberBandSling_Data`

#### 연필창 설정 예시
```
Weapon Name: 연필창
Weapon Type: PencilSpear
Damage: 30
Damage Type: Physical
Knockback Force: 3
Projectile Speed: 25
Projectile Lifetime: 5
Fire Rate: 2 (초당 2발)
Max Ammo: -1 (무한)
```

#### 지우개폭탄 설정 예시
```
Weapon Name: 지우개폭탄
Weapon Type: EraserBomb
Damage: 50
Damage Type: Explosion
Knockback Force: 10
Projectile Speed: 15
Projectile Lifetime: 5
Fire Rate: 1 (초당 1발)
Max Ammo: 5
Reload Time: 2
```

#### 고무줄 슬링 설정 예시
```
Weapon Name: 고무줄 슬링
Weapon Type: RubberBandSling
Damage: 20
Damage Type: Impact
Knockback Force: 2
Projectile Speed: 30
Projectile Lifetime: 3
Fire Rate: 3 (초당 3발)
Max Ammo: 10
Reload Time: 1.5
```

---

## 발사체 프리팹 만들기

### 연필창 프리팹
1. **빈 GameObject** 생성 → 이름: `PencilSpear_Projectile`
2. **3D Model** 추가 (Cylinder 또는 커스텀 모델)
   - Scale: (0.05, 0.5, 0.05)
   - Rotation: (90, 0, 0)
3. **Rigidbody** 추가
   - Use Gravity: ✅
   - Collision Detection: Continuous Dynamic
4. **Capsule Collider** 추가
   - Radius: 0.1
   - Height: 1
   - Direction: Y-Axis
5. **PencilSpearProjectile** 스크립트 추가
   - Max Penetrations: 3
   - Penetration Damage Multiplier: 0.8
   - Rotate While Flying: ✅
   - Rotation Speed: 720
6. **Prefab**으로 저장

### 지우개폭탄 프리팹
1. **빈 GameObject** 생성 → 이름: `EraserBomb_Projectile`
2. **3D Model** 추가 (Cube 또는 커스텀 모델)
   - Scale: (0.2, 0.2, 0.2)
3. **Rigidbody** 추가
4. **Box Collider** 추가
5. **EraserBombProjectile** 스크립트 추가
   - Explosion Radius: 3
   - Explosion Damage Multiplier: 1.5
   - Explosion Force: 500
   - Fuse Time: 3
   - Explode On Impact: ✅
6. **Prefab**으로 저장

### 고무줄 슬링 프리팹
1. **빈 GameObject** 생성 → 이름: `RubberBand_Projectile`
2. **3D Model** 추가 (Sphere 또는 커스텀 모델)
   - Scale: (0.1, 0.1, 0.1)
3. **Rigidbody** 추가
4. **Sphere Collider** 추가
   - Radius: 0.1
5. **Trail Renderer** 추가 (선택 사항)
   - Time: 0.5
   - Width: 0.05
   - Color: 연한 파란색
6. **RubberBandProjectile** 스크립트 추가
   - Slow Duration: 2
   - Slow Intensity: 0.5
7. **Prefab**으로 저장

---

## 플레이어에 적용하기

### 1. Player GameObject 설정

1. **Player GameObject** 선택
2. **WeaponController** 컴포넌트 추가
3. **PlayerWeaponHandler** 컴포넌트 추가

### 2. Fire Point 생성

```
PlayerArmature
└── Spine
    └── Chest
        └── RightShoulder
            └── RightHand
                └── FirePoint (빈 GameObject)
```

- FirePoint Position: (0.3, 0, 0.5) - 손 앞쪽
- FirePoint을 WeaponController의 Fire Point에 할당

### 3. WeaponController 설정

- **Current Weapon**: PencilSpear_Data (처음 장착할 무기)
- **Fire Point**: FirePoint GameObject

### 4. PlayerWeaponHandler 설정

- **Input**: StarterAssetsInputs (자동 할당됨)
- **Fire Point**: FirePoint GameObject
- **Available Weapons** (크기: 3):
  - Element 0: PencilSpear_Data
  - Element 1: EraserBomb_Data
  - Element 2: RubberBandSling_Data
- **Use Mouse For Fire**: ✅
- **Use Throw Button For Fire**: ✅

### 5. WeaponData에 발사체 연결

각 WeaponData 에셋에서:
- **Projectile Prefab**: 해당 발사체 프리팹 할당
  - PencilSpear_Data → PencilSpear_Projectile
  - EraserBomb_Data → EraserBomb_Projectile
  - RubberBandSling_Data → RubberBand_Projectile

---

## 적에게 적용하기

### 1. 선생님 GameObject 설정

1. **선생님 GameObject** 선택 (TeacherPatrolAI가 있는 오브젝트)
2. **EnemyHealth** 컴포넌트 추가

### 2. EnemyHealth 설정

```
Max Health: 100
Hit Flash Duration: 0.1
Hit Flash Color: Red
Death Delay: 2
```

### 3. Tag 설정

- GameObject Tag: `Enemy` (반드시 설정!)

---

## 테스트하기

### 조작법

#### 발사
- **마우스 좌클릭** 또는 **우클릭** - 무기 발사

#### 재장전
- **R 키** - 재장전

#### 무기 교체
- **1, 2, 3 키** - 무기 직접 선택
- **마우스 휠** - 무기 순환 교체

### 테스트 시나리오

1. **기본 발사 테스트**
   - 플레이 모드 실행
   - 마우스 좌클릭으로 발사
   - Console에서 발사 로그 확인

2. **적 데미지 테스트**
   - 선생님에게 발사
   - 선생님이 빨갛게 깜빡이는지 확인
   - Console에서 데미지 로그 확인

3. **연필창 관통 테스트**
   - 여러 적을 일렬로 배치
   - 연필창 발사
   - 관통 효과 확인

4. **지우개폭탄 폭발 테스트**
   - 여러 적을 모아놓기
   - 지우개폭탄 발사
   - 범위 폭발 확인

5. **고무줄 슬링 슬로우 테스트**
   - 순찰 중인 선생님에게 발사
   - 이동 속도가 느려지는지 확인

### 디버그 명령어

**WeaponController**에서 우클릭:
- `Print Weapon Info` - 현재 무기 정보 출력

**PlayerWeaponHandler**에서 우클릭:
- `Print Available Weapons` - 사용 가능한 무기 목록

**EnemyHealth**에서 우클릭:
- `Print Health` - 현재 체력 출력
- `Kill Enemy` - 즉시 죽이기

---

## 트러블슈팅

### 발사체가 생성되지 않음
✅ WeaponData의 Projectile Prefab이 할당되었는지 확인
✅ Fire Point가 설정되었는지 확인

### 적이 데미지를 받지 않음
✅ 적 GameObject의 Tag가 "Enemy"인지 확인
✅ EnemyHealth 컴포넌트가 있는지 확인
✅ WeaponProjectileBase의 Hit Layers에 적 레이어가 포함되어 있는지 확인

### 발사체가 날아가지 않음
✅ Rigidbody가 있는지 확인
✅ Use Gravity가 켜져 있는지 확인
✅ Collider가 있고 Is Trigger가 꺼져 있는지 확인

### 무기 교체가 안 됨
✅ PlayerWeaponHandler의 Available Weapons 배열에 무기가 있는지 확인
✅ WeaponData 에셋이 null이 아닌지 확인

---

## 다음 단계

1. **이펙트 추가**
   - 머즐 플래시 (발사 시 불꽃)
   - 히트 이펙트 (충돌 시 파티클)
   - 폭발 이펙트 (지우개폭탄)

2. **사운드 추가**
   - 발사 사운드
   - 충돌 사운드
   - 폭발 사운드
   - 재장전 사운드

3. **UI 연동**
   - 탄약 표시
   - 무기 아이콘
   - 크로스헤어

4. **밸런싱**
   - 데미지 조절
   - 발사속도 조절
   - 탄약 수 조절

---

## 추가 정보

### WeaponData 파라미터 설명

| 파라미터 | 설명 | 예시 |
|---------|------|------|
| Damage | 기본 데미지 | 30 |
| Fire Rate | 초당 발사 횟수 | 2 (1초에 2발) |
| Max Ammo | 최대 탄약 (-1=무한) | 10 |
| Reload Time | 재장전 시간 (초) | 2 |
| Projectile Speed | 발사체 속도 | 25 |
| Projectile Lifetime | 발사체 수명 (초) | 5 |
| Knockback Force | 넉백 힘 | 5 |

### DamageType 종류

- **Physical**: 물리 데미지 (연필창)
- **Explosion**: 폭발 데미지 (지우개폭탄)
- **Impact**: 충격 데미지 (고무줄 슬링)

---

**구현 완료!** 🎉

질문이 있으면 언제든지 물어보세요!
