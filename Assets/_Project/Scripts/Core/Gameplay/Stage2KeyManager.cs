using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;
using YajaGame.UI;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// Stage2 전용 키 수집 시스템 관리자
    /// 지정된 사각형 영역 내에서 키 5개를 랜덤 생성하고 수집 관리
    /// </summary>
    public class Stage2KeyManager : MonoBehaviour
    {
        [Header("Key Settings")]
        [SerializeField] private GameObject keyPrefab; // 키 프리팹
        [SerializeField] private int totalKeysToSpawn = 5; // 생성할 키 개수
        
        [Header("Spawn Area - Rectangle Corners")]
        [Tooltip("사각형 영역의 네 모서리 좌표")]
        [SerializeField] private Vector3 corner1 = new Vector3(-85, 1, 60);  // 좌하단
        [SerializeField] private Vector3 corner2 = new Vector3(-85, 1, 75);  // 좌상단
        [SerializeField] private Vector3 corner3 = new Vector3(-16, 1, 60);  // 우하단
        [SerializeField] private Vector3 corner4 = new Vector3(-16, 1, 75);  // 우상단
        
        [Header("Spawn Settings")]
        [SerializeField] private float spawnHeight = 1f; // 스폰 높이
        [SerializeField] private float minDistanceBetweenKeys = 5f; // 키들 간 최소 거리
        [SerializeField] private LayerMask groundLayer = 1; // Ground 레이어
        [SerializeField] private int maxSpawnAttempts = 20; // 최대 스폰 시도 횟수
        
        [Header("Exclusion Zone")]
        [Tooltip("키 스폰을 제외할 사각형 구역")]
        [SerializeField] private Vector3 exclusionZoneMin = new Vector3(-60f, 1f, 72f); // 제외 구역 최소 좌표
        [SerializeField] private Vector3 exclusionZoneMax = new Vector3(-50f, 1f, 75f); // 제외 구역 최대 좌표
        [SerializeField] private bool enableExclusionZone = true; // 제외 구역 활성화 여부
        
        [Header("Round Completion")]
        [SerializeField] private string nextSceneName = "MainMenu"; // 다음 씬 이름
        [SerializeField] private float roundCompleteDelay = 3f;
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI keyCountText;
        [SerializeField] private GameObject roundCompletePanel;
        [SerializeField] private TextMeshProUGUI roundCompleteText;
        [SerializeField] private KeyCollectionUI keyCollectionUI; // 키 수집 UI
        
        [Header("Audio")]
        [SerializeField] private AudioClip keyCollectSound;
        [SerializeField] private AudioClip roundCompleteSound;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject keySpawnEffect;
        [SerializeField] private GameObject roundCompleteEffect;
        
        [Header("Debug")]
        [SerializeField] private bool showSpawnArea = true;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Events")]
        public UnityEvent<int> OnKeyCountChanged = new UnityEvent<int>();
        
        // 상태 변수
        private int keysCollected = 0;
        private List<GameObject> spawnedKeys = new List<GameObject>();
        private bool roundCompleted = false;
        
        // 사각형 영역 계산용 변수
        private float minX, maxX, minZ, maxZ;
        
        // 프로퍼티
        public int KeysCollected => keysCollected;
        public int TotalKeys => totalKeysToSpawn;
        public bool IsRoundComplete => roundCompleted;
        
        private void Start()
        {
            InitializeKeySystem();
        }
        
        /// <summary>
        /// 키 시스템 초기화
        /// </summary>
        private void InitializeKeySystem()
        {
            // 사각형 영역 범위 계산
            CalculateSpawnBounds();
            
            // UI 초기화
            if (roundCompletePanel != null)
                roundCompletePanel.SetActive(false);
                
            // KeyCollectionUI 자동 찾기 및 초기화
            if (keyCollectionUI == null)
            {
                keyCollectionUI = FindObjectOfType<KeyCollectionUI>();
                if (keyCollectionUI != null)
                {
                    Debug.Log("[Stage2KeyManager] KeyCollectionUI 자동 연결 성공");
                }
            }
                
            UpdateUI();
            
            // 키 스폰
            StartCoroutine(SpawnKeysWithDelay());

            // 초기 키 수 이벤트 발생 (0개부터 시작)
            OnKeyCountChanged?.Invoke(keysCollected);
            
            if (enableDebugLogs)
            {
                Debug.Log($"[Stage2KeyManager] Stage2 시작! 지정된 영역에서 {totalKeysToSpawn}개의 키를 수집하세요.");
                Debug.Log($"[Stage2KeyManager] 스폰 영역: X({minX:F1} ~ {maxX:F1}), Z({minZ:F1} ~ {maxZ:F1})");
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
        /// 딜레이 후 키 스폰 (시각적 효과를 위해)
        /// </summary>
        private IEnumerator SpawnKeysWithDelay()
        {
            yield return new WaitForSeconds(1f); // 1초 대기
            
            SpawnKeys();
        }
        
        /// <summary>
        /// 키들을 사각형 영역 내 랜덤 위치에 스폰
        /// </summary>
        private void SpawnKeys()
        {
            if (keyPrefab == null)
            {
                Debug.LogError("[Stage2KeyManager] 키 프리팹이 설정되지 않았습니다!");
                return;
            }
            
            spawnedKeys.Clear();
            
            for (int i = 0; i < totalKeysToSpawn; i++)
            {
                Vector3 spawnPosition = GetValidSpawnPosition();
                
                if (spawnPosition != Vector3.zero)
                {
                    // 키 생성
                    GameObject key = Instantiate(keyPrefab, spawnPosition, GetRandomRotation());
                    
                    // KeyItem 컴포넌트 확인 및 추가
                    KeyItem keyItem = key.GetComponent<KeyItem>();
                    if (keyItem == null)
                    {
                        keyItem = key.AddComponent<KeyItem>();
                    }
                    
                    // 키 초기화
                    keyItem.Initialize();
                    
                    // 스폰 이펙트
                    if (keySpawnEffect != null)
                    {
                        Instantiate(keySpawnEffect, spawnPosition, Quaternion.identity);
                    }
                    
                    spawnedKeys.Add(key);
                    
                    if (enableDebugLogs)
                    {
                        Debug.Log($"[Stage2KeyManager] 키 {i + 1} 스폰 완료: {spawnPosition}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[Stage2KeyManager] 키 {i + 1} 스폰 실패 - 적절한 위치를 찾을 수 없습니다.");
                }
            }
            
            if (enableDebugLogs)
            {
                Debug.Log($"[Stage2KeyManager] 총 {spawnedKeys.Count}개의 키 스폰 완료!");
            }
        }
        
        /// <summary>
        /// 유효한 스폰 위치 찾기
        /// </summary>
        private Vector3 GetValidSpawnPosition()
        {
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                // 사각형 영역 내 랜덤 위치 생성
                Vector3 randomPosition = GetRandomPositionInRectangle();
                
                // 바닥 높이 감지
                if (Physics.Raycast(randomPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
                {
                    randomPosition.y = hit.point.y + spawnHeight;
                }
                else
                {
                    randomPosition.y = spawnHeight; // 기본 높이 사용
                }
                
                // 다른 키들과의 거리 체크
                if (IsValidDistance(randomPosition))
                {
                    return randomPosition;
                }
            }
            
            // 실패 시 기본 위치 반환 (영역 중심)
            Vector3 centerPosition = new Vector3((minX + maxX) / 2f, spawnHeight, (minZ + maxZ) / 2f);
            return centerPosition;
        }
        
        /// <summary>
        /// 사각형 영역 내 랜덤 위치 생성
        /// </summary>
        private Vector3 GetRandomPositionInRectangle()
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            return new Vector3(randomX, 0, randomZ);
        }
        
        /// <summary>
        /// 다른 키들과의 거리가 적절한지 체크
        /// </summary>
        private bool IsValidDistance(Vector3 position)
        {
            // 제외 구역 체크
            if (enableExclusionZone && IsInExclusionZone(position))
            {
                return false;
            }
            
            foreach (var existingKey in spawnedKeys)
            {
                if (existingKey != null)
                {
                    float distance = Vector3.Distance(position, existingKey.transform.position);
                    if (distance < minDistanceBetweenKeys)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        /// <summary>
        /// 위치가 제외 구역 내에 있는지 확인
        /// </summary>
        private bool IsInExclusionZone(Vector3 position)
        {
            return position.x >= exclusionZoneMin.x && position.x <= exclusionZoneMax.x &&
                   position.z >= exclusionZoneMin.z && position.z <= exclusionZoneMax.z;
        }
        
        /// <summary>
        /// 랜덤 회전값 생성 (Key prefab의 원래 회전값 유지)
        /// </summary>
        private Quaternion GetRandomRotation()
        {
            // Key prefab의 원래 회전값 (z축 -90도)을 유지하면서 Y축 랜덤 회전 추가
            float randomYRotation = Random.Range(0f, 360f);
            return Quaternion.Euler(0, randomYRotation, -90f);
        }
        
        /// <summary>
        /// 키 수집 처리 (KeyItem에서 호출)
        /// </summary>
        public void OnKeyCollected()
        {
            if (roundCompleted) 
            {
                Debug.LogWarning("[Stage2KeyManager] 라운드가 이미 완료되었습니다. 키 수집 무시됨.");
                return;
            }
            
            keysCollected++;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[Stage2KeyManager] ★ 키 수집! 현재: {keysCollected}/{totalKeysToSpawn}");
            }
            
            // 사운드 재생
            if (keyCollectSound != null)
            {
                AudioSource.PlayClipAtPoint(keyCollectSound, Camera.main.transform.position, 0.8f);
            }
            
            // UI 업데이트
            UpdateUI();
            
            // KeyCollectionUI 업데이트
            if (keyCollectionUI != null)
            {
                keyCollectionUI.OnKeyCollected(keysCollected);
            }

            // RoundUI 키 카운트 이벤트 발생
            OnKeyCountChanged?.Invoke(keysCollected);
            
            // 모든 키를 수집했는지 체크
            if (keysCollected >= totalKeysToSpawn)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[Stage2KeyManager] ★★★ 모든 키 수집 완료! CompleteRound() 호출");
                }
                CompleteRound();
            }
        }
        
        /// <summary>
        /// 라운드 완료 처리
        /// </summary>
        private void CompleteRound()
        {
            if (roundCompleted) 
            {
                Debug.LogWarning("[Stage2KeyManager] CompleteRound()가 이미 호출되었습니다.");
                return;
            }
            
            roundCompleted = true;
            
            Debug.Log("[Stage2KeyManager] ★★★ CompleteRound() 시작! 모든 키를 수집했습니다!");
            
            // RoundManager의 라운드 클리어 이벤트 호출 (Round Successful UI 표시를 위해)
            if (RoundManager.Instance != null)
            {
                Debug.Log("[Stage2KeyManager] ★★★ RoundManager.OnRoundClear 이벤트 호출!");
                RoundManager.Instance.OnRoundClear?.Invoke();
            }
            else
            {
                Debug.LogWarning("[Stage2KeyManager] RoundManager.Instance가 null입니다. Round Successful UI가 표시되지 않을 수 있습니다.");
            }
            
            // 완료 이펙트
            if (roundCompleteEffect != null)
            {
                Instantiate(roundCompleteEffect, Camera.main.transform.position, Quaternion.identity);
            }
            
            // 완료 UI 표시 (기존 Stage2 전용 UI)
            if (roundCompletePanel != null)
            {
                roundCompletePanel.SetActive(true);
                if (roundCompleteText != null)
                {
                    roundCompleteText.text = "모든 키를 수집했습니다!\nMainMenu 씬으로 이동합니다...";
                }
            }
            
            // 사운드 재생
            if (roundCompleteSound != null)
            {
                AudioSource.PlayClipAtPoint(roundCompleteSound, Camera.main.transform.position);
            }
            
            Debug.Log($"[Stage2KeyManager] ★★★ {roundCompleteDelay}초 후 '{nextSceneName}' 씬으로 전환 시작");
            
            // 딜레이 후 다음 씬으로 전환
            StartCoroutine(LoadNextSceneWithDelay());
        }
        
        /// <summary>
        /// 딜레이 후 다음 씬 로드
        /// </summary>
        private IEnumerator LoadNextSceneWithDelay()
        {
            Debug.Log($"[Stage2KeyManager] ★★★ LoadNextSceneWithDelay() 시작 - {roundCompleteDelay}초 대기 중...");
            
            yield return new WaitForSeconds(roundCompleteDelay);
            
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[Stage2KeyManager] ★★★ 씬 전환 시도: '{nextSceneName}'");
                
                // 씬이 빌드 설정에 있는지 확인
                int sceneIndex = SceneManager.GetSceneByName(nextSceneName).buildIndex;
                if (sceneIndex >= 0)
                {
                    Debug.Log($"[Stage2KeyManager] ★★★ 씬 '{nextSceneName}' 발견됨. 빌드 인덱스: {sceneIndex}");
                }
                else
                {
                    Debug.LogWarning($"[Stage2KeyManager] ⚠️ 씬 '{nextSceneName}'이 빌드 설정에 없을 수 있습니다!");
                }
                
                try
                {
                    SceneManager.LoadScene(nextSceneName);
                    Debug.Log($"[Stage2KeyManager] ★★★ SceneManager.LoadScene('{nextSceneName}') 호출 완료!");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Stage2KeyManager] ❌ 씬 로드 실패: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("[Stage2KeyManager] ❌ 다음 씬 이름이 설정되지 않았습니다!");
            }
        }
        
        /// <summary>
        /// UI 업데이트
        /// </summary>
        private void UpdateUI()
        {
            if (keyCountText != null)
            {
                keyCountText.text = $"키: {keysCollected}/{totalKeysToSpawn}";
            }
        }
        
        /// <summary>
        /// 디버그용 - 모든 키 즉시 수집
        /// </summary>
        [ContextMenu("Collect All Keys (Debug)")]
        public void CollectAllKeysDebug()
        {
            if (enableDebugLogs)
            {
                Debug.Log("[Stage2KeyManager] 디버그: 모든 키 즉시 수집");
            }
            
            // 남은 키들 강제 수집
            int remainingKeys = totalKeysToSpawn - keysCollected;
            for (int i = 0; i < remainingKeys; i++)
            {
                OnKeyCollected();
            }
        }
        
        /// <summary>
        /// 디버그용 - 키 재스폰
        /// </summary>
        [ContextMenu("Respawn Keys (Debug)")]
        public void RespawnKeysDebug()
        {
            if (enableDebugLogs)
            {
                Debug.Log("[Stage2KeyManager] 디버그: 키 재스폰");
            }
            
            // 기존 키들 제거
            foreach (var key in spawnedKeys)
            {
                if (key != null)
                {
                    DestroyImmediate(key);
                }
            }
            
            // 상태 초기화
            spawnedKeys.Clear();
            keysCollected = 0;
            roundCompleted = false;
            
            if (roundCompletePanel != null)
                roundCompletePanel.SetActive(false);
            
            UpdateUI();
            
            // KeyCollectionUI 리셋
            if (keyCollectionUI != null)
            {
                keyCollectionUI.OnKeyCollected(0);
            }

            // RoundUI 키 카운트 리셋 이벤트 발생
            OnKeyCountChanged?.Invoke(keysCollected);
            
            // 키 재스폰
            SpawnKeys();
        }
        
        // 기즈모 표시를 완전히 비활성화
        // private void OnDrawGizmos() 메서드를 주석 처리하여 기즈모가 표시되지 않도록 함
    }
}