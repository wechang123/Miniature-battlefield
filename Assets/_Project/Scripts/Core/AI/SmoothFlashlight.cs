using UnityEngine;

/// <summary>
/// 손전등 빛을 부드럽게 만들어주는 스크립트
/// 손 흔들림을 줄여줌
/// </summary>
public class SmoothFlashlight : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform followTarget; // 따라갈 대상 (오른손)
    [SerializeField] private Transform bodyTransform; // 몸통 (방향 참조용)

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothSpeed = 8f; // 위치 부드러움
    [SerializeField] private float rotationSmoothSpeed = 5f; // 회전 부드러움
    [SerializeField] private bool useBodyForward = true; // 몸통 방향 사용

    [Header("Offset")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    private Vector3 _smoothedPosition;
    private Quaternion _smoothedRotation;

    private void Start()
    {
        if (followTarget != null)
        {
            _smoothedPosition = followTarget.position;
            _smoothedRotation = followTarget.rotation;
        }

        // bodyTransform 자동 찾기
        if (bodyTransform == null)
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                if (parent.GetComponent<Animator>() != null)
                {
                    bodyTransform = parent;
                    break;
                }
                parent = parent.parent;
            }
        }
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;

        // 위치 부드럽게 따라가기
        Vector3 targetPos = followTarget.position + followTarget.TransformDirection(positionOffset);
        _smoothedPosition = Vector3.Lerp(_smoothedPosition, targetPos, Time.deltaTime * positionSmoothSpeed);
        transform.position = _smoothedPosition;

        // 회전 부드럽게 따라가기
        Quaternion targetRot;
        if (useBodyForward && bodyTransform != null)
        {
            // 몸통의 forward 방향 사용 (더 안정적)
            targetRot = Quaternion.LookRotation(bodyTransform.forward) * Quaternion.Euler(rotationOffset);
        }
        else
        {
            // 손의 회전 따라가기
            targetRot = followTarget.rotation * Quaternion.Euler(rotationOffset);
        }

        _smoothedRotation = Quaternion.Slerp(_smoothedRotation, targetRot, Time.deltaTime * rotationSmoothSpeed);
        transform.rotation = _smoothedRotation;
    }

    /// <summary>
    /// 따라갈 대상 설정
    /// </summary>
    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
        if (target != null)
        {
            _smoothedPosition = target.position;
            _smoothedRotation = target.rotation;
        }
    }
}
