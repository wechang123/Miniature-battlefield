using UnityEngine;

namespace YajaGame.Effects
{
    /// <summary>
    /// 타격 이펙트 - 자동 파괴
    /// </summary>
    public class HitEffect : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private bool scaleOverTime = true;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        private float _spawnTime;
        private Vector3 _initialScale;

        private void Start()
        {
            _spawnTime = Time.time;
            _initialScale = transform.localScale;

            // 자동 파괴
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            // 스케일 애니메이션이 활성화된 경우에만 처리 (성능 최적화)
            if (scaleOverTime && gameObject.activeInHierarchy)
            {
                float t = (Time.time - _spawnTime) / lifetime;
                if (t <= 1f) // 애니메이션이 끝나지 않은 경우에만 계산
                {
                    float scale = scaleCurve.Evaluate(t);
                    transform.localScale = _initialScale * scale;
                }
            }
        }
    }
}
