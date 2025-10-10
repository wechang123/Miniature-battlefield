using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// 줍기 가능한 오브젝트 컴포넌트
    /// 연필, 지우개 등 줍고 던질 수 있는 아이템에 부착
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PickableObject : MonoBehaviour
    {
        [Header("Item Properties")]
        [Tooltip("아이템 이름")]
        public string itemName = "Item";

        [Tooltip("아이템 무게 (kg)")]
        public float weight = 0.5f;

        [Tooltip("줍기 가능 여부")]
        public bool canBePickedUp = true;

        [Header("Throw Properties")]
        [Tooltip("최대 던지기 힘")]
        public float maxThrowForce = 15f;

        [Tooltip("던지기 시 회전력")]
        public float throwTorque = 5f;

        [Header("Visual Feedback")]
        [Tooltip("줍기 가능할 때 표시할 색상")]
        public Color highlightColor = new Color(1f, 1f, 0f, 0.3f);

        private Rigidbody _rigidbody;
        private Collider _collider;
        private Renderer _renderer;
        private Material _originalMaterial;
        private Material _highlightMaterial;

        // 상태
        private bool _isBeingHeld = false;
        private bool _isHighlighted = false;

        public bool IsBeingHeld => _isBeingHeld;
        public Rigidbody Rigidbody => _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _renderer = GetComponentInChildren<Renderer>();

            if (_renderer != null)
            {
                _originalMaterial = _renderer.material;
            }
        }

        /// <summary>
        /// 아이템을 집었을 때 호출
        /// </summary>
        public void OnPickup()
        {
            _isBeingHeld = true;

            // 물리 비활성화
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;

            // 충돌 비활성화 (플레이어와 충돌 방지)
            if (_collider != null)
            {
                _collider.isTrigger = true;
            }

            // 하이라이트 제거
            RemoveHighlight();
        }

        /// <summary>
        /// 아이템을 던졌을 때 호출
        /// </summary>
        /// <param name="throwForce">던지는 힘</param>
        /// <param name="throwDirection">던지는 방향</param>
        public void OnThrow(float throwForce, Vector3 throwDirection)
        {
            _isBeingHeld = false;

            // 물리 활성화
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;

            // 충돌 활성화
            if (_collider != null)
            {
                _collider.isTrigger = false;
            }

            // 힘 적용
            _rigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);

            // 회전력 추가 (더 자연스러운 던지기)
            Vector3 randomTorque = new Vector3(
                Random.Range(-throwTorque, throwTorque),
                Random.Range(-throwTorque, throwTorque),
                Random.Range(-throwTorque, throwTorque)
            );
            _rigidbody.AddTorque(randomTorque, ForceMode.Impulse);
        }

        /// <summary>
        /// 아이템을 내려놓았을 때 호출
        /// </summary>
        public void OnDrop()
        {
            _isBeingHeld = false;

            // 물리 활성화
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;

            // 충돌 활성화
            if (_collider != null)
            {
                _collider.isTrigger = false;
            }

            RemoveHighlight();
        }

        /// <summary>
        /// 줍기 가능 표시 (하이라이트)
        /// </summary>
        public void Highlight()
        {
            if (_isBeingHeld || _isHighlighted || _renderer == null) return;

            _isHighlighted = true;

            // 간단한 하이라이트 효과 (색상 변경)
            if (_highlightMaterial == null)
            {
                _highlightMaterial = new Material(_originalMaterial);
                _highlightMaterial.color = highlightColor;
            }

            _renderer.material = _highlightMaterial;
        }

        /// <summary>
        /// 하이라이트 제거
        /// </summary>
        public void RemoveHighlight()
        {
            if (!_isHighlighted || _renderer == null) return;

            _isHighlighted = false;
            _renderer.material = _originalMaterial;
        }

        private void OnDrawGizmosSelected()
        {
            // 에디터에서 줍기 가능 오브젝트 시각화
            Gizmos.color = canBePickedUp ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}
