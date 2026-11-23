// Mutes the attached AudioSource while the game is not actively playing. Reacts to
// GameStateManager state changes: music plays only on GameState.Playing. Applies
// GameOptions.MusicVolume as the AudioSource volume and stays in sync if the player
// cambia el slider de musica en Opciones mientras la cancion sigue sonando.

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
            ApplyVolume();
            GameOptions.OnMusicVolumeChanged += ApplyVolume;
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
            GameOptions.OnMusicVolumeChanged -= ApplyVolume;
        }

        private void ApplyVolume()
        {
            if (source != null) source.volume = GameOptions.MusicVolume;
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
