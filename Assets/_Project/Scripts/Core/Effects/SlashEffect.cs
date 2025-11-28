using UnityEngine;

namespace YajaGame.Effects
{
    /// <summary>
    /// 슬래시(베기) 이펙트
    /// 연필창 공격 시 재생
    /// </summary>
    public class SlashEffect : MonoBehaviour
    {
        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem slashParticle;
        [SerializeField] private ParticleSystem trailParticle;

        [Header("Settings")]
        [SerializeField] private float lifetime = 0.5f;
        [SerializeField] private bool autoDestroy = true;

        private void Start()
        {
            // 자동 재생
            Play();

            if (autoDestroy)
            {
                Destroy(gameObject, lifetime);
            }
        }

        public void Play()
        {
            if (slashParticle != null)
            {
                slashParticle.Play();
            }
            if (trailParticle != null)
            {
                trailParticle.Play();
            }
        }

        public void Stop()
        {
            if (slashParticle != null)
            {
                slashParticle.Stop();
            }
            if (trailParticle != null)
            {
                trailParticle.Stop();
            }
        }

        /// <summary>
        /// 슬래시 이펙트 생성 (정적 메서드)
        /// </summary>
        public static SlashEffect Create(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            GameObject instance = Instantiate(prefab, position, rotation);
            return instance.GetComponent<SlashEffect>();
        }
    }
}
