// Shown when GameState == LevelComplete. Displays score, run time and narrative. Next
// loads the next level scene by name convention Level02_* if present, else back to menu.

using Kikiri9.Levels;
using Kikiri9.Session;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Al completar nivel: persistir score en GameSession y marcar progreso para 'Continuar'.

namespace Kikiri9.UI
{
    public class LevelCompletePanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text narrativeText;
        [SerializeField] private string nextSceneName = "Level02_Dolores";
        [SerializeField] private string menuSceneName = "MainMenu";

        private float startTime;

        private bool subscribed;

        private void Awake()
        {
            startTime = Time.time;
        }

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
            if (s != GameState.LevelComplete) { root.SetActive(false); return; }
            root.SetActive(true);
            if (scoreText != null && ScoreTracker.Instance != null)
                scoreText.text = $"Puntaje: {ScoreTracker.Instance.CurrentScore}";
            if (timeText != null)
            {
                float t = Time.time - startTime;
                int mm = Mathf.FloorToInt(t / 60f);
                int ss = Mathf.FloorToInt(t % 60f);
                timeText.text = $"Tiempo: {mm:00}:{ss:00}";
            }
            if (narrativeText != null)
            {
                var lc = FindFirstObjectByType<LevelController>();
                if (lc != null && lc.Definition != null) narrativeText.text = lc.Definition.endingNarrative;
            }

            // Persistir estado para 'Continuar' en el MainMenu
            if (GameSession.Instance != null && ScoreTracker.Instance != null)
            {
                GameSession.Instance.SetScore(ScoreTracker.Instance.CurrentScore);
                GameSession.Instance.SetCurrentLevel(SceneManager.GetActiveScene().name);
            }
        }

        public void OnMenuClicked()
        {
            Time.timeScale = 1f;
            LoadWithFade(menuSceneName, true);
        }

        public void OnNextClicked()
        {
            Time.timeScale = 1f;
            LoadWithFade(nextSceneName, false);
        }

        private void LoadWithFade(string sceneName, bool fallbackToMenu)
        {
            if (SceneFader.Instance != null && Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneFader.Instance.LoadSceneWithFade(sceneName);
                return;
            }
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName);
            else if (fallbackToMenu && Application.CanStreamedLevelBeLoaded(menuSceneName))
                SceneManager.LoadScene(menuSceneName);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
