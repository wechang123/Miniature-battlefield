연필창 프리팹 설정 가이드
====================

1. Hierarchy에서 빈 GameObject 생성
   이름: PencilSpear_Projectile

2. PencilSpear.fbx를 자식으로 추가
   위치: Assets/_Project/Models/Weapons/PencilSpear/PencilSpear.fbx

3. 자식 모델 Transform 설정:
   - Position: (0, 0, 0)
   - Rotation: (90, 0, 0)  ← 중요! 앞으로 날아가도록
   - Scale: (0.3, 0.3, 0.3) 또는 적절한 크기

4. PencilSpear_Projectile에 컴포넌트 추가:

   a) Rigidbody
      - Use Gravity: 체크
      - Mass: 0.1
      - Collision Detection: Continuous Dynamic

   b) Capsule Collider
      - Radius: 0.1
      - Height: 1
      - Direction: Y-Axis
      - Is Trigger: 체크 해제

   c) PencilSpearProjectile 스크립트
      - Max Penetrations: 3
      - Penetration Damage Multiplier: 0.8
      - Rotate While Flying: 체크
      - Rotation Speed: 720

5. Prefab으로 저장
   - PencilSpear_Projectile을 Assets/_Project/Prefabs/Projectiles/로 드래그
   - Hierarchy에서 원본 삭제

컴파일 에러 해결 방법:
- Unity 에디터에서 Assets → Refresh (Cmd + R)
- Console 창에서 에러 확인 (Cmd + Shift + C)
