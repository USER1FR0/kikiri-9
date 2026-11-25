// Shown when GameState == GameOver. Retry reloads scene, Menu goes back to main menu.
// Also fills Distance and Score fields from the running game systems.

using Kikiri9.Levels;
using Kikiri9.Session;
using Kikiri9.World;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kikiri9.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text distanceText;
        [SerializeField] private TMP_Text scoreText;
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

        private void HandleStateChanged(GameState s)
        {
            if (root == null) return;
            bool show = s == GameState.GameOver;
            root.SetActive(show);
            if (!show) return;

            if (distanceText != null)
            {
                var lc = FindFirstObjectByType<LevelController>();
                float m = WorldScroller.Instance != null ? WorldScroller.Instance.MetersAdvanced : 0f;
                float t = lc != null && lc.Definition != null ? lc.Definition.targetDistance : 0f;
                distanceText.text = t > 0f
                    ? $"{Mathf.FloorToInt(m)} / {Mathf.FloorToInt(t)} m"
                    : $"{Mathf.FloorToInt(m)} m";
            }
            if (scoreText != null)
                scoreText.text = (ScoreTracker.Instance != null ? ScoreTracker.Instance.CurrentScore : 0).ToString("D5");
        }

        public void OnRetryClicked()
        {
            Time.timeScale = 1f;
            string current = SceneManager.GetActiveScene().name;
            if (SceneFader.Instance != null) SceneFader.Instance.LoadSceneWithFade(current);
            else SceneManager.LoadScene(current);
        }

        public void OnMenuClicked()
        {
            Time.timeScale = 1f;
            if (SceneFader.Instance != null && Application.CanStreamedLevelBeLoaded(menuSceneName))
                SceneFader.Instance.LoadSceneWithFade(menuSceneName);
            else if (Application.CanStreamedLevelBeLoaded(menuSceneName))
                SceneManager.LoadScene(menuSceneName);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
