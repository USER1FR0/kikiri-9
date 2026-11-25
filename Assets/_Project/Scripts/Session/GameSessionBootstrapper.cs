// Si la escena de nivel se abre directamente (en editor, sin pasar por Boot/MainMenu),
// GameSession.Instance seria null. Este helper crea un GameSession en runtime para que
// los paneles (LevelComplete, GameOver, Pause) puedan leerlo y escribirlo sin NREs.

using UnityEngine;

namespace Kikiri9.Session
{
    public class GameSessionBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            if (GameSession.Instance != null) return;
            var go = new GameObject("GameSession");
            go.AddComponent<GameSession>();
        }
    }
}
