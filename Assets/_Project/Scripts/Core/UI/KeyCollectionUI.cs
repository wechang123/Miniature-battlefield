using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using YajaGame.Gameplay;

namespace YajaGame.UI
{
    /// <summary>
    /// 키 수집 상태를 표시하는 UI 시스템
    /// 수집된 키와 남은 키 슬롯을 시각적으로 표시
    /// </summary>
    public class KeyCollectionUI : MonoBehaviour
    {
        [Header("Key UI References")]
        [SerializeField] private Transform keySlotContainer; // 키 슬롯들의 부모 오브젝트
        [SerializeField] private GameObject keySlotPrefab; // 키 슬롯 프리팹
        [SerializeField] private TextMeshProUGUI keyCountText; // "키: 2/5" 형태의 텍스트
        
        [Header("Key Images")]
        [SerializeField] private Sprite[] keySprites; // key-1.png ~ key-5.png
        [SerializeField] private Sprite emptyKeySlotSprite; // key-빈 슬롯.png
        [SerializeField] private Sprite largeKeySprite; // large-key.png (선택사항)
        
        [Header("UI Layout")]
        [SerializeField] private float slotSpacing = 10f; // 슬롯 간 간격
        [SerializeField] private Vector2 slotSize = new Vector2(60f, 60f); // 슬롯 크기
        
        [Header("Animation")]
        [SerializeField] private bool animateKeyCollection = true;
        [SerializeField] private float collectAnimationDuration = 0.5f;
        [SerializeField] private AnimationCurve collectAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Audio")]
        [SerializeField] private AudioClip keyUIUpdateSound;
        
        // 내부 변수
        private List<Image> keySlotImages = new List<Image>();
        private Stage2KeyManager keyManager;
        private int totalKeys = 5;
        private int collectedKeys = 0;
        
        private void Start()
        {
            InitializeKeyUI();
        }
        
        /// <summary>
        /// 키 UI 시스템 초기화
        /// </summary>
        private void InitializeKeyUI()
        {
            // Stage2KeyManager 찾기
            keyManager = FindObjectOfType<Stage2KeyManager>();
            if (keyManager != null)
            {
                totalKeys = keyManager.TotalKeys;
                collectedKeys = keyManager.KeysCollected;
                
                Debug.Log($"[KeyCollectionUI] Stage2KeyManager 연결 성공. 총 키: {totalKeys}");
            }
            else
            {
                Debug.LogWarning("[KeyCollectionUI] Stage2KeyManager를 찾을 수 없습니다. 기본값 사용.");
                totalKeys = 5; // 기본값
            }
            
            // 키 슬롯 생성
            CreateKeySlots();
            
            // 초기 UI 업데이트
            UpdateKeyUI();
        }
        
        /// <summary>
        /// 키 슬롯들 생성
        /// </summary>
        private void CreateKeySlots()
        {
            if (keySlotContainer == null)
            {
                Debug.LogError("[KeyCollectionUI] keySlotContainer가 설정되지 않았습니다!");
                return;
            }
            
            // 기존 슬롯들 제거
            foreach (Transform child in keySlotContainer)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
            keySlotImages.Clear();
            
            // 새 슬롯들 생성
            for (int i = 0; i < totalKeys; i++)
            {
                GameObject slotObj = CreateKeySlot(i);
                if (slotObj != null)
                {
                    Image slotImage = slotObj.GetComponent<Image>();
                    if (slotImage != null)
                    {
                        keySlotImages.Add(slotImage);
                    }
                }
            }
            
            Debug.Log($"[KeyCollectionUI] {keySlotImages.Count}개의 키 슬롯 생성 완료");
        }
        
        /// <summary>
        /// 개별 키 슬롯 생성
        /// </summary>
        private GameObject CreateKeySlot(int index)
        {
            GameObject slotObj;
            
            // 프리팹이 있으면 프리팹 사용, 없으면 기본 GameObject 생성
            if (keySlotPrefab != null)
            {
                slotObj = Instantiate(keySlotPrefab, keySlotContainer);
            }
            else
            {
                slotObj = new GameObject($"KeySlot_{index}");
                slotObj.transform.SetParent(keySlotContainer);
                
                // Image 컴포넌트 추가
                Image slotImage = slotObj.AddComponent<Image>();
                slotImage.sprite = emptyKeySlotSprite;
                slotImage.preserveAspect = true;
                
                // RectTransform 설정
                RectTransform rectTransform = slotObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = slotSize;
            }
            
            // 위치 설정
            RectTransform slotRect = slotObj.GetComponent<RectTransform>();
            slotRect.anchoredPosition = new Vector2(index * (slotSize.x + slotSpacing), 0);
            slotRect.localScale = Vector3.one;
            
            return slotObj;
        }
        
        /// <summary>
        /// 키 수집 시 호출되는 메서드 (Stage2KeyManager에서 호출)
        /// </summary>
        public void OnKeyCollected(int newCollectedCount)
        {
            int previousCount = collectedKeys;
            collectedKeys = newCollectedCount;
            
            // UI 업데이트
            UpdateKeyUI();
            
            // 새로 수집된 키에 애니메이션 적용
            if (animateKeyCollection && newCollectedCount > previousCount)
            {
                int newKeyIndex = newCollectedCount - 1; // 0-based index
                if (newKeyIndex >= 0 && newKeyIndex < keySlotImages.Count)
                {
                    StartCoroutine(AnimateKeyCollection(newKeyIndex));
                }
            }
            
            // 사운드 재생
            if (keyUIUpdateSound != null)
            {
                AudioSource.PlayClipAtPoint(keyUIUpdateSound, Camera.main.transform.position, 0.5f);
            }
            
            Debug.Log($"[KeyCollectionUI] 키 수집 UI 업데이트: {collectedKeys}/{totalKeys}");
        }
        
        /// <summary>
        /// 키 UI 업데이트
        /// </summary>
        private void UpdateKeyUI()
        {
            // 키 카운트 텍스트 업데이트
            if (keyCountText != null)
            {
                keyCountText.text = $"키: {collectedKeys}/{totalKeys}";
            }
            
            // 키 슬롯 이미지 업데이트
            for (int i = 0; i < keySlotImages.Count; i++)
            {
                if (keySlotImages[i] != null)
                {
                    if (i < collectedKeys)
                    {
                        // 수집된 키 - 키 이미지 표시
                        keySlotImages[i].sprite = GetKeySprite(i);
                        keySlotImages[i].color = Color.white;
                    }
                    else
                    {
                        // 수집되지 않은 키 - 빈 슬롯 표시
                        keySlotImages[i].sprite = emptyKeySlotSprite;
                        keySlotImages[i].color = new Color(1f, 1f, 1f, 0.5f); // 반투명
                    }
                }
            }
        }
        
        /// <summary>
        /// 키 인덱스에 맞는 키 스프라이트 반환
        /// </summary>
        private Sprite GetKeySprite(int keyIndex)
        {
            if (keySprites != null && keyIndex >= 0 && keyIndex < keySprites.Length)
            {
                return keySprites[keyIndex];
            }
            
            // 기본 키 스프라이트 (첫 번째 키 이미지 또는 큰 키 이미지)
            if (keySprites != null && keySprites.Length > 0)
            {
                return keySprites[0];
            }
            
            if (largeKeySprite != null)
            {
                return largeKeySprite;
            }
            
            return emptyKeySlotSprite; // 최후의 수단
        }
        
        /// <summary>
        /// 키 수집 애니메이션
        /// </summary>
        private System.Collections.IEnumerator AnimateKeyCollection(int keyIndex)
        {
            if (keyIndex < 0 || keyIndex >= keySlotImages.Count) yield break;
            
            Image keyImage = keySlotImages[keyIndex];
            if (keyImage == null) yield break;
            
            Vector3 originalScale = keyImage.transform.localScale;
            float elapsedTime = 0f;
            
            while (elapsedTime < collectAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / collectAnimationDuration;
                float animValue = collectAnimationCurve.Evaluate(progress);
                
                // 스케일 애니메이션 (펄스 효과)
                float scale = 1f + (animValue * 0.3f);
                keyImage.transform.localScale = originalScale * scale;
                
                yield return null;
            }
            
            // 원래 크기로 복원
            keyImage.transform.localScale = originalScale;
        }
        
        /// <summary>
        /// Stage2KeyManager와 연동 설정 (Inspector에서 수동 호출 가능)
        /// </summary>
        [ContextMenu("Link with Stage2KeyManager")]
        public void LinkWithKeyManager()
        {
            keyManager = FindObjectOfType<Stage2KeyManager>();
            if (keyManager != null)
            {
                totalKeys = keyManager.TotalKeys;
                collectedKeys = keyManager.KeysCollected;
                
                // UI 재생성
                CreateKeySlots();
                UpdateKeyUI();
                
                Debug.Log("[KeyCollectionUI] Stage2KeyManager와 연동 완료");
            }
            else
            {
                Debug.LogError("[KeyCollectionUI] Stage2KeyManager를 찾을 수 없습니다!");
            }
        }
        
        /// <summary>
        /// 디버그용 - 키 수집 시뮬레이션
        /// </summary>
        [ContextMenu("Debug: Collect Next Key")]
        public void DebugCollectNextKey()
        {
            if (collectedKeys < totalKeys)
            {
                OnKeyCollected(collectedKeys + 1);
            }
        }
        
        /// <summary>
        /// 디버그용 - 모든 키 수집
        /// </summary>
        [ContextMenu("Debug: Collect All Keys")]
        public void DebugCollectAllKeys()
        {
            OnKeyCollected(totalKeys);
        }
        
        /// <summary>
        /// 디버그용 - UI 리셋
        /// </summary>
        [ContextMenu("Debug: Reset Keys")]
        public void DebugResetKeys()
        {
            OnKeyCollected(0);
        }
        
        private void OnValidate()
        {
            // Inspector에서 값 변경 시 실시간 업데이트 (에디터에서만)
            if (!Application.isPlaying) return;
            
            if (keySlotContainer != null && keySlotImages.Count != totalKeys)
            {
                CreateKeySlots();
            }
            
            UpdateKeyUI();
        }
    }
}