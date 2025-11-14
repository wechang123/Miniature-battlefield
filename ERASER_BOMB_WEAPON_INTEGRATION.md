# 지우개폭탄 무기 시스템 통합 가이드

## 개요

이 가이드는 **지우개폭탄을 주워서 인벤토리에 저장하고, 마우스 클릭으로 발사하는 전체 시스템**을 Unity에서 설정하는 방법을 설명합니다.

**전체 플로우:**
1. 교실에 지우개폭탄 아이템 랜덤 스폰
2. 플레이어가 돌아다니며 지우개폭탄 주움
3. InventoryManager에 개수 저장 (탄약으로 사용)
4. 마우스 클릭으로 지우개폭탄 발사
5. EraserBomb_Projectile 생성 → 폭발

---

## 1단계: InventoryManager 설정

### 1. InventoryManager GameObject 생성

1. **Hierarchy 창**에서 우클릭 → **Create Empty**
2. 이름: **`InventoryManager`**
3. **Add Component** → **InventoryManager** 스크립트 추가

### 2. InventoryManager 설정

**Inspector 창에서:**
- **Weapon Parts Inventory**:
  - Pencil Spear Parts: 0
  - **Eraser Bomb Parts: 0** (시작 시 0개, 주우면 증가)
  - Rubber Band Sling Parts: 0

- **Upgrade Requirements**:
  - Parts Required For Upgrade: 5 (5개 모으면 업그레이드 가능)

**IMPORTANT**: InventoryManager는 싱글톤이므로 씬에 1개만 있어야 합니다!

---

## 2단계: EraserBomb_Item 프리팹 생성

> 이미 `ERASER_BOMB_ITEM_SETUP.md` 가이드를 따라 했다면 이 단계는 건너뜁니다.

**프리팹 위치:** `Assets/_Project/Prefabs/Items/EraserBomb_Item.prefab`

**구성:**
- EraserBomb 3D 모델 (자식)
- Sphere Collider (Is Trigger ✅)
- EraserBombItem 스크립트
  - Bomb Count: 1 (주우면 얻는 개수)
  - Weapon Part Type: EraserBomb
  - Is Pickable: ✅

---

## 3단계: ItemSpawner 설정 (랜덤 스폰)

> 이미 `ERASER_BOMB_ITEM_SETUP.md` 가이드를 따라 했다면 이 단계는 건너뜁니다.

### EraserBombSpawner GameObject 생성

1. **Hierarchy 창** → **Create Empty**
2. 이름: **`EraserBombSpawner`**
3. **Add Component** → **ItemSpawner**

### Spawner 설정

**Spawn Settings:**
- **Item Prefab**: `EraserBomb_Item` 프리팹 드래그 앤 드롭
- **Initial Spawn Count**: 5
- **Spawn Interval**: 10.0
- **Max Items In Scene**: 10

**Spawn Area:**
- **Spawn Area Center**: (0, 0, 0) - 교실 중심 좌표
- **Spawn Area Size**: (20, 0, 20) - 교실 크기에 맞게 조정
- **Spawn Height**: 1.0
- **Ground Layer**: Ground 레이어 선택

---

## 4단계: 플레이어에 무기 시스템 설정

### 1. Player GameObject 찾기

**Hierarchy 창**에서 플레이어 GameObject 선택 (보통 `PlayerArmature` 또는 `Player`)

### 2. Fire Point 생성

발사체가 나가는 위치를 지정합니다.

1. Player GameObject의 **자식으로 빈 GameObject 생성**
2. 이름: **`FirePoint`**
3. **Transform 위치 조정**:
   - Position: (0, 1.5, 0.5) - 플레이어 가슴 앞쪽
   - Rotation: (0, 0, 0)
   - 또는 Scene 뷰에서 위치 조정 (플레이어 앞쪽, 눈높이 근처)

### 3. InventoryWeaponController 추가

1. Player GameObject 선택
2. **Add Component** → **InventoryWeaponController**

**Inspector 설정:**

**Weapon Settings:**
- **Weapon Data**: `EraserBomb_Data` ScriptableObject 드래그 앤 드롭
  - 위치: `Assets/_Project/EraserBomb_Data.asset`
  - (없으면 아래 "5단계: WeaponData 생성" 참고)
- **Weapon Part Type**: **EraserBomb**
- **Fire Point**: 위에서 만든 `FirePoint` GameObject 드래그 앤 드롭

**Events:** (선택사항, UI 연동 시 사용)
- On Weapon Fired: 발사 시 호출되는 이벤트
- On Ammo Changed: 탄약 변경 시 호출되는 이벤트

### 4. PlayerWeaponInput 추가

1. Player GameObject 선택
2. **Add Component** → **PlayerWeaponInput**

**Inspector 설정:**

**References:**
- **Weapon Controller**: 자동으로 `InventoryWeaponController` 찾음 (비어있어도 OK)
- **Input**: 자동으로 `StarterAssetsInputs` 찾음 (비어있어도 OK)

**Input Settings:**
- **Use Mouse Input**: ✅ (마우스 왼쪽 클릭으로 발사)
- **Use Input Action**: ✅ (StarterAssets shoot 입력도 사용)

---

## 5단계: WeaponData 생성 (이미 있으면 건너뛰기)

### EraserBomb_Data.asset 확인

**이미 존재:** `Assets/_Project/EraserBomb_Data.asset`

만약 없다면:

1. **Project 창** → `Assets/_Project` 폴더
2. 우클릭 → **Create → YajaGame → Weapons → Weapon Data**
3. 이름: **`EraserBomb_Data`**

### WeaponData 설정

**Basic Info:**
- **Weapon Name**: "지우개폭탄"
- **Weapon Type**: **EraserBomb**
- **Description**: "던지면 폭발하는 지우개 폭탄"

**Combat Stats:**
- **Damage**: 30.0 (기본 데미지, EraserBombProjectile이 explosion multiplier 적용)
- **Damage Type**: Physical
- **Knockback Force**: 10.0

**Projectile Settings:**
- **Projectile Prefab**: `EraserBomb_Projectile` 프리팹 드래그
  - 위치: `Assets/_Project/Prefabs/Projectiles/EraserBomb_Projectile.prefab`
- **Projectile Speed**: 15.0 (던지는 속도)
- **Projectile Lifetime**: 5.0 (공중에 있는 최대 시간)
- **Projectile Gravity Scale**: 1.0

**Fire Settings:**
- **Fire Rate**: 2.0 (초당 2발, 즉 0.5초마다 발사 가능)
- **Max Ammo**: -1 (무한, InventoryManager에서 관리하므로 -1로 설정)
- **Reload Time**: 0 (사용 안 함)

**Audio:** (선택사항)
- **Fire Sound**: 던지는 사운드 클립
- **Empty Sound**: 탄약 없을 때 사운드 클립

**Visual Effects:** (선택사항)
- **Muzzle Flash Prefab**: 발사 시 이펙트
- **Hit Effect Prefab**: 맞았을 때 이펙트 (폭발은 EraserBombProjectile에서 처리)

---

## 6단계: EraserBomb_Projectile 프리팹 확인

**프리팹 위치:** `Assets/_Project/Prefabs/Projectiles/EraserBomb_Projectile.prefab`

**필수 컴포넌트:**
1. **Rigidbody**
   - Use Gravity: ✅
   - Mass: 0.5

2. **Sphere Collider** (또는 다른 Collider)
   - Is Trigger: ❌ (트리거 아님)

3. **EraserBombProjectile** 스크립트
   - **Projectile Settings:**
     - Life Time: 5
     - Destroy On Hit: ✅
   - **Hit Settings:**
     - Hit Layers: Everything (또는 적 레이어)
   - **Eraser Bomb Settings:**
     - **Explosion Radius**: 3
     - **Explosion Damage Multiplier**: 1.5
     - **Explosion Force**: 500
     - **Explosion Effect Prefab**: (선택사항) 폭발 이펙트
     - **Explosion Sound**: (선택사항) 폭발 사운드
     - **Explosion Layers**: Everything
     - **Show Explosion Range**: ✅ (디버그용)
   - **Fuse Settings:**
     - **Fuse Time**: 0 (즉시 폭발)
     - **Explode On Impact**: ✅ (충돌 시 폭발)

---

## 7단계: 플레이어 태그 확인

1. **Hierarchy 창**에서 플레이어 GameObject 선택
2. **Inspector 창** 상단의 **Tag** 확인
3. **Player** 태그가 설정되어 있어야 함
4. 없으면 **Tag → Player** 선택

---

## 8단계: 테스트

### 테스트 순서

1. **Play 버튼** 클릭

2. **아이템 스폰 확인:**
   - 교실에 지우개폭탄 5개가 랜덤 위치에 스폰됨
   - 아이템이 둥둥 떠다니며 회전함

3. **아이템 획득 테스트:**
   - 플레이어로 아이템에 가까이 가기
   - Console에 `[EraserBombItem] 지우개폭탄 1개 획득!` 메시지
   - Console에 `[InventoryManager] EraserBomb 부품 추가: 1개` 메시지
   - 아이템 사라짐

4. **인벤토리 확인:**
   - **Hierarchy**에서 `InventoryManager` 선택
   - **Inspector**에서 **Eraser Bomb Parts** 값이 1로 증가했는지 확인

5. **발사 테스트:**
   - **마우스 왼쪽 클릭**
   - Console에 `[InventoryWeaponController] 발사!` 메시지
   - 지우개폭탄 발사체가 날아감
   - 충돌 시 폭발 (빨간 구체로 표시)
   - InventoryManager의 Eraser Bomb Parts가 0으로 감소

6. **탄약 소진 테스트:**
   - 탄약이 0일 때 마우스 클릭
   - Console에 `[InventoryWeaponController] 탄약이 없습니다!` 메시지
   - Empty Sound 재생 (설정했다면)

7. **재스폰 테스트:**
   - 10초 기다리기
   - 새로운 지우개폭탄 스폰됨
   - 주워서 다시 발사 가능

### 확인 사항

✅ 아이템 스폰됨
✅ 아이템 주울 수 있음
✅ 인벤토리에 개수 저장됨
✅ 마우스 클릭으로 발사됨
✅ 발사 시 인벤토리 개수 감소
✅ 탄약 없으면 발사 안 됨
✅ 폭발 효과 작동함

---

## 9단계: UI 연동 (선택사항)

### 탄약 UI 표시

InventoryWeaponController의 **OnAmmoChanged** 이벤트를 사용하여 UI에 탄약 개수 표시:

```csharp
// UI 스크립트 예시
public class AmmoUI : MonoBehaviour
{
    [SerializeField] private Text ammoText;
    [SerializeField] private InventoryWeaponController weaponController;

    private void Start()
    {
        weaponController.OnAmmoChanged.AddListener(UpdateAmmoDisplay);
    }

    private void UpdateAmmoDisplay(int currentAmmo)
    {
        ammoText.text = $"지우개폭탄: {currentAmmo}";
    }
}
```

또는 Unity Event로 연결:
1. InventoryWeaponController의 **On Ammo Changed** 이벤트에 **+** 버튼 클릭
2. UI Text GameObject 드래그
3. **Text → text** 함수 선택
4. Dynamic int 파라미터 사용

---

## 문제 해결

### 아이템을 주워도 인벤토리에 추가되지 않음

**원인:** InventoryManager 인스턴스가 씬에 없음

**해결:**
1. Hierarchy에 `InventoryManager` GameObject가 있는지 확인
2. InventoryManager 스크립트가 추가되어 있는지 확인
3. Play 모드에서 Console 확인: `[InventoryManager] 인벤토리 초기화` 메시지 있어야 함

### 발사가 안 됨

**원인 1:** WeaponData가 설정되지 않음
- InventoryWeaponController의 **Weapon Data** 필드에 `EraserBomb_Data` 할당

**원인 2:** Fire Point가 없음
- InventoryWeaponController의 **Fire Point** 필드에 FirePoint GameObject 할당

**원인 3:** Projectile Prefab이 설정되지 않음
- WeaponData의 **Projectile Prefab**에 `EraserBomb_Projectile` 할당

### 발사체가 날아가지 않음

**원인:** EraserBombProjectile의 Initialize() 호출 안 됨

**해결:**
1. `EraserBomb_Projectile` 프리팹 열기
2. EraserBombProjectile 스크립트 확인
3. Rigidbody가 있는지 확인
4. Projectile Speed 값 확인 (WeaponData에서)

### 폭발이 안 됨

**원인:** Explode On Impact가 체크되지 않음

**해결:**
1. `EraserBomb_Projectile` 프리팹 열기
2. EraserBombProjectile 스크립트에서 **Explode On Impact** ✅ 체크

### 탄약이 무한으로 나감

**원인:** InventoryManager에서 탄약이 제거되지 않음

**해결:**
1. InventoryWeaponController.Fire() 메서드에서 `RemoveWeaponPart()` 호출 확인
2. Console에서 `[InventoryManager] EraserBomb 부품 제거` 메시지 확인

---

## 다음 단계

지우개폭탄 시스템이 완료되었으므로:

1. ✅ **고무줄 슬링** 3D 모델 생성 (meshy.ai)
2. ✅ **지우개 파편** 시스템 구현 (폭발 시 생성)
3. ✅ **고무줄 슬링 무기** 구현 (파편을 탄약으로 사용)
4. ✅ **연필창** 근접 무기 프리팹 생성 및 플레이어 연동

---

## 요약

**시스템 구성:**
1. **EraserBombItem** (ItemBase) → 주울 수 있는 아이템
2. **ItemSpawner** → 교실에 랜덤 스폰
3. **InventoryManager** (싱글톤) → 지우개폭탄 개수 저장
4. **InventoryWeaponController** → 인벤토리 개수를 탄약으로 사용
5. **PlayerWeaponInput** → 마우스 클릭 입력 처리
6. **EraserBombProjectile** (WeaponProjectileBase) → 발사체, 폭발 구현

**데이터 플로우:**
```
ItemSpawner → EraserBomb_Item 스폰
     ↓
Player 근처 → OnPickup() 호출
     ↓
InventoryManager.AddWeaponPart(EraserBomb, 1)
     ↓
플레이어가 마우스 클릭
     ↓
PlayerWeaponInput → InventoryWeaponController.TryFire()
     ↓
InventoryManager.RemoveWeaponPart(EraserBomb, 1)
     ↓
EraserBomb_Projectile 생성 → Initialize()
     ↓
충돌 감지 → Explode()
     ↓
폭발 데미지 & 넉백 & 이펙트
```
