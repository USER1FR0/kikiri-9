// Simple sound bank attached to the pollo. Public methods are invoked by PlayerController,
// BreadLauncher, PlayerHealth and GameOverHandler. Clips are assigned via the Inspector or
// by the editor bootstrap (Level01SceneBootstrap wires them from the Draft audio folder).

using UnityEngine;

namespace Kikiri9.Player
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerSfx : MonoBehaviour
    {
        [SerializeField] private AudioClip jumpClip;
        [SerializeField] private AudioClip slideClip;
        [SerializeField] private AudioClip throwClip;
        [SerializeField] private AudioClip damageClip;
        [SerializeField] private AudioClip gameOverClip;

        [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
        }

        public void PlayJump() => Play(jumpClip);
        public void PlaySlide() => Play(slideClip);
        public void PlayThrow() => Play(throwClip);
        public void PlayDamage() => Play(damageClip);
        public void PlayGameOver() => Play(gameOverClip);

        private void Play(AudioClip clip)
        {
            if (clip == null || source == null) return;
            source.PlayOneShot(clip, volume);
        }
    }
}
