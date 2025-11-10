# 근접 무기 시스템 설정 가이드

## 📋 목차
1. [연필창 프리팹 만들기](#1-연필창-프리팹-만들기)
2. [WeaponData 생성](#2-weapondata-생성)
3. [플레이어에 적용](#3-플레이어에-적용)
4. [테스트](#4-테스트)
5. [애니메이션 연동](#5-애니메이션-연동-선택)

---

## 1. 연필창 프리팹 만들기

### A) GameObject 구조 생성

```
PencilSpear_Melee (빈 GameObject)
├── PencilSpear (3D 모델 FBX)
└── Hitbox (빈 GameObject)
```

### B) 단계별 설정

#### **1) PencilSpear_Melee 생성**
- Hierarchy 우클릭 → Create Empty
- 이름: `PencilSpear_Melee`

#### **2) 3D 모델 추가**
- `Assets/_Project/Models/Weapons/PencilSpear/PencilSpear.fbx`를 자식으로 드래그
- Transform:
  - Position: (0, 0, 0)
  - Rotation: (0, 0, 0)
  - Scale: (0.2, 0.2, 0.2)

#### **3) PencilSpearMelee 스크립트 추가**
- `PencilSpear_Melee` 선택
- Add Component → PencilSpearMelee
- 설정:
  ```
  Attack Cooldown: 0.4
  Max Hits Per Attack: 3
  Damage Multiplier Per Hit: 0.9
  Show Trail Effect: ✅
  ```

#### **4) Hitbox 생성**
- `PencilSpear_Melee` 우클릭 → Create Empty
- 이름: `Hitbox`
- Transform:
  ```
  Position: (0, 0.5, 0)
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)
  ```

#### **5) Hitbox 컴포넌트 추가**
- Add Component → Box Collider
  ```
  Center: (0, 0, 0)
  Size: (0.3, 1, 0.3)
  Is Trigger: ✅
  ```
- Add Component → MeleeAttackHitbox
  ```
  Target Layers: Everything (또는 Enemy 레이어만)
  Debug Mode: ✅
  ```

#### **6) Prefab 저장**
- `PencilSpear_Melee`를 `Assets/_Project/Prefabs/Weapons/`로 드래그
- Hierarchy에서 원본 삭제

---

## 2. WeaponData 생성

### A) ScriptableObject 생성
1. Project 창에서 `Assets/_Project/` 우클릭
2. Create → YajaGame → Weapons → Weapon Data
3. 이름: `PencilSpear_MeleeData`

### B) 설정값

```
=== Basic Info ===
Weapon Name: 연필창
Weapon Type: PencilSpear
Weapon Icon: (없으면 비우기)
Description: 날카로운 연필로 만든 창. 빠르게 휘둘러 여러 적을 동시에 공격할 수 있다.

=== Combat Stats ===
Damage: 25
Damage Type: Physical
Knockback Force: 3

=== Projectile Settings ===
(근접 무기는 사용하지 않으므로 비우기)

=== Fire Settings ===
Fire Rate: 2.5
Max Ammo: -1
Reload Time: 0

=== Audio ===
Fire Sound: (타격 사운드, 있으면)
Reload Sound: (비우기)
Empty Sound: (비우기)

=== Visual Effects ===
Muzzle Flash Prefab: (비우기)
Hit Effect Prefab: (파티클 있으면)
```

---

## 3. 플레이어에 적용

### A) WeaponHolder 생성
1. Hierarchy에서 `PlayerArmature` 또는 `Player` 선택
2. 우클릭 → Create Empty
3. 이름: `WeaponHolder`
4. Transform을 손 위치로 조정:
   ```
   Position: (0.5, 1.0, 0.3) - 오른손 근처
   Rotation: (0, 0, 0)
   ```

### B) 연필창 프리팹 배치
1. `Assets/_Project/Prefabs/Weapons/PencilSpear_Melee` 프리팹을
2. `WeaponHolder`의 자식으로 드래그
3. Transform 조정 (손에 맞게)

### C) PencilSpearMelee 설정
1. `PencilSpear_Melee` 선택
2. Inspector:
   - Weapon Data: `PencilSpear_MeleeData` 드래그
   - Animator: Player의 Animator (자동 찾기 가능)

### D) PlayerMeleeController 추가
1. `Player` GameObject 선택
2. Add Component → PlayerMeleeController
3. 설정:
   ```
   Input: StarterAssetsInputs (자동 찾기)
   Melee Weapon: PencilSpear_Melee 드래그
   Use Mouse Left Click: ✅
   Use Attack Button: ✅
   ```

---

## 4. 테스트

### A) Play 모드 실행
1. Unity 에디터에서 Play 버튼 클릭
2. 씬 뷰를 Game 뷰로 전환

### B) 공격 테스트
1. **마우스 좌클릭** - 공격 실행
2. Console 창에서 로그 확인:
   ```
   [MeleeWeapon] 공격 시작: 연필창
   [MeleeWeapon] 히트박스 활성화
   [MeleeWeapon] 히트박스 비활성화
   [MeleeWeapon] 공격 종료
   ```

### C) Scene 뷰에서 확인
1. Play 모드 중 Scene 뷰 열기
2. Gizmos 켜기
3. 공격할 때 빨간색 히트박스가 보이는지 확인

### D) 적과 충돌 테스트
1. 선생님(Enemy) 근처에서 공격
2. Console에서 타격 로그 확인:
   ```
   [PencilSpear] EnemyName에게 25.0 데미지! (타격 1/3, 배율 1.00)
   ```

---

## 5. 애니메이션 연동 (선택)

### A) 애니메이션 없이 사용 (현재)
- 공격 버튼 누르면 즉시 히트박스 활성화
- 0.3초 후 비활성화
- 0.5초 후 공격 종료

### B) 애니메이션 추가 시
1. **공격 애니메이션 준비**
   - 연필창 휘두르기 애니메이션 클립

2. **Animator Controller 설정**
   - Attack 트리거 파라미터 추가
   - Attack 애니메이션 State 추가

3. **Animation Event 추가**
   - 애니메이션 클립 선택
   - 타격 타이밍에 Event 추가:
     - Function: `ActivateHitbox`
   - 공격 끝에 Event 추가:
     - Function: `DeactivateHitbox`
     - Function: `OnAttackAnimationEnd`

4. **PencilSpearMelee 설정**
   - Animator: Player의 Animator 할당
   - Attack Trigger Name: "Attack"

---

## 🔧 문제 해결

### 문제: 공격이 안 됨
- Console에서 에러 확인
- PlayerMeleeController가 Player에 추가되었는지 확인
- Melee Weapon 참조가 연결되었는지 확인

### 문제: 히트박스가 적을 감지 안 함
- Hitbox의 Collider가 Trigger로 설정되었는지 확인
- MeleeAttackHitbox의 Target Layers 확인
- Enemy에 Collider가 있는지 확인
- Enemy에 IDamageable (EnemyHealth) 컴포넌트가 있는지 확인

### 문제: 데미지가 안 들어감
- Enemy에 EnemyHealth 스크립트가 있는지 확인
- WeaponData의 Damage 값이 설정되었는지 확인
- Console에서 "[PencilSpear] 타격" 로그가 나오는지 확인

---

## ✅ 완료 체크리스트

- [ ] PencilSpear_Melee 프리팹 생성
- [ ] Hitbox 설정 완료
- [ ] WeaponData ScriptableObject 생성
- [ ] Player에 WeaponHolder 생성
- [ ] 연필창 프리팹 배치
- [ ] PlayerMeleeController 추가
- [ ] 모든 참조 연결
- [ ] Play 모드에서 공격 테스트
- [ ] 적 타격 테스트
- [ ] Console 로그 확인
- [ ] Scene 뷰에서 히트박스 시각화 확인

---

## 🎯 다음 단계

근접 무기 완성 후:
1. 원거리 무기 구현 (지우개폭탄, 고무줄 슬링)
2. 무기 전환 시스템
3. UI 연동
4. 애니메이션 추가
