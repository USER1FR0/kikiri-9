// Dispara LevelIntroBanner cuando el estado pasa a Playing (al cerrar tutorial).
// Tambien instala el InfiniteBreadBadge con el sprite "pan" serializado desde Editor.

using Kikiri9.Levels;
using Kikiri9.Session;
using UnityEngine;

namespace Kikiri9.UI
{
    public class LevelIntroHook : MonoBehaviour
    {
        [SerializeField] private string levelNumberLabel = "NIVEL 2";
        [SerializeField] private LevelController controller;
        [SerializeField] private Sprite breadIconSprite;

        private bool fired;

        private void Start()
        {
            if (controller == null) controller = FindFirstObjectByType<LevelController>();
            InfiniteBreadBadge.Install(breadIconSprite);
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += HandleState;
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleState;
        }

        private void HandleState(GameState s)
        {
            if (fired) return;
            if (s != GameState.Playing) return;
            fired = true;
            LevelIntroBanner.SpawnFor(levelNumberLabel, controller != null ? controller.Definition : null);
        }
    }
}
