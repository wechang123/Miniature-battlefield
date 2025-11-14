# 지우개폭탄 폭발 + 파편 시스템 설정 가이드

## 완성된 기능

✅ **지우개폭탄 던지면 폭발**
✅ **폭발 시 파편 생성**
✅ **파편 주우면 인벤토리에 저장** (고무줄슬링 탄약용)
✅ **큰 무기는 1개만 소지** (교체 시 현재 무기 버리기)

---

## Unity에서 설정해야 할 것

### 1단계: 지우개 파편 프리팹 생성

#### 1. 큐브 GameObject 생성
1. **Hierarchy** → **3D Object → Cube**
2. 이름: `EraserFragment_Item`
3. **Transform 설정:**
   - Position: (0, 0, 0)
   - Rotation: (0, 0, 0)
   - **Scale**: **(0.2, 0.2, 0.2)** ← 작은 큐브

#### 2. 머티리얼 설정 (선택사항)
1. **Project 창** → **Create → Material**
2. 이름: `EraserFragmentMaterial`
3. **Base Color**: 연한 분홍색 또는 흰색
4. 큐브의 **MeshRenderer**에 드래그 앤 드롭

#### 3. Rigidbody 추가
1. `EraserFragment_Item` 선택
2. **Add Component** → **Rigidbody**
3. 설정:
   - **Mass**: 0.1 (가볍게)
   - **Use Gravity**: ✅
   - **Is Kinematic**: ❌

#### 4. Box Collider 설정
- 이미 Cube에 포함됨
- **Is Trigger**: ❌ (물리 충돌 필요)

#### 5. Sphere Collider 추가 (픽업용 Trigger)
1. **Add Component** → **Sphere Collider**
2. 설정:
   - **Is Trigger**: ✅
   - **Radius**: 0.5 (픽업 범위)

#### 6. EraserFragmentItem 스크립트 추가
1. **Add Component** → **EraserFragmentItem**
2. 설정:
   - **Item Name**: "지우개 파편"
   - **Item Type**: Consumable
   - **Fragment Value**: 1
   - **Is Pickable**: ✅
   - **Pickup Range**: 2.0
   - **Bob Speed**: 2.0
   - **Bob Height**: 0.3
   - **Rotation Speed**: 50

#### 7. Layer 설정
- **Layer**: Item (또는 Default)

#### 8. 프리팹으로 저장
1. `EraserFragment_Item`을 **Project 창**의 `Assets/_Project/Prefabs/Items` 폴더로 드래그
2. **Hierarchy**에서 `EraserFragment_Item` 삭제

---

### 2단계: EraserBomb_Projectile 프리팹 설정

1. **Project 창**에서 `EraserBomb_Projectile` 프리팹 열기
2. **Inspector**에서 **Eraser Bomb Projectile (Script)** 찾기
3. **Fragment Settings** 섹션:
   - **Fragment Prefab**: `EraserFragment_Item` 프리팹 드래그 앤 드롭
   - **Fragment Count**: 8 (파편 개수)
   - **Fragment Force**: 5 (튀는 힘)
   - **Fragment Spread**: 1 (퍼짐 정도)

---

### 3단계: ItemThrowSystem 설정

1. **Hierarchy**에서 **Player** GameObject 선택
2. **Inspector**에서 **Item Throw System (Script)** 찾기
3. **Weapon Projectiles** 섹션:
   - **Eraser Bomb Projectile Prefab**: `EraserBomb_Projectile` 프리팹 드래그 앤 드롭

---

### 4단계: 테스트

#### 테스트 시나리오:

1. **Play 버튼** 클릭

2. **지우개폭탄 주우기:**
   - E 키로 지우개폭탄 주움
   - 플레이어가 들고 다님

3. **지우개폭탄 던지기:**
   - Throw 키 누르기 (기본: T 또는 마우스 우클릭)
   - 지우개폭탄이 날아감

4. **폭발 확인:**
   - ✅ 선생님이나 바닥에 충돌 시 **폭발!** 💥
   - ✅ 폭발 이펙트 표시 (설정했다면)
   - ✅ 폭발 사운드 재생 (설정했다면)
   - ✅ **파편 8개가 사방으로 튐!**

5. **파편 주우기:**
   - 파편 근처에 가서 **E 키**
   - Console에 "[EraserFragmentItem] 지우개 파편 1개 획득!" 메시지

6. **무기 교체 테스트:**
   - 지우개폭탄을 들고 있는 상태
   - 다른 지우개폭탄 아이템에 가서 **E 키**
   - ✅ 현재 지우개폭탄이 바닥에 떨어짐
   - ✅ 새 지우개폭탄을 들음

---

### 5단계: 선생님 데미지 테스트

#### 선생님 GameObject 설정 (임시):

1. **Hierarchy**에서 선생님 GameObject 선택
2. **Add Component** → **Box Collider** (아직 없다면)
3. 선생님이 폭발 범위에 있으면 데미지 받음 (IDamageable 인터페이스 필요)

#### 확인 사항:
- 지우개폭탄이 선생님 근처에서 폭발 시
- Console에 데미지 로그 출력 (IDamageable 구현되었다면)

---

## 설정 값 조정 가이드

### EraserBombProjectile 설정:

**폭발력 조정:**
- **Explosion Radius**: 3 → 크게 하면 넓은 범위 폭발
- **Explosion Force**: 500 → 크게 하면 적이 더 멀리 날아감
- **Explosion Damage Multiplier**: 1.5 → 폭발 데미지 배율

**파편 조정:**
- **Fragment Count**: 8 → 파편 개수 (5~15 권장)
- **Fragment Force**: 5 → 파편이 튀는 힘 (3~10)
- **Fragment Spread**: 1 → 파편 퍼짐 (0.5~2)

### ItemThrowSystem 설정:

**던지기 힘 조정:**
- **Throw Force**: 15 → 크게 하면 더 멀리 날아감 (10~25)
- **Throw Angle**: 30 → 던지는 각도 (20~45)

---

## 문제 해결

### 폭발이 안 됨
**원인:** Explode On Impact가 체크 안 됨
**해결:** EraserBomb_Projectile의 **Explode On Impact** ✅ 체크

### 파편이 생성 안 됨
**원인:** Fragment Prefab 할당 안 됨
**해결:** EraserBomb_Projectile의 **Fragment Prefab**에 `EraserFragment_Item` 할당

### 던질 때 일반 아이템처럼 굴러감
**원인:** Eraser Bomb Projectile Prefab 할당 안 됨
**해결:** ItemThrowSystem의 **Eraser Bomb Projectile Prefab**에 `EraserBomb_Projectile` 할당

### 파편을 못 주움
**원인:**
- EraserFragmentItem에 Sphere Collider (Trigger) 없음
- Is Pickable 체크 안 됨
**해결:**
- Sphere Collider 추가하고 Is Trigger ✅
- Is Pickable ✅ 체크

### 무기 교체가 안 됨
**원인:** 아이템에 WeaponPartItem 스크립트가 없음
**해결:** 지우개폭탄 Item 프리팹에 **WeaponPartItem** 또는 **EraserBombItem** 스크립트 있는지 확인

---

## 다음 단계

✅ 지우개폭탄 폭발 시스템 완료!
⬜ 고무줄슬링 무기 구현
⬜ 고무줄슬링이 지우개 파편을 탄약으로 사용
⬜ 연필창 근접 무기 구현

---

## 요약

**게임플레이 플로우:**
```
1. 교실에 지우개폭탄 스폰
    ↓
2. E 키로 주움 (들고 다님)
    ↓
3. Throw 키로 던짐
    ↓
4. 선생님/바닥에 맞으면 💥 폭발!
    ↓
5. 파편 8개 사방으로 튐
    ↓
6. E 키로 파편 주움 → 인벤토리 저장
    ↓
7. (나중에) 고무줄슬링으로 파편 발사!
```

**무기 시스템:**
- 큰 무기(지우개폭탄, 고무줄슬링, 연필창): **1개만 소지**
- 다른 무기 주울 때: 현재 무기 **자동으로 버림**
- 파편(탄약): **인벤토리에 저장**

설정 완료 후 테스트해보세요! 🎯💥
