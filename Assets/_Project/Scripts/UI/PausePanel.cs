// Pause panel. Shown while GameState == Paused. Resume unpauses, Restart reloads scene,
// Menu loads MainMenu scene (if present in build settings). Options is a placeholder.

using Kikiri9.Session;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kikiri9.UI
{
    public class PausePanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private string menuSceneName = "MainMenu";

        private bool subscribed;

        private void Start()
        {
            if (root != null) root.SetActive(false);
            StartCoroutine(SubscribeWhenReady());
        }

        private System.Collections.IEnumerator SubscribeWhenReady()
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

        private void HandleStateChanged(GameState s) { if (root != null) root.SetActive(s == GameState.Paused); }

        public void OnResumeClicked() => GameStateManager.Instance?.SetState(GameState.Playing);
        public void OnRestartClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        public void OnOptionsClicked() { /* placeholder: ampliar en avance 3 */ }
        public void OnMenuClicked()
        {
            Time.timeScale = 1f;
            if (Application.CanStreamedLevelBeLoaded(menuSceneName))
                SceneManager.LoadScene(menuSceneName);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
