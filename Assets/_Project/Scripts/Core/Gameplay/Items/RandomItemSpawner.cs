using UnityEngine;
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

        [Header("Spawn Area")]
        [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
        [SerializeField] private Vector3 spawnAreaSize = new Vector3(20, 0, 20);
        [SerializeField] private float spawnHeight = 0f; // 바닥에 바로 생성
        [SerializeField] private LayerMask groundLayer;

        [Header("Debug")]
        [SerializeField] private bool showSpawnArea = true;

        private List<GameObject> spawnedItems = new List<GameObject>();
        private float nextSpawnTime;
        private float totalWeight;

        private void Start()
        {
            // 전체 가중치 계산
            CalculateTotalWeight();

            // 초기 스폰
            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnRandomItem();
            }

            nextSpawnTime = Time.time + spawnInterval;
        }

        private void Update()
        {
            if (Time.time >= nextSpawnTime)
            {
                // null이거나 부모가 있는 아이템 제거 (플레이어가 들고 있는 아이템)
                spawnedItems.RemoveAll(item => item == null || item.transform.parent != null);

                if (spawnedItems.Count < maxItemsInScene)
                {
                    SpawnRandomItem();
                }

                nextSpawnTime = Time.time + spawnInterval;
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
            float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);

            Vector3 randomPosition = spawnAreaCenter + new Vector3(randomX, 0, randomZ);

            if (Physics.Raycast(randomPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                randomPosition.y = hit.point.y + spawnHeight;
            }
            else
            {
                randomPosition.y = spawnAreaCenter.y + spawnHeight;
            }

            return randomPosition;
        }

        private void OnDrawGizmos()
        {
            if (!showSpawnArea) return;

            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
        }
    }
}
