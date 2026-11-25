// Button helper: toggles pause via GameStateManager. Auto-wires its handler to the
// Button component at Awake, so it works regardless of prefab serialization quirks.

using Kikiri9.Session;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    [RequireComponent(typeof(Button))]
    public class PauseButtonHelper : MonoBehaviour
    {
        private void Awake()
        {
            var btn = GetComponent<Button>();
            btn.onClick.RemoveListener(TogglePause);
            btn.onClick.AddListener(TogglePause);
        }

        public void TogglePause() => GameStateManager.Instance?.TogglePause();
    }
}
