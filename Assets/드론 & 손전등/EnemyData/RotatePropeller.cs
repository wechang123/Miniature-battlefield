using UnityEngine;

/*public class RotatePropeller : MonoBehaviour

{
    public float rotationSpeed = 360f;
    
    // 로컬 축 기준으로 회전 (프로펠러 자신의 축)
    // 프로펠러 축 피벗으로 조정함
    public Vector3 rotationAxis = Vector3.forward; // 또는 Vector3.up, Vector3.right

    void Update()
    {
        float angleToRotate = rotationSpeed * Time.deltaTime;
        
        // Space.Self를 사용하면 로컬 좌표계 기준으로 회전
        transform.Rotate(rotationAxis, angleToRotate, Space.Self);
    }
}*/




public class RotatePropeller : MonoBehaviour
{
    // "실제"처럼 보이려면 3000~5000 사이의 높은 값 필요
    public float rotationSpeed = 3000f; 
    
    public Vector3 rotationAxis = Vector3.forward; 


    // 물리 시간에 맞춰 회전하도록 FixedUpdate() 사용!
    void FixedUpdate()
    {
        // FixedUpdate 안에서는 Time.deltaTime 대신 Time.fixedDeltaTime을 사용합니다.
        float angleToRotate = rotationSpeed * Time.fixedDeltaTime;
        
        // 로컬 축 기준으로 회전
        transform.Rotate(rotationAxis, angleToRotate, Space.Self);
    }
}