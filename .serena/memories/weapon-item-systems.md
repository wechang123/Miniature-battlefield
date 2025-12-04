# 무기 및 아이템 시스템 상세

## 무기 시스템

### WeaponController.cs
- 플레이어 무기 발사 관리
- ScriptableObject 기반 (WeaponData)
- 기능:
  - TryFire(): 발사 시도 (쿨다운, 탄약 체크)
  - TryReload(): 재장전
  - ChangeWeapon(): 무기 변경
  - GetFireDirection(): 화면 중앙 기준 조준

### WeaponProjectileBase.cs (추상 클래스)
- 모든 투사체의 기본 클래스
- Rigidbody + Collider 필수
- Initialize(WeaponData, direction): 초기화
- OnCollisionEnter → HandleEnemyHit / HandleEnvironmentHit
- DamageInfo 생성하여 IDamageable에 데미지 전달

### 투사체 종류
- PencilSpearProjectile: 연필창 (기본)
- EraserBombProjectile: 지우개폭탄 (폭발)
- RubberBandProjectile: 고무줄 (미구현?)

## 아이템 시스템

### ItemCarrySystem.cs
- 한 번에 하나의 아이템만 캐리 가능
- CarryPosition 자동 탐색 (또는 자동 생성)
- 아이템 타입별 다른 위치/스케일 설정:
  - 기본(연필창): carryOffset, carryRotation, carryScale
  - 지우개폭탄: eraserBombOffset, eraserBombRotation, eraserBombScale
  - 커스텀: UseCustomCarrySettings 플래그
- PickupItem(): 들기 (물리/충돌 비활성화)
- ReleaseItem(): 놓기 (던지기 전 준비)
- ForceDropItem(): 강제 드롭

### ItemBase.cs (추상 클래스)
- IPickupable 구현
- ItemType: WeaponPart, Consumable, Currency
- 애니메이션: 상하 움직임 (bobHeight), 회전 (rotationSpeed)
- OnPickup(): 수집 시 이펙트/사운드 → ProcessPickup() → Destroy

### 아이템 타입별 스크립트
- **WeaponPartItem**: 무기 부품 아이템
  - OnEquipped(): 장착 시 애니메이션 중지
  - OnUnequipped(): 해제 시 애니메이션 재개
- **EraserBombItem**: 지우개폭탄 아이템
- **MeleeWeaponItem**: 근접무기 아이템

## 던지기 시스템

### ItemThrowSystem.cs
- ItemCarrySystem와 연동
- 던지기 궤적 예측 (TrajectoryPredictor)
- 무기 타입별 다른 투사체 생성

### ThrownProjectile.cs
- 던져진 아이템 처리
- 착지 후 아이템으로 변환 또는 폭발

## 흐름 정리

### 아이템 줍기
1. PlayerInteraction이 주변 IPickupable 감지
2. ItemCarrySystem.PickupItem() 호출
3. 아이템 CarryPosition에 부착
4. Collider/Rigidbody 비활성화

### 아이템 던지기
1. 던지기 입력 감지
2. ItemCarrySystem.ReleaseItem()
3. ItemThrowSystem이 투사체 생성
4. ThrownProjectile 또는 WeaponProjectileBase 처리

### 데미지 처리
1. 투사체 충돌 감지
2. IDamageable 검색
3. DamageInfo 생성 (데미지, 타입, 넉백)
4. TakeDamage() 호출
5. 체력 감소 + 넉백 적용

## 주요 인터페이스

```csharp
// IPickupable
Transform Transform { get; }
bool IsPickable { get; }
void OnPickup();

// IDamageable
void TakeDamage(DamageInfo damageInfo);
float CurrentHealth { get; }
float MaxHealth { get; }
bool IsAlive { get; }
Transform Transform { get; }
```
