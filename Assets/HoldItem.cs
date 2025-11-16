using UnityEngine;

public class HoldItem : MonoBehaviour
{
    [Header("Item Settings")]
    public GameObject itemPrefab;           // 들고 있을 아이템 (손전등 등)
    public Transform rightHandTransform;    // 오른손 본
    
    [Header("Item Transform")]
    public Vector3 itemPosition = new Vector3(0.05f, -0.05f, 0.1f);
    public Vector3 itemRotation = new Vector3(0, 90, 0);
    public Vector3 itemScale = new Vector3(0.25f, 0.25f, 0.25f);
    
    private GameObject heldItem;

    void Start()
    {
        // 손 본 자동 찾기
        if (rightHandTransform == null)
        {
            rightHandTransform = FindRightHand();
        }
        
        // 아이템 생성 및 손에 부착
        if (itemPrefab != null && rightHandTransform != null)
        {
            AttachItem();
        }
        else
        {
            Debug.LogError("Item Prefab 또는 Right Hand Transform이 설정되지 않았습니다!");
        }
    }

    void AttachItem()
    {
        // 아이템 생성
        heldItem = Instantiate(itemPrefab, rightHandTransform);
        
        // 위치, 회전, 크기 설정
        heldItem.transform.localPosition = itemPosition;
        heldItem.transform.localRotation = Quaternion.Euler(itemRotation);
        heldItem.transform.localScale = itemScale;
        
        Debug.Log($"{itemPrefab.name}을(를) {rightHandTransform.name}에 부착했습니다.");
    }

    // 오른손 본 자동 찾기
    Transform FindRightHand()
    {
        Animator animator = GetComponent<Animator>();
        
        if (animator != null && animator.isHuman)
        {
            return animator.GetBoneTransform(HumanBodyBones.RightHand);
        }
        else
        {
            // Generic Rig일 경우 이름으로 찾기
            return FindDeepChild(transform, "RightHand");
        }
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(name))
                return child;
            
            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}