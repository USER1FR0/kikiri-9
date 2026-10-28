// Shown at level start, pauses the world via GameStateManager until the player closes it.
// Reads tutorial text from the active LevelController's LevelDefinition.

using Kikiri9.Levels;
using Kikiri9.Session;
using TMPro;
using UnityEngine;

namespace Kikiri9.UI
{
    public class TutorialPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private bool showOnStart = true;

        private void Start()
        {
            if (root == null) return;
            if (!showOnStart) { root.SetActive(false); return; }

            var lc = FindFirstObjectByType<LevelController>();
            if (bodyText != null && lc != null && lc.Definition != null)
                bodyText.text = lc.Definition.tutorialText;

            root.SetActive(true);
            StartCoroutine(PauseWhenReady());
        }

        private System.Collections.IEnumerator PauseWhenReady()
        {
            while (GameStateManager.Instance == null) yield return null;
            GameStateManager.Instance.SetState(GameState.Tutorial);
        }

        public void OnUnderstoodClicked()
        {
            root.SetActive(false);
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.SetState(GameState.Playing);
        }
    }
}
