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
            if (scaleOverTime)
            {
                float t = (Time.time - _spawnTime) / lifetime;
                float scale = scaleCurve.Evaluate(t);
                transform.localScale = _initialScale * scale;
            }
        }
    }
}
