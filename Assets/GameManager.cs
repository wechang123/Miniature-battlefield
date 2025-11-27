using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject gameOverText;

    [Header("Settings")]
    public float restartDelay = 3f;

    [Header("Player & Camera")]
    public GameObject player;
    public Camera playerCamera;
    public Transform npcAttacker;
    public Vector3 cameraOffset = new Vector3(0, 1.5f, -2f);
    public bool lockCameraOnAttack = true;

    private bool isGameOver = false;
    private bool isCameraLocked = false;

    void Awake()
    {
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
        if (gameOverText != null)
        {
            gameOverText.SetActive(false);
        }

        // �ڵ����� �÷��̾�� ī�޶� ã��
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        // ���� �� ī�޶� ����
        if (isCameraLocked && npcAttacker != null && playerCamera != null)
        {
            LockCameraOnNPC();
        }
    }

    // �⺻ ȣ�� (�Ű����� ����)
    public void PlayerCaught()
    {
        PlayerCaught(null);
    }

    // Transform �޴� �޼��� (ī�޶� ������)
    public void PlayerCaught(Transform attacker)
    {
        if (isGameOver) return;

        isGameOver = true;
        npcAttacker = attacker;

        Debug.Log("�÷��̾� �ƿ�!");

        // �÷��̾� ������ ����
        FreezePlayer();

        // ī�޶� ����
        if (lockCameraOnAttack && attacker != null)
        {
            isCameraLocked = true;
        }

        // UI ǥ��
        if (gameOverText != null)
        {
            gameOverText.SetActive(true);
        }

        // 3�� �� �����
        Invoke(nameof(RestartGame), restartDelay);
    }

    // �÷��̾� ����
    void FreezePlayer()
    {
        if (player == null) return;

        // ��� MonoBehaviour ��ũ��Ʈ ��Ȱ��ȭ
        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // CharacterController ��Ȱ��ȭ
        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // Rigidbody�� ������ ����
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Debug.Log("�÷��̾� ������ ����");
    }

    // ī�޶� NPC�� ����
    void LockCameraOnNPC()
    {
        // NPC ���� �ణ ������ ���� ����� ������
        Vector3 targetPosition = npcAttacker.position + npcAttacker.forward * cameraOffset.z + Vector3.up * cameraOffset.y;
        
        playerCamera.transform.position = Vector3.Lerp(
            playerCamera.transform.position, 
            targetPosition, 
            Time.deltaTime * 5f
        );

        // NPC�� ���ϵ���
        Vector3 lookDirection = (npcAttacker.position + Vector3.up * 1.2f) - playerCamera.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        
        playerCamera.transform.rotation = Quaternion.Slerp(
            playerCamera.transform.rotation,
            targetRotation,
            Time.deltaTime * 5f
        );
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
