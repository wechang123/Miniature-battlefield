using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("Component References")]
    public Rigidbody rb;
    public Transform droneModel; // 시각적 기울기를 적용할 자식 모델

    [Header("Movement Settings")]
    public float moveSpeed = 10f;  // 앞/뒤 이동 속도
    public float strafeSpeed = 8f; // 좌/우 이동 속도
    public float hoverSpeed = 5f;  // 위/아래 이동 속도

    [Header("Rotation Settings")]
    public float yawSpeed = 4f;    // Y축 회전(Yaw) 속도

    [Header("Tilting Settings")]
    public float maxTiltAngle = 20.0f; // 최대 기울기 각도
    public float tiltSpeed = 7.0f;     // 기울어지는 속도

    // 입력값 저장 변수
    private float horizontalInput, verticalInput, hoverInput, yawInput;

    void Start()
    {
        // Rigidbody를 자동으로 찾아 할당 (없으면 경고)
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody component is missing!");
            }
        }
    }

    void Update()
    {
        // --- 1. 입력 받기 (Update에서) ---
        // 키보드 입력은 매 프레임 감지하는 것이 좋습니다.
        
        // 좌/우 (A, D)
        horizontalInput = Input.GetAxis("Horizontal"); 
        // 앞/뒤 (W, S)
        verticalInput = Input.GetAxis("Vertical");
        
        // 좌/우 회전 (Q, E)
        yawInput = 0f;
        if (Input.GetKey(KeyCode.E)) yawInput = 1f;
        if (Input.GetKey(KeyCode.Q)) yawInput = -1f;

        // 위/아래 (Space, Left Shift)
        hoverInput = 0f;
        if (Input.GetKey(KeyCode.Space)) hoverInput = 1f;
        if (Input.GetKey(KeyCode.LeftShift)) hoverInput = -1f;

        // --- 2. 시각적 모델 기울이기 (Update에서) ---
        // 물리(Rigidbody)가 아닌 자식 모델(droneModel)을 회전시킵니다.
        TiltModel();
    }

    void FixedUpdate()
    {
        // --- 3. 물리 이동/회전 적용 (FixedUpdate에서) ---
        // Rigidbody에 힘(Force)과 토크(Torque)를 가합니다.
        
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        // 위/아래 이동 (World Y축 기준)
        rb.AddForce(Vector3.up * hoverInput * hoverSpeed);

        // 앞/뒤 이동 (드론 기준 Z축)
        rb.AddRelativeForce(Vector3.forward * verticalInput * moveSpeed);

        // 좌/우 이동 (드론 기준 X축)
        rb.AddRelativeForce(Vector3.right * horizontalInput * strafeSpeed);
    }

    void HandleRotation()
    {
        // Y축 회전 (Yaw)
        rb.AddTorque(transform.up * yawInput * yawSpeed);
    }

    void TiltModel()
    {
        if (droneModel == null) return;

        // --- 핵심 기울기 로직 ---

        // 1. 앞/뒤 기울기 (Pitch) - X축 기준
        // Vertical 입력(W/S)에 따라 X축 회전값을 계산합니다.
        // (W = 1 -> X축 양수 회전 = 드론 앞부분이 아래로 숙여짐)
        float targetPitch = verticalInput * maxTiltAngle;

        // 2. 좌/우 기울기 (Roll) - Z축 기준
        // Horizontal 입력(A/D)에 따라 Z축 회전값을 계산합니다.
        // (D = 1 -> Z축 음수 회전 = 드론 오른쪽이 아래로 기울어짐)
        float targetRoll = -horizontalInput * maxTiltAngle;

        // 3. 목표 회전값(Quaternion) 생성
        // Y축(Yaw)은 0으로 둡니다. (Y축 회전은 부모 Rigidbody가 담당)
        Quaternion targetRotation = Quaternion.Euler(targetPitch, 0, targetRoll);

        // 4. 부드럽게 회전 적용 (Slerp)
        // droneModel의 '로컬' 회전(localRotation)을 변경합니다.
        droneModel.localRotation = Quaternion.Slerp(
            droneModel.localRotation,
            targetRotation,
            Time.deltaTime * tiltSpeed
        );
    }
}