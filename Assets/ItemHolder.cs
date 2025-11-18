using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    [Header("Hand Reference")]
    public Transform rightHandTransform;
    public Transform leftHandTransform;
    
    [Header("Items")]
    public GameObject flashlightPrefab;
    
    private GameObject currentItem;
    private Transform currentHand;

    void Start()
    {
        if (rightHandTransform == null)
        {
            rightHandTransform = FindHandBone("RightHand");
        }

        // 게임 시작 시 손전등 자동 장착
        if (flashlightPrefab != null)
        {
            EquipFlashlight();
        }
    }

    public GameObject EquipItem(GameObject itemPrefab, bool useRightHand = true)
    {
        if (currentItem != null)
        {
            Destroy(currentItem);
        }

        currentHand = useRightHand ? rightHandTransform : leftHandTransform;
        
        if (currentHand == null)
        {
            Debug.LogError("�� Transform�� �������� �ʾҽ��ϴ�!");
            return null;
        }

        currentItem = Instantiate(itemPrefab);
        currentItem.transform.SetParent(currentHand, false);
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
        currentItem.transform.localScale = Vector3.one;
        
        return currentItem;
    }

    public GameObject EquipFlashlight()
    {
        GameObject flashlight = EquipItem(flashlightPrefab, true);
        
        if (flashlight != null)
        {
            // ũ�� ���� (�� �𵨿� �°�)
            flashlight.transform.localScale = Vector3.one * 0.5f;
            
            // ��ġ ���� (���� �� �õ�)
            flashlight.transform.localPosition = new Vector3(0, 0, 0.15f);
            flashlight.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            Debug.Log($"������ ���� �Ϸ�! ��ġ: {flashlight.transform.position}");
        }
        
        return flashlight;
    }

    public void UnequipItem()
    {
        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
        }
    }

    public GameObject GetCurrentItem()
    {
        return currentItem;
    }

    private Transform FindHandBone(string handName)
    {
        Animator animator = GetComponent<Animator>();
        
        if (animator != null && animator.isHuman)
        {
            if (handName == "RightHand")
                return animator.GetBoneTransform(HumanBodyBones.RightHand);
            else if (handName == "LeftHand")
                return animator.GetBoneTransform(HumanBodyBones.LeftHand);
        }
        else
        {
            return FindDeepChild(transform, handName);
        }
        
        return null;
    }

    private Transform FindDeepChild(Transform parent, string name)
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