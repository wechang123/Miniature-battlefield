using UnityEngine;
using TMPro;  // TextMeshPro 사용 시
// using UnityEngine.UI;  // Legacy Text 사용 시

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;  // 싱글톤
    
    [Header("UI")]
    public GameObject gameOverText;      // 게임오버 텍스트
    
    [Header("Settings")]
    public float restartDelay = 3f;      // 재시작까지 대기 시간
    
    private bool isGameOver = false;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 게임 시작 시 UI 숨김
        if (gameOverText != null)
        {
            gameOverText.SetActive(false);
        }
    }

    public void PlayerCaught()
    {
        if (isGameOver) return;  // 이미 게임오버면 무시
        
        isGameOver = true;
        
        // 게임오버 텍스트 표시
        if (gameOverText != null)
        {
            gameOverText.SetActive(true);
        }
        
        // 플레이어 움직임 정지
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Character Controller 비활성화
            var controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
            
            // Third Person Controller 비활성화
            var tpc = player.GetComponent<MonoBehaviour>();
            if (tpc != null)
            {
                tpc.enabled = false;
            }
        }
        
        Debug.Log("플레이어 아웃!");
        
        // 3초 후 재시작
        Invoke(nameof(RestartGame), restartDelay);
    }

    void RestartGame()
    {
        // 현재 씬 재시작
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void Update()
    {
        // R키로 즉시 재시작 (테스트용)
        //if (isGameOver && Input.GetKeyDown(KeyCode.R))
        //{
        //    RestartGame();
        //}
    }
}
