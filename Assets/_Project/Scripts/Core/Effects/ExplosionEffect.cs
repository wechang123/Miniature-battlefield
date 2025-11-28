using UnityEngine;

namespace YajaGame.Effects
{
    /// <summary>
    /// 폭발 이펙트
    /// 지우개폭탄 폭발 시 재생
    /// </summary>
    public class ExplosionEffect : MonoBehaviour
    {
        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem smokeParticle;      // 연기
        [SerializeField] private ParticleSystem debrisParticle;     // 파편
        [SerializeField] private ParticleSystem flashParticle;      // 섬광

        [Header("Settings")]
        [SerializeField] private float lifetime = 2f;
        [SerializeField] private bool autoDestroy = true;

        [Header("Audio")]
        [SerializeField] private AudioClip explosionSound;
        [SerializeField] private float volume = 1f;

        private void Start()
        {
            Play();

            // 사운드 재생
            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, transform.position, volume);
            }

            if (autoDestroy)
            {
                Destroy(gameObject, lifetime);
            }
        }

        public void Play()
        {
            if (smokeParticle != null) smokeParticle.Play();
            if (debrisParticle != null) debrisParticle.Play();
            if (flashParticle != null) flashParticle.Play();
        }

        public void Stop()
        {
            if (smokeParticle != null) smokeParticle.Stop();
            if (debrisParticle != null) debrisParticle.Stop();
            if (flashParticle != null) flashParticle.Stop();
        }

        /// <summary>
        /// 폭발 이펙트 생성 (정적 메서드)
        /// </summary>
        public static ExplosionEffect Create(GameObject prefab, Vector3 position)
        {
            if (prefab == null) return null;

            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            return instance.GetComponent<ExplosionEffect>();
        }
    }
}
