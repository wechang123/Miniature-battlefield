# 지우개폭탄 설정 가이드

## 📦 파일 위치
- 모델: `Assets/_Project/Models/Weapons/EraserBomb/EraserBomb.fbx`
- 텍스처: `Assets/_Project/Models/Weapons/EraserBomb/EraserBomb_Texture.png`

## 🎯 프리팹 구조

```
EraserBomb_Projectile (GameObject)
├── Rigidbody
├── Sphere Collider
├── EraserBombProjectile (Script)
└── EraserBomb (3D Model FBX)
    └── Mesh Renderer (텍스처 자동 적용)
```

## ⚙️ 컴포넌트 설정

### Rigidbody
- Use Gravity: ✅
- Collision Detection: Continuous Dynamic
- Mass: 0.2

### Sphere Collider
- Radius: 0.15
- Is Trigger: ❌

### EraserBombProjectile
- Explosion Radius: 3
- Explosion Damage Multiplier: 1.5
- Explosion Force: 500
- Explosion Layers: Everything
- Use Fuse Timer: ❌
- Fuse Time: 3 (사용 안 함)

## 📊 WeaponData 설정

```
이름: EraserBomb_Data

Basic:
- Weapon Name: 지우개폭탄
- Damage: 40
- Damage Type: Explosion
- Knockback Force: 8

Projectile:
- Prefab: EraserBomb_Projectile
- Speed: 15
- Lifetime: 5
- Gravity Scale: 1

Fire:
- Fire Rate: 0.8
- Max Ammo: 5
- Reload Time: 2
```

## 🎮 사용 방법

### 플레이어 설정
1. WeaponController에 EraserBomb_Data 추가
2. Available Weapons 배열에 등록
3. 던지기 버튼 (G키 또는 우클릭) 매핑

### 테스트
1. Play 모드 실행
2. 던지기 버튼 누르기
3. 충돌 시 폭발 확인
4. 범위 데미지 확인

## 💥 폭발 시스템

### 폭발 범위
- 반경 3m 내 모든 적 탐지
- 거리에 따른 데미지 감소:
  ```
  최종 데미지 = 기본 데미지 × 1.5 × (1 - 거리/반경)
  ```

### 폭발 효과
1. 범위 내 적들에게 데미지
2. Rigidbody가 있으면 물리 폭발력 적용
3. 폭발 이펙트 생성 (있으면)
4. 지우개 파편 생성 (구현 예정)

## 🔧 문제 해결

### 문제: 폭발 안 함
- EraserBombProjectile 스크립트 확인
- Collider가 Trigger가 아닌지 확인
- Explosion Layers 설정 확인

### 문제: 데미지 안 들어감
- Enemy에 EnemyHealth 컴포넌트 확인
- Enemy Layer 설정 확인
- Console에서 폭발 로그 확인

### 문제: 포물선이 이상함
- Rigidbody Gravity 체크
- Projectile Speed 조정
- Throw Direction 확인

## 📝 다음 단계

### 파편 시스템 추가
1. EraserDebris 프리팹 생성
2. EraserBombProjectile.cs에서 SpawnDebris() 호출
3. 파편을 주워서 고무줄 슬링 탄약 충전

### 폭발 이펙트
1. 파티클 시스템 추가
2. EraserBomb_Data의 Hit Effect Prefab에 할당
3. 폭발 사운드 추가
