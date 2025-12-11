using UnityEngine;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// Stage2에서 수집해야 하는 키 아이템
    /// 인벤토리 시스템과 분리된 독립적인 수집 아이템
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class KeyItem : MonoBehaviour
    {
        [Header("Key Item Settings")]
        [SerializeField] private GameObject collectEffect;
        [SerializeField] private AudioClip collectSound;
        [SerializeField] private float pickupRange = 3f;
        
        [Header("Visual Effects")]
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.3f;
        [SerializeField] private float rotationSpeed = 30f;
        
        private Vector3 startPosition;
        private Collider itemCollider;
        private bool isCollected = false;
        
        private void Awake()
        {
            itemCollider = GetComponent<Collider>();
            if (itemCollider == null)
            {
                itemCollider = gameObject.AddComponent<BoxCollider>();
            }
            itemCollider.isTrigger = true;
            startPosition = transform.position;
        }
        
        private void Update()
        {
            if (!isCollected)
            {
                AnimateItem();
            }
        }
        
        /// <summary>
        /// 키 애니메이션 (떠다니는 효과)
        /// </summary>
        private void AnimateItem()
        {
            // 상하 움직임
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            
            // 회전
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;
            
            // 플레이어와 충돌 시 수집 처리
            if (other.CompareTag("Player"))
            {
                CollectKey();
            }
        }
        
        /// <summary>
        /// 키 수집 처리
        /// </summary>
        private void CollectKey()
        {
            if (isCollected) return;
            
            isCollected = true;
            
            // Stage2KeyManager에게 키 수집 알림
            Stage2KeyManager keyManager = FindObjectOfType<Stage2KeyManager>();
            if (keyManager != null)
            {
                keyManager.OnKeyCollected();
            }
            else
            {
                Debug.LogWarning("[KeyItem] Stage2KeyManager를 찾을 수 없습니다!");
            }
            
            // 수집 이펙트 재생
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }
            
            // 수집 사운드 재생
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, 0.7f);
            }
            
            Debug.Log("[KeyItem] 키를 수집했습니다!");
            
            // 키 오브젝트 제거
            Destroy(gameObject);
        }
        
        /// <summary>
        /// 키 아이템 초기화 (Stage2KeyManager에서 호출)
        /// </summary>
        public void Initialize()
        {
            // 키 생성 시 초기 설정
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
            
            // 콜라이더 확인
            if (itemCollider == null)
            {
                itemCollider = GetComponent<Collider>();
                if (itemCollider == null)
                {
                    itemCollider = gameObject.AddComponent<BoxCollider>();
                }
                itemCollider.isTrigger = true;
            }
            
            // 시작 위치 저장 (애니메이션용)
            startPosition = transform.position;
        }
    }
}