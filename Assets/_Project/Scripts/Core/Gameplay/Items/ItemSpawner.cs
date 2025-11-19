using UnityEngine;
using System.Collections.Generic;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 맵에 아이템을 랜덤 스폰
    /// </summary>
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject itemPrefab; // 스폰할 아이템 프리팹
        [SerializeField] private int initialSpawnCount = 5; // 시작 시 스폰 개수
        [SerializeField] private float spawnInterval = 10f; // 재스폰 간격 (초)
        [SerializeField] private int maxItemsInScene = 10; // 맵에 동시 존재 가능한 최대 개수

        [Header("Spawn Area")]
        [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero; // 스폰 영역 중심
        [SerializeField] private Vector3 spawnAreaSize = new Vector3(20, 0, 20); // 스폰 영역 크기 (XZ 평면)
        [SerializeField] private float spawnHeight = 0.3f; // 바닥에서 얼마나 위에 스폰할지 (낮춤)
        [SerializeField] private LayerMask groundLayer; // 바닥 레이어

        [Header("Debug")]
        [SerializeField] private bool showSpawnArea = true; // 기즈모로 스폰 영역 표시

        private List<GameObject> spawnedItems = new List<GameObject>();
        private float nextSpawnTime;

        private void Start()
        {
            // 게임 시작 시 초기 아이템 스폰
            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnItem();
            }

            nextSpawnTime = Time.time + spawnInterval;
        }

        private void Update()
        {
            // 주기적으로 아이템 재스폰
            if (Time.time >= nextSpawnTime)
            {
                // 제거된 아이템 리스트에서 삭제
                spawnedItems.RemoveAll(item => item == null);

                // 최대 개수 미만이면 스폰
                if (spawnedItems.Count < maxItemsInScene)
                {
                    SpawnItem();
                }

                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        private void SpawnItem()
        {
            if (itemPrefab == null)
            {
                Debug.LogWarning("[ItemSpawner] 아이템 프리팹이 설정되지 않았습니다!");
                return;
            }

            // 랜덤 위치 계산
            Vector3 randomPosition = GetRandomSpawnPosition();

            // 아이템 생성
            GameObject item = Instantiate(itemPrefab, randomPosition, Quaternion.identity);

            // 활성화 확인 및 초기화 (연필창 버그 방지)
            if (!item.activeInHierarchy)
            {
                item.SetActive(true);
                Debug.LogWarning($"[ItemSpawner] 생성된 아이템이 비활성 상태였습니다: {item.name}");
            }

            // ItemBase 컴포넌트 확인
            ItemBase itemBase = item.GetComponent<ItemBase>();
            if (itemBase != null && !itemBase.enabled)
            {
                itemBase.enabled = true;
                Debug.LogWarning($"[ItemSpawner] ItemBase가 비활성 상태였습니다: {item.name}");
            }

            spawnedItems.Add(item);

            Debug.Log($"[ItemSpawner] 아이템 스폰: {item.name} at {randomPosition}");
        }

        private Vector3 GetRandomSpawnPosition()
        {
            // 스폰 영역 내 랜덤 위치
            float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);

            Vector3 randomPosition = spawnAreaCenter + new Vector3(randomX, 0, randomZ);

            // 레이캐스트로 바닥 높이 찾기
            if (Physics.Raycast(randomPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                randomPosition.y = hit.point.y + spawnHeight;
            }
            else
            {
                // 레이캐스트 실패 시 기본 높이 사용
                randomPosition.y = spawnAreaCenter.y + spawnHeight;
            }

            return randomPosition;
        }

        /// <summary>
        /// 수동으로 아이템 스폰 (외부에서 호출 가능)
        /// </summary>
        public void ForceSpawnItem()
        {
            SpawnItem();
        }

        /// <summary>
        /// 특정 위치에 아이템 스폰
        /// </summary>
        public void SpawnItemAt(Vector3 position)
        {
            if (itemPrefab == null) return;

            GameObject item = Instantiate(itemPrefab, position, Quaternion.identity);
            spawnedItems.Add(item);
        }

        private void OnDrawGizmos()
        {
            if (!showSpawnArea) return;

            // 스폰 영역 표시
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
        }
    }
}
