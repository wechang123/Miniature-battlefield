# YajaGame 프로젝트 아키텍처

## 프로젝트 개요
- **프로젝트명**: Miniature-battlefield (야자게임)
- **엔진**: Unity (C#)
- **장르**: 학교 배경 잠입/공포 게임

## 폴더 구조
```
Assets/
├── _Project/Scripts/Core/     # 핵심 게임 시스템
│   ├── AI/                    # AI 관련 (손전등 등)
│   ├── Gameplay/              # 게임플레이 시스템
│   │   ├── Combat/            # 전투 시스템
│   │   ├── Weapons/           # 무기 시스템
│   │   ├── Projectiles/       # 투사체
│   │   └── Items/             # 아이템
│   ├── Effects/               # 이펙트 시스템
│   └── UI/                    # UI 시스템
├── StarterAssets/             # Unity Starter Assets (3인칭 컨트롤러)
└── 루트 스크립트               # AI 컨트롤러, GameManager 등
```

## 핵심 시스템

### 1. AI 시스템
- **AIController.cs**: 기본 AI - 랜덤 순찰, 시야 감지, 손전등 장착 가능
- **SimpleAIController.cs**: 선생님 AI - 순찰/추격/공격 상태, NavMesh 기반
  - 추격 시 속도 증가 (chaseSpeed)
  - 플레이어 잡으면 공격 애니메이션 후 GameOver
- **DroneAIController.cs**: 드론 AI - 공중 비행, 경계 영역 설정, 장애물 회피

### 2. 전투 시스템 (YajaGame.Gameplay.Combat 네임스페이스)
- **IDamageable**: 데미지 인터페이스
  - TakeDamage(DamageInfo), CurrentHealth, MaxHealth, IsAlive
- **DamageInfo**: 데미지 정보 (Amount, DamageType, Source, Direction, KnockbackForce 등)
- **PlayerHealth**: 플레이어 체력 (무적 시간, 회복, 사망 → GameManager 호출)
- **EnemyHealth**: 적 체력 (히트 이펙트, 넉백, NavMesh 처리, 사망 효과)

### 3. 인벤토리 시스템 (YajaGame.Gameplay 네임스페이스)
- **InventoryManager**: 싱글톤 매니저
  - 무기 부품 관리 (PencilSpear, EraserBomb, RubberBandSling)
  - 던지기 통계 추적
  - 업그레이드 시스템 (partsRequiredForUpgrade)

### 4. 무기 시스템 (YajaGame.Gameplay.Weapons 네임스페이스)
- **WeaponData**: ScriptableObject 기반 무기 데이터
  - 기본 정보, 전투 스탯, 투사체 설정, 발사 설정
- **WeaponPartType**: PencilSpear, EraserBomb, RubberBandSling

### 5. 아이템 시스템
- **ItemBase**: 추상 기본 클래스 (IPickupable 구현)
  - 아이템 타입: WeaponPart, Consumable, Currency
  - 애니메이션 (떠다님, 회전)
  - 커스텀 Carry 설정

### 6. GameManager (싱글톤)
- 게임오버 처리
- 플레이어/카메라 관리
- 씬 재시작

## 주요 기능
1. 3인칭 캐릭터 컨트롤 (StarterAssets)
2. AI 적 (선생님, 드론) - 순찰/추격
3. 무기 던지기 시스템
4. 아이템 수집 및 인벤토리
5. 체력/데미지 시스템
6. 넉백 시스템

## 네임스페이스
- `YajaGame.Gameplay` - 게임플레이 관련
- `YajaGame.Gameplay.Combat` - 전투 시스템
- `YajaGame.Gameplay.Weapons` - 무기 시스템

## 레이어/태그
- Player 태그 사용
- playerLayer, targetLayer 마스크 사용
