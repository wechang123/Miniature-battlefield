using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 이 슬롯에 드론 오브젝트를 끌어다 놓으세요.
    public Transform target;

    // 카메라가 얼마나 부드럽게 따라갈지 정합니다. (숫자가 높을수록 빠름)
    public float smoothSpeed = 5f;

    // 카메라가 드론과 유지할 초기 거리와 방향
    private Vector3 offset;

    void Start()
    {
        // 게임이 시작될 때, 카메라와 타겟(드론)의 초기 거리를 계산해서 저장합니다.
        // (중요: 스크립트를 실행하기 전에 씬 뷰에서 카메라 위치를 미리 잡아놔야 합니다)
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    // LateUpdate는 Update()가 끝난 후 호출됩니다.
    // 캐릭터(드론)가 먼저 움직이고 카메라가 따라가야 하므로 LateUpdate가 더 좋습니다.
    void LateUpdate()
    {
        if (target == null)
        {
            return; // 드론이 없으면 아무것도 안 함
        }

        // 1. 카메라가 있어야 할 목표 위치 계산
        // (드론의 현재 위치 + 우리가 처음 정한 거리)
        Vector3 desiredPosition = target.position + offset;

        // 2. 현재 위치에서 목표 위치까지 부드럽게 이동
        // Vector3.Lerp(현재위치, 목표위치, 속도)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 3. 카메라가 항상 드론을 바라보도록 회전
        transform.LookAt(target);
    }
}