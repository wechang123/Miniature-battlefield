using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 여러 아이템 중 랜덤으로 스폰
    /// </summary>
    public class RandomItemSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnableItem
        {
            public GameObject itemPrefab;
            [Range(0f, 100f)]
            public float spawnWeight = 10f; // 스폰 확률 가중치 (높을수록 자주 나옴)
        }

        [Header("Spawn Items")]
        [SerializeField] private List<SpawnableItem> spawnableItems = new List<SpawnableItem>();

        [Header("Spawn Settings")]
        [SerializeField] private int initialSpawnCount = 5;
        [SerializeField] private float spawnInterval = 10f;
        [SerializeField] private int maxItemsInScene = 10;

        [Header("Spawn Area - Rectangle Corners")]
        [Tooltip("사각형 영역의 네 모서리 좌표")]
        [SerializeField] private Vector3 corner1 = new Vector3(-85, 1, 72);  // 좌하단
        [SerializeField] private Vector3 corner2 = new Vector3(-85, 1, 75);  // 좌상단
        [SerializeField] private Vector3 corner3 = new Vector3(-16, 1, 72);  // 우하단
        [SerializeField] private Vector3 corner4 = new Vector3(-16, 1, 75);  // 우상단
        [SerializeField] private float spawnHeight = 1f; // 스폰 높이
        [SerializeField] private LayerMask groundLayer;
        
        [Header("Scene-Specific Settings")]
        [Tooltip("현재 씬에 따라 스폰 영역을 자동으로 설정")]
        [SerializeField] private bool useSceneSpecificAreas = true;
        
        // 사각형 영역 계산용 변수
        private float minX, maxX, minZ, maxZ;

        [Header("Debug")]
        [SerializeField] private bool showSpawnArea = true;

        private List<GameObject> spawnedItems = new List<GameObject>();
        private float nextSpawnTime;
        private float totalWeight;

        private void Start()
        {
            // 씬별 스폰 영역 설정
            if (useSceneSpecificAreas)
            {
                SetSceneSpecificSpawnArea();
            }
            
            // 사각형 영역 범위 계산
            CalculateSpawnBounds();
            
            // 전체 가중치 계산
            CalculateTotalWeight();

            // 초기 스폰
            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnRandomItem();
            }

            // 코루틴으로 스폰 시스템 시작
            StartCoroutine(SpawnCoroutine());
        }
        
        /// <summary>
        /// 씬에 따라 스폰 영역을 자동으로 설정
        /// </summary>
        private void SetSceneSpecificSpawnArea()
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            switch (currentSceneName)
            {
                case "Stage2":
                case "Round2":
                    // Stage2/Round2 씬 - 복도 영역
                    corner1 = new Vector3(-85, 1, 72);  // 좌하단
                    corner2 = new Vector3(-85, 1, 75);  // 좌상단
                    corner3 = new Vector3(-16, 1, 72);  // 우하단
                    corner4 = new Vector3(-16, 1, 75);  // 우상단
                    Debug.Log($"[RandomItemSpawner] {currentSceneName} 복도 영역으로 설정됨");
                    break;
                    
                case "Playground":
                    // Playground 씬 - 교실 영역 (씬 파일에서 확인한 실제 좌표)
                    // spawnAreaCenter: (-41, 3, 96), spawnAreaSize: (51, 4, 40)
                    // 중심에서 크기의 절반만큼 빼고 더해서 모서리 계산
                    float centerX = -41f, centerZ = 96f;
                    float halfSizeX = 51f / 2f, halfSizeZ = 40f / 2f;
                    corner1 = new Vector3(centerX - halfSizeX, 1, centerZ - halfSizeZ);  // 좌하단
                    corner2 = new Vector3(centerX - halfSizeX, 1, centerZ + halfSizeZ);  // 좌상단
                    corner3 = new Vector3(centerX + halfSizeX, 1, centerZ - halfSizeZ);  // 우하단
                    corner4 = new Vector3(centerX + halfSizeX, 1, centerZ + halfSizeZ);  // 우상단
                    Debug.Log("[RandomItemSpawner] Playground 교실 영역으로 설정됨");
                    break;
                    
                default:
                    // 기본값은 현재 설정된 좌표 유지
                    Debug.Log($"[RandomItemSpawner] 알 수 없는 씬: {currentSceneName}, 기본 좌표 사용");
                    break;
            }
        }
        
        /// <summary>
        /// 사각형 영역의 최소/최대 좌표 계산
        /// </summary>
        private void CalculateSpawnBounds()
        {
            // 네 모서리 중 최소/최대 X, Z 좌표 찾기
            minX = Mathf.Min(corner1.x, corner2.x, corner3.x, corner4.x);
            maxX = Mathf.Max(corner1.x, corner2.x, corner3.x, corner4.x);
            minZ = Mathf.Min(corner1.z, corner2.z, corner3.z, corner4.z);
            maxZ = Mathf.Max(corner1.z, corner2.z, corner3.z, corner4.z);
        }

        /// <summary>
        /// 코루틴 기반 스폰 시스템 (Update 대신 사용하여 성능 향상)
        /// </summary>
        private IEnumerator SpawnCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);

                // null이거나 부모가 있는 아이템 제거 (플레이어가 들고 있는 아이템)
                spawnedItems.RemoveAll(item => item == null || item.transform.parent != null);

                if (spawnedItems.Count < maxItemsInScene)
                {
                    SpawnRandomItem();
                }
            }
        }

        private void CalculateTotalWeight()
        {
            totalWeight = 0f;
            foreach (var item in spawnableItems)
            {
                totalWeight += item.spawnWeight;
            }
        }

        private void SpawnRandomItem()
        {
            if (spawnableItems.Count == 0)
            {
                Debug.LogWarning("[RandomItemSpawner] 스폰 가능한 아이템이 없습니다!");
                return;
            }

            // 가중치 기반 랜덤 선택
            GameObject selectedPrefab = SelectRandomItem();
            if (selectedPrefab == null) return;

            // 랜덤 위치 계산
            Vector3 randomPosition = GetRandomSpawnPosition();

            // 아이템 생성
            GameObject item = Instantiate(selectedPrefab, randomPosition, Quaternion.identity);

            // 활성화 확인 및 초기화 (연필창 버그 방지)
            if (!item.activeInHierarchy)
            {
                item.SetActive(true);
                Debug.LogWarning($"[RandomItemSpawner] 생성된 아이템이 비활성 상태였습니다: {item.name}");
            }

            // ItemBase 컴포넌트 확인
            ItemBase itemBase = item.GetComponent<ItemBase>();
            if (itemBase != null && !itemBase.enabled)
            {
                itemBase.enabled = true;
                Debug.LogWarning($"[RandomItemSpawner] ItemBase가 비활성 상태였습니다: {item.name}");
            }

            spawnedItems.Add(item);

            Debug.Log($"[RandomItemSpawner] {selectedPrefab.name} 스폰 at {randomPosition}");
        }

        private GameObject SelectRandomItem()
        {
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var item in spawnableItems)
            {
                currentWeight += item.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return item.itemPrefab;
                }
            }

            // 기본값 (첫 번째 아이템)
            return spawnableItems[0].itemPrefab;
        }

        private Vector3 GetRandomSpawnPosition()
        {
            // 사각형 영역 내 랜덤 위치 생성
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            
            Vector3 randomPosition = new Vector3(randomX, 0, randomZ);

            if (Physics.Raycast(randomPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                randomPosition.y = hit.point.y + spawnHeight;
            }
            else
            {
                randomPosition.y = spawnHeight;
            }

            return randomPosition;
        }

        // 기즈모 표시를 완전히 비활성화
        // private void OnDrawGizmos() 메서드를 주석 처리하여 기즈모가 표시되지 않도록 함
    }
}
