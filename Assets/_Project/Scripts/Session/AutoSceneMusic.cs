// Instancia automaticamente una pista de musica por escena cuando la escena
// activa es MainMenu o StoryScene. Respeta GameOptions.MusicVolume. Se destruye
// con la escena. NO modifica escenas en disco ni duplica si ya hay musica.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kikiri9.Session
{
    public class AutoSceneMusic : MonoBehaviour
    {
        private AudioSource source;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryInstallFor(SceneManager.GetActiveScene().name);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryInstallFor(scene.name);
        }

        private static void TryInstallFor(string sceneName)
        {
            string clipName = null;
            if (sceneName == "MainMenu") clipName = "menu_loop";
            else if (sceneName == "StoryScene") clipName = "historia_loop";
            else if (sceneName == "CreditsScene") clipName = "historia_loop";
            if (clipName == null) return;

            // Si ya hay alguna fuente de musica tocando, no hacemos nada
            var existing = FindObjectsByType<MusicController>(FindObjectsSortMode.None);
            if (existing != null && existing.Length > 0) return;

            var clip = Resources.Load<AudioClip>("Audio/Music/" + clipName);
            if (clip == null) return;

            var go = new GameObject("AutoSceneMusic");
            var src = go.AddComponent<AudioSource>();
            // Si la escena no tiene AudioListener, lo agregamos aqui para que la musica se oiga
            if (FindFirstObjectByType<AudioListener>() == null)
                go.AddComponent<AudioListener>();
            src.clip = clip;
            src.loop = true;
            src.playOnAwake = false;
            src.volume = GameOptions.MusicVolume;
            src.Play();

            var comp = go.AddComponent<AutoSceneMusic>();
            comp.source = src;

            GameOptions.OnMusicVolumeChanged += comp.ApplyVolume;
        }

        private void ApplyVolume()
        {
            if (source != null) source.volume = GameOptions.MusicVolume;
        }

        private void OnDestroy()
        {
            GameOptions.OnMusicVolumeChanged -= ApplyVolume;
        }
    }
}
