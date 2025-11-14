# 지우개폭탄 시스템 빠른 설정 체크리스트

Unity에서 해야 할 작업 목록입니다. 각 단계를 완료하면 ✅ 체크하세요.

---

## 필수 GameObject 생성

### □ InventoryManager
- [ ] Hierarchy → Create Empty → 이름: `InventoryManager`
- [ ] Add Component → **InventoryManager**
- [ ] Eraser Bomb Parts: 0으로 설정

### □ EraserBombSpawner
- [ ] Hierarchy → Create Empty → 이름: `EraserBombSpawner`
- [ ] Add Component → **ItemSpawner**
- [ ] Item Prefab: `EraserBomb_Item` 프리팹 할당
- [ ] Spawn Area Center/Size 교실에 맞게 조정
- [ ] Ground Layer 설정

---

## 프리팹 생성

### □ EraserBomb_Item 프리팹
- [ ] Hierarchy → Create Empty → 이름: `EraserBomb_Item`
- [ ] 자식으로 EraserBomb.fbx 3D 모델 추가
- [ ] Add Component → **Sphere Collider** (Is Trigger ✅)
- [ ] Add Component → **EraserBombItem**
- [ ] Layer: Item으로 설정
- [ ] Prefabs/Items 폴더에 프리팹으로 저장

### □ EraserBomb_Projectile 프리팹 (이미 있음)
- [ ] 위치 확인: `Assets/_Project/Prefabs/Projectiles/EraserBomb_Projectile.prefab`
- [ ] Rigidbody 있는지 확인
- [ ] EraserBombProjectile 스크립트 있는지 확인
- [ ] Explode On Impact ✅ 확인

---

## 플레이어 설정

### □ Player GameObject 찾기
- [ ] Hierarchy에서 플레이어 GameObject 선택
- [ ] Tag가 **Player**인지 확인

### □ FirePoint 생성
- [ ] Player의 자식으로 Create Empty → 이름: `FirePoint`
- [ ] Position: (0, 1.5, 0.5) 또는 플레이어 가슴 앞쪽

### □ InventoryWeaponController 추가
- [ ] Player에 Add Component → **InventoryWeaponController**
- [ ] **Weapon Data**: `EraserBomb_Data.asset` 할당
- [ ] **Weapon Part Type**: EraserBomb 선택
- [ ] **Fire Point**: 위에서 만든 FirePoint 할당

### □ PlayerWeaponInput 추가
- [ ] Player에 Add Component → **PlayerWeaponInput**
- [ ] **Use Mouse Input**: ✅
- [ ] **Use Input Action**: ✅

---

## WeaponData 설정

### □ EraserBomb_Data.asset
- [ ] 위치: `Assets/_Project/EraserBomb_Data.asset`
- [ ] **Weapon Name**: "지우개폭탄"
- [ ] **Weapon Type**: EraserBomb
- [ ] **Damage**: 30
- [ ] **Projectile Prefab**: `EraserBomb_Projectile` 할당
- [ ] **Projectile Speed**: 15
- [ ] **Fire Rate**: 2
- [ ] **Max Ammo**: -1 (무한, InventoryManager에서 관리)

---

## 테스트

### □ 게임 실행 테스트
- [ ] Play 버튼 클릭
- [ ] 교실에 지우개폭탄 5개 스폰됨
- [ ] 아이템 둥둥 떠다님
- [ ] 플레이어로 아이템 주울 수 있음
- [ ] Console: "[EraserBombItem] 지우개폭탄 1개 획득!" 메시지
- [ ] InventoryManager의 Eraser Bomb Parts 증가 확인
- [ ] 마우스 왼쪽 클릭으로 발사됨
- [ ] 발사체 날아감
- [ ] 충돌 시 폭발 (빨간 구체)
- [ ] InventoryManager의 Eraser Bomb Parts 감소
- [ ] 탄약 0일 때 발사 안 됨

---

## 문제 발생 시

### 인벤토리에 추가 안 됨
→ InventoryManager GameObject가 씬에 있는지 확인

### 발사 안 됨
→ InventoryWeaponController의 Weapon Data, Fire Point 확인

### 발사체 날아가지 않음
→ WeaponData의 Projectile Prefab, Projectile Speed 확인

### 폭발 안 됨
→ EraserBombProjectile의 Explode On Impact ✅ 확인

---

## 상세 가이드

더 자세한 설명은 다음 파일들을 참고하세요:

1. **ERASER_BOMB_WEAPON_INTEGRATION.md** - 전체 시스템 통합 가이드 (가장 중요!)
2. **ERASER_BOMB_ITEM_SETUP.md** - 아이템 프리팹 생성 가이드

---

완료하면 지우개폭탄을 주워서 던질 수 있습니다! 🎉
