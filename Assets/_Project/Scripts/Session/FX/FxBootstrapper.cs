// Garantiza que existan singletons de HitStop, CameraShake, ComboManager, PanBombaHUDBadge.
// Se autoejecuta despues de la carga de escena, sin tocar prefabs ni UI existentes.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kikiri9.Session.FX
{
    public static class FxBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Install();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Install();

        private static void Install()
        {
            // Solo niveles (Level01/02/03). Boot/MainMenu no necesitan FX.
            var activeName = SceneManager.GetActiveScene().name;
            if (!activeName.StartsWith("Level")) return;

            if (HitStop.Instance == null)
            {
                var go = new GameObject("HitStop");
                go.AddComponent<HitStop>();
                Object.DontDestroyOnLoad(go);
            }
            if (ComboManager.Instance == null)
            {
                var go = new GameObject("ComboManager");
                go.AddComponent<AudioSource>();
                go.AddComponent<ComboManager>();
            }
            if (CameraShake.Instance == null && Camera.main != null)
            {
                Camera.main.gameObject.AddComponent<CameraShake>();
            }

            // Pan bomba HUD badge: solo si el pollo tiene PanBombaAmmo (si no, no estorba)
            var existingBadge = Object.FindFirstObjectByType<Kikiri9.UI.PanBombaHUDBadge>();
            if (existingBadge == null)
            {
                var go = new GameObject("PanBombaHUDBadge");
                go.AddComponent<Kikiri9.UI.PanBombaHUDBadge>();
            }
            var existingBarredor = Object.FindFirstObjectByType<Kikiri9.UI.PanBarredorHUDBadge>();
            if (existingBarredor == null)
            {
                var go = new GameObject("PanBarredorHUDBadge");
                go.AddComponent<Kikiri9.UI.PanBarredorHUDBadge>();
            }

            // Clamp del pollo a la pantalla durante oleadas free-movement
            var existingClamp = Object.FindFirstObjectByType<Kikiri9.World.ArenaScreenClamp>();
            if (existingClamp == null)
            {
                var go = new GameObject("ArenaScreenClamp");
                go.AddComponent<Kikiri9.World.ArenaScreenClamp>();
            }

            // Asignar el clip de impacto para los Defeat de enemigos (si esta en Resources no se
            // carga aqui; se asigna desde el Editor via Etapa3Installer o por un AudioLoader).
            var audioLoader = Object.FindFirstObjectByType<AudioClipLoader>();
            if (audioLoader == null)
            {
                var go = new GameObject("AudioClipLoader");
                go.AddComponent<AudioClipLoader>();
            }
        }
    }
}
