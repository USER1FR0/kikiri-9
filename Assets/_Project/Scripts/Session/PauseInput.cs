// Listens for ESC/P or Gamepad Start and toggles pause via GameStateManager.
// Attached to the Session root alongside GameStateManager.

using UnityEngine;
using UnityEngine.InputSystem;

namespace Kikiri9.Session
{
    public class PauseInput : MonoBehaviour
    {
        private InputAction pauseAction;

        private void OnEnable()
        {
            pauseAction = new InputAction("Pause", InputActionType.Button);
            pauseAction.AddBinding("<Keyboard>/escape");
            pauseAction.AddBinding("<Keyboard>/p");
            pauseAction.AddBinding("<Gamepad>/start");
            pauseAction.performed += OnPausePressed;
            pauseAction.Enable();
        }

        private void OnDisable()
        {
            if (pauseAction == null) return;
            pauseAction.performed -= OnPausePressed;
            pauseAction.Disable();
            pauseAction.Dispose();
            pauseAction = null;
        }

        private void OnPausePressed(InputAction.CallbackContext _)
        {
            var gsm = GameStateManager.Instance;
            if (gsm == null) return;
            // Solo togglear durante Playing/Paused (no durante Tutorial/LevelComplete/GameOver)
            if (gsm.CurrentState == GameState.Playing || gsm.CurrentState == GameState.Paused)
                gsm.TogglePause();
        }
    }
}
