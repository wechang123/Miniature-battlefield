using UnityEngine;

public class DroneProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 10f;
    public float speed = 15f;
    public float lifetime = 5f;
    
    [Header("Effects")]
    public GameObject hitEffect;
    public Light projectileLight;

    void Start()
    {
        Destroy(gameObject, lifetime);
        
        // 자동으로 빨간 빛 추가
        if (projectileLight == null)
        {
            GameObject lightObj = new GameObject("Light");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            
            projectileLight = lightObj.AddComponent<Light>();
            projectileLight.color = Color.red;
            projectileLight.range = 5f;
            projectileLight.intensity = 2f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 드론이나 NPC는 무시
        if (other.CompareTag("Enemy") || other.CompareTag("Drone"))
        {
            return;
        }
        
        // 플레이어에게 맞으면
        if (other.CompareTag("Player"))
        {
            Debug.Log("? 발사체가 플레이어 명중!");
            
            // SendMessage로 플레이어 공격
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            
            // 충돌 효과
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            Destroy(gameObject);
        }
        // 벽이나 바닥에 맞으면
        else
        {
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            Destroy(gameObject);
        }
    }
}