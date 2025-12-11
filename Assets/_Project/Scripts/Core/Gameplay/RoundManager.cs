using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using YajaGame.Gameplay.Combat;

namespace YajaGame.Gameplay
{
    /// <summary>
    /// 라운드 상태
    /// </summary>
    public enum RoundState
    {
        Waiting,        // 대기 중
        InProgress,     // 라운드 진행 중
        RoundClear,     // 라운드 클리어
        GameOver        // 게임 오버
    }

    /// <summary>
    /// 라운드 시스템 관리자
    /// 1라운드: 교실에서 드론+선생님 처치
    /// 2라운드: 준원이 제작할 복도 씬
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        public static RoundManager Instance { get; private set; }

        [Header("Round Settings")]
        [SerializeField] private int currentRound = 1;
        [SerializeField] private float roundStartDelay = 2f;
        [SerializeField] private float roundClearDelay = 3f;

        [Header("Enemy Spawn Settings")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private GameObject teacherPrefab;
        [SerializeField] private GameObject dronePrefab;
        [SerializeField] private int teacherCount = 1;
        [SerializeField] private int droneCount = 3;
        
        [Header("Drone Random Spawn Area - Corridor")]
        [Tooltip("드론 랜덤 스폰을 위한 복도 구역")]
        [SerializeField] private bool useRandomDroneSpawn = true;
        [SerializeField] private Vector3 droneSpawnCorner1 = new Vector3(-85, 3, 72);  // 좌하단 (높이 3m)
        [SerializeField] private Vector3 droneSpawnCorner2 = new Vector3(-85, 3, 75);  // 좌상단
        [SerializeField] private Vector3 droneSpawnCorner3 = new Vector3(-16, 3, 72);  // 우하단
        [SerializeField] private Vector3 droneSpawnCorner4 = new Vector3(-16, 3, 75);  // 우상단
        
        // 드론 스폰 영역 계산용 변수
        private float droneMinX, droneMaxX, droneMinZ, droneMaxZ;

        [Header("Scene Transition")]
        [SerializeField] private string nextSceneName = "MainMenu";
        [SerializeField] private bool useSceneTransition = true;

        [Header("UI References")]
        [SerializeField] private GameObject roundStartPanel;
        [SerializeField] private Image roundStartImage;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI enemyCountText;
        [SerializeField] private GameObject roundClearPanel;
        [SerializeField] private Image roundClearImage;

        [Header("Round Start Images")]
        [SerializeField] private Sprite round1Image;
        [SerializeField] private Sprite round2Image;
        [SerializeField] private Sprite round3Image;

        [Header("Audio")]
        [SerializeField] private AudioClip roundStartSound;
        [SerializeField] private AudioClip roundClearSound;
        [SerializeField] private AudioClip enemyDeathSound;

        [Header("Events")]
        public UnityEvent OnRoundStart = new UnityEvent();
        public UnityEvent OnRoundClear = new UnityEvent();
        public UnityEvent<int> OnEnemyCountChanged = new UnityEvent<int>();
        public UnityEvent<int> OnDroneCountChanged = new UnityEvent<int>();

        // 상태 변수
        private RoundState _state = RoundState.Waiting;
        private int _remainingEnemies = 0;
        private int _remainingDrones = 0;
        private List<GameObject> _spawnedEnemies = new List<GameObject>();
        private List<GameObject> _spawnedDrones = new List<GameObject>();

        // 프로퍼티
        public RoundState State => _state;
        public int CurrentRound => currentRound;
        public int RemainingEnemies => _remainingEnemies;
        public int RemainingDrones => _remainingDrones;

        private void Awake()
        {
            // 싱글톤 설정
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // 드론 스폰 영역 계산
            CalculateDroneSpawnBounds();
            
            // UI 초기화
            if (roundStartPanel != null) roundStartPanel.SetActive(false);
            if (roundClearPanel != null) roundClearPanel.SetActive(false);

            // 라운드 시작
            StartCoroutine(StartRoundWithDelay(roundStartDelay));
        }
        
        /// <summary>
        /// 드론 스폰 영역의 최소/최대 좌표 계산
        /// </summary>
        private void CalculateDroneSpawnBounds()
        {
            droneMinX = Mathf.Min(droneSpawnCorner1.x, droneSpawnCorner2.x, droneSpawnCorner3.x, droneSpawnCorner4.x);
            droneMaxX = Mathf.Max(droneSpawnCorner1.x, droneSpawnCorner2.x, droneSpawnCorner3.x, droneSpawnCorner4.x);
            droneMinZ = Mathf.Min(droneSpawnCorner1.z, droneSpawnCorner2.z, droneSpawnCorner3.z, droneSpawnCorner4.z);
            droneMaxZ = Mathf.Max(droneSpawnCorner1.z, droneSpawnCorner2.z, droneSpawnCorner3.z, droneSpawnCorner4.z);
        }
        
        /// <summary>
        /// 복도 구역 내 랜덤 드론 스폰 위치 생성
        /// </summary>
        private Vector3 GetRandomDroneSpawnPosition()
        {
            float randomX = Random.Range(droneMinX, droneMaxX);
            float randomZ = Random.Range(droneMinZ, droneMaxZ);
            float randomY = Random.Range(3f, 5f); // 드론은 공중에 떠있음
            
            return new Vector3(randomX, randomY, randomZ);
        }

        /// <summary>
        /// 딜레이 후 라운드 시작
        /// </summary>
        private IEnumerator StartRoundWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartRound();
        }

        /// <summary>
        /// 라운드 시작
        /// </summary>
        public void StartRound()
        {
            if (_state == RoundState.InProgress) return;

            _state = RoundState.InProgress;
            Debug.Log($"[RoundManager] 라운드 {currentRound} 시작!");

            // 라운드 시작 UI 표시
            StartCoroutine(ShowRoundStartUI());

            // 적 스폰
            SpawnEnemies();

            // UI 업데이트
            UpdateUI();

            // 사운드 재생
            if (roundStartSound != null)
            {
                AudioSource.PlayClipAtPoint(roundStartSound, Camera.main.transform.position);
            }

            // 이벤트 발생
            OnRoundStart?.Invoke();
        }

        /// <summary>
        /// 라운드 시작 UI 표시
        /// </summary>
        private IEnumerator ShowRoundStartUI()
        {
            if (roundStartPanel != null)
            {
                // 라운드에 맞는 이미지 설정
                if (roundStartImage != null)
                {
                    switch (currentRound)
                    {
                        case 1:
                            if (round1Image != null) roundStartImage.sprite = round1Image;
                            break;
                        case 2:
                            if (round2Image != null) roundStartImage.sprite = round2Image;
                            break;
                        case 3:
                            if (round3Image != null) roundStartImage.sprite = round3Image;
                            break;
                    }
                }

                roundStartPanel.SetActive(true);
                yield return new WaitForSeconds(2f);
                roundStartPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 적 스폰
        /// </summary>
        private void SpawnEnemies()
        {
            _spawnedEnemies.Clear();
            _spawnedDrones.Clear();
            _remainingEnemies = 0;
            _remainingDrones = 0;

            // 먼저 씬에 이미 있는 적들 등록
            RegisterExistingEnemies();

            // 프리팹이 설정되어 있으면 추가 스폰
            if (teacherPrefab != null || dronePrefab != null)
            {
                int spawnIndex = 0;

                // 선생님 스폰
                for (int i = 0; i < teacherCount; i++)
                {
                    if (teacherPrefab != null && spawnIndex < spawnPoints.Length)
                    {
                        GameObject teacher = Instantiate(teacherPrefab, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
                        RegisterEnemy(teacher);
                        spawnIndex++;
                        Debug.Log($"[RoundManager] 선생님 스폰: {spawnPoints[spawnIndex - 1].position}");
                    }
                }

                // 드론 스폰
                for (int i = 0; i < droneCount; i++)
                {
                    if (dronePrefab != null)
                    {
                        Vector3 spawnPosition;
                        
                        if (useRandomDroneSpawn)
                        {
                            // 복도 구역에서 랜덤 스폰
                            spawnPosition = GetRandomDroneSpawnPosition();
                        }
                        else if (spawnIndex < spawnPoints.Length)
                        {
                            // 기존 스폰 포인트 사용
                            spawnPosition = spawnPoints[spawnIndex].position;
                            spawnIndex++;
                        }
                        else
                        {
                            // 기본 위치
                            spawnPosition = new Vector3(-50, 3, 73.5f);
                        }
                        
                        GameObject drone = Instantiate(dronePrefab, spawnPosition, Quaternion.identity);
                        RegisterEnemy(drone, isDrone: true);  // 드론으로 등록
                        Debug.Log($"[RoundManager] 드론 스폰: {spawnPosition}");
                    }
                }
            }

            Debug.Log($"[RoundManager] 총 {_remainingEnemies}명의 적 스폰/등록 완료 (드론: {_remainingDrones})");

            // UI 업데이트 (초기 적 수 표시)
            OnEnemyCountChanged?.Invoke(_remainingEnemies);
            OnDroneCountChanged?.Invoke(_remainingDrones);
        }

        /// <summary>
        /// 씬에 이미 있는 적들 등록
        /// </summary>
        private void RegisterExistingEnemies()
        {
            // SimpleAIController (선생님) 찾기
            SimpleAIController[] teachers = FindObjectsOfType<SimpleAIController>();
            foreach (var teacher in teachers)
            {
                RegisterEnemy(teacher.gameObject, isDrone: false);
                Debug.Log($"[RoundManager] 기존 선생님 등록: {teacher.name}");
            }

            // DroneAIController (드론) 찾기
            DroneAIController[] drones = FindObjectsOfType<DroneAIController>();
            foreach (var drone in drones)
            {
                RegisterEnemy(drone.gameObject, isDrone: true);
                Debug.Log($"[RoundManager] 기존 드론 등록: {drone.name}");
            }
        }

        /// <summary>
        /// 적 등록
        /// </summary>
        private void RegisterEnemy(GameObject enemy, bool isDrone = false)
        {
            _spawnedEnemies.Add(enemy);
            _remainingEnemies++;

            // 드론인 경우 별도 리스트에도 추가
            if (isDrone)
            {
                _spawnedDrones.Add(enemy);
                _remainingDrones++;
                Debug.Log($"[RoundManager] 드론 등록: {enemy.name}, 총 드론: {_remainingDrones}");
            }
            else
            {
                Debug.Log($"[RoundManager] 적 등록: {enemy.name}, 총 {_remainingEnemies}명 (드론 외)");
            }

            // 참고: 적 사망 시 DroneAIController.Die() 또는 SimpleAIController.Die()에서
            // 직접 RoundManager.Instance.OnEnemyDeath() 호출함
        }

        /// <summary>
        /// 적 사망 처리 (EnemyHealth.OnDeath에서 호출)
        /// </summary>
        public void OnEnemyDeath(GameObject enemy = null)
        {
            if (_state != RoundState.InProgress) return;

            // 중복 호출 방지: 이미 제거된 적이면 무시
            if (enemy != null && !_spawnedEnemies.Contains(enemy))
            {
                Debug.Log($"[RoundManager] 이미 처리된 적: {enemy.name}");
                return;
            }

            _remainingEnemies--;
            _remainingEnemies = Mathf.Max(0, _remainingEnemies);

            // 드론인 경우 드론 카운트도 감소
            bool wasDrone = false;
            if (enemy != null)
            {
                _spawnedEnemies.Remove(enemy);

                // 드론 리스트에서도 제거
                if (_spawnedDrones.Contains(enemy))
                {
                    _spawnedDrones.Remove(enemy);
                    _remainingDrones--;
                    _remainingDrones = Mathf.Max(0, _remainingDrones);
                    wasDrone = true;
                    Debug.Log($"[RoundManager] 드론 처치! 남은 드론: {_remainingDrones}");
                }
            }

            Debug.Log($"[RoundManager] 적 처치! 남은 적: {_remainingEnemies}");

            // 사운드 재생
            if (enemyDeathSound != null)
            {
                AudioSource.PlayClipAtPoint(enemyDeathSound, Camera.main.transform.position);
            }

            // UI 업데이트
            UpdateUI();

            // 이벤트 발생
            OnEnemyCountChanged?.Invoke(_remainingEnemies);

            // 드론 카운트 변경 이벤트 (드론이 죽었을 때만)
            if (wasDrone)
            {
                OnDroneCountChanged?.Invoke(_remainingDrones);
            }

            // 라운드 클리어 체크
            CheckRoundClear();
        }

        /// <summary>
        /// 라운드 클리어 체크
        /// 드론 3개와 보스(선생님) 1개를 모두 처치해야 클리어
        /// </summary>
        private void CheckRoundClear()
        {
            // 모든 적이 처치되었는지 확인 (드론 3개 + 보스 1개 = 총 4개)
            if (_remainingEnemies <= 0)
            {
                Debug.Log($"[RoundManager] 라운드 클리어 조건 달성! 드론: {_remainingDrones}/3, 총 적: {_remainingEnemies}");
                RoundClear();
            }
            else
            {
                Debug.Log($"[RoundManager] 아직 적이 남아있음. 드론: {_remainingDrones}/3, 총 적: {_remainingEnemies}");
            }
        }

        /// <summary>
        /// 라운드 클리어 처리
        /// </summary>
        private void RoundClear()
        {
            if (_state == RoundState.RoundClear) return;

            _state = RoundState.RoundClear;
            Debug.Log($"[RoundManager] 라운드 {currentRound} 클리어!");

            // 클리어 UI 표시
            if (roundClearPanel != null)
            {
                roundClearPanel.SetActive(true);
            }

            // 사운드 재생
            if (roundClearSound != null)
            {
                AudioSource.PlayClipAtPoint(roundClearSound, Camera.main.transform.position);
            }

            // 이벤트 발생
            OnRoundClear?.Invoke();

            // 다음 씬으로 전환
            if (useSceneTransition)
            {
                Invoke(nameof(LoadNextScene), roundClearDelay);
            }
        }

        /// <summary>
        /// 다음 씬 로드
        /// </summary>
        private void LoadNextScene()
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[RoundManager] 다음 씬으로 전환: {nextSceneName}");
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("[RoundManager] 다음 씬 이름이 설정되지 않았습니다!");
            }
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        private void UpdateUI()
        {
            // 라운드 텍스트
            if (roundText != null)
            {
                roundText.text = $"라운드 {currentRound}";
            }

            // 남은 적 텍스트 (드론과 보스 구분해서 표시)
            if (enemyCountText != null)
            {
                int remainingBosses = _remainingEnemies - _remainingDrones;
                enemyCountText.text = $"남은 적: {_remainingEnemies} (드론: {_remainingDrones}, 보스: {remainingBosses})";
            }
        }

        /// <summary>
        /// 게임 오버 처리 (GameManager에서 호출 가능)
        /// </summary>
        public void GameOver()
        {
            if (_state == RoundState.GameOver) return;

            _state = RoundState.GameOver;
            Debug.Log("[RoundManager] 게임 오버!");

            // 남은 적들 정리
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy != null)
                {
                    // AI 비활성화
                    MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();
                    foreach (var script in scripts)
                    {
                        script.enabled = false;
                    }
                }
            }

            // 게임 오버 후 MainMenu로 전환
            StartCoroutine(LoadMainMenuAfterGameOver());
        }

        /// <summary>
        /// 게임 오버 후 MainMenu 씬으로 전환
        /// </summary>
        private System.Collections.IEnumerator LoadMainMenuAfterGameOver()
        {
            yield return new WaitForSeconds(3f); // 3초 대기
            
            Debug.Log("[RoundManager] 게임 오버 - MainMenu 씬으로 전환");
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// 라운드 재시작 (테스트용)
        /// </summary>
        [ContextMenu("Restart Round")]
        public void RestartRound()
        {
            // 기존 적 제거
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy != null) Destroy(enemy);
            }
            _spawnedEnemies.Clear();

            // UI 초기화
            if (roundStartPanel != null) roundStartPanel.SetActive(false);
            if (roundClearPanel != null) roundClearPanel.SetActive(false);

            // 라운드 시작
            _state = RoundState.Waiting;
            StartRound();
        }

        /// <summary>
        /// 모든 적 즉시 처치 (디버그용)
        /// </summary>
        [ContextMenu("Kill All Enemies")]
        public void KillAllEnemies()
        {
            List<GameObject> enemies = new List<GameObject>(_spawnedEnemies);
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                    if (health != null)
                    {
                        health.KillInstantly();
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 스폰 포인트 시각화
            if (spawnPoints != null)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    if (spawnPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);
                        Gizmos.DrawLine(spawnPoints[i].position, spawnPoints[i].position + Vector3.up * 2f);

#if UNITY_EDITOR
                        UnityEditor.Handles.Label(spawnPoints[i].position + Vector3.up * 2.5f, $"Spawn {i + 1}");
#endif
                    }
                }
            }
        }
    }
}
