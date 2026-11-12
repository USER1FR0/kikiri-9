// Mutes the attached AudioSource while the game is not actively playing. Reacts to
// GameStateManager state changes: music plays only on GameState.Playing.

using System.Collections;
using UnityEngine;

namespace Kikiri9.Session
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicController : MonoBehaviour
    {
        private AudioSource source;
        private bool subscribed;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        private void Start()
        {
            // Usa el musicClip del LevelDefinition activo si existe, sin tocar el AudioSource
            // cuando ya tiene el mismo clip asignado.
            var lc = FindFirstObjectByType<Kikiri9.Levels.LevelController>();
            if (lc != null && lc.Definition != null && lc.Definition.musicClip != null)
            {
                if (source.clip != lc.Definition.musicClip)
                {
                    source.clip = lc.Definition.musicClip;
                }
                source.loop = true;
            }
            StartCoroutine(SubscribeWhenReady());
        }

        private IEnumerator SubscribeWhenReady()
        {
            while (GameStateManager.Instance == null) yield return null;
            GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
            GameStateManager.Instance.OnStateChanged += HandleStateChanged;
            subscribed = true;
            HandleStateChanged(GameStateManager.Instance.CurrentState);
        }

        private void OnDestroy()
        {
            if (subscribed && GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState s)
        {
            if (source == null) return;
            if (s == GameState.Playing)
            {
                if (!source.isPlaying) source.UnPause();
                if (!source.isPlaying) source.Play();
            }
            else
            {
                source.Pause();
            }
        }
    }
}
