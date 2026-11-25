// Shown when GameState == LevelComplete. Displays score, run time and narrative. Next
// loads the next level scene by name convention Level02_* if present, else back to menu.

using Kikiri9.Levels;
using Kikiri9.Session;
using Kikiri9.UI.Story;
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

            var lc = FindFirstObjectByType<LevelController>();

            if (scoreText != null && ScoreTracker.Instance != null)
                scoreText.text = $"Puntaje: {ScoreTracker.Instance.CurrentScore}";
            if (timeText != null)
            {
                float t = Time.time - startTime;
                int mm = Mathf.FloorToInt(t / 60f);
                int ss = Mathf.FloorToInt(t % 60f);
                timeText.text = $"Tiempo: {mm:00}:{ss:00}";
            }
            if (narrativeText != null && lc != null && lc.Definition != null)
                narrativeText.text = lc.Definition.endingNarrative;

            // Persistir progreso real en disco (#15): desbloquea el siguiente nivel,
            // guarda el mejor puntaje y otorga la botella dorada del nivel solo la
            // primera vez que se gana. Esto sobrevive a cerrar el juego, a diferencia
            // de GameSession (que solo vive durante la sesion actual).
            if (ScoreTracker.Instance != null)
            {
                int levelNumber = SaveService.LevelNumberForScene(SceneManager.GetActiveScene().name);
                if (levelNumber > 0)
                {
                    int goldReward = (lc != null && lc.Definition != null) ? lc.Definition.goldBottlesReward : 0;
                    SaveService.ReportLevelCompleted(levelNumber, ScoreTracker.Instance.CurrentScore, goldReward);
                }
            }

            // GameSession se mantiene por compatibilidad dentro de la sesion en curso
            // (otras pantallas pueden seguir leyendo Score/CurrentLevelSceneName).
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

            int currentLevel = SaveService.LevelNumberForScene(SceneManager.GetActiveScene().name);

            // Calculamos el destino REAL via SaveService. El nextSceneName serializado
            // puede estar desactualizado o vacio en algunos prefabs; SaveService es la
            // fuente de verdad del mapeo nivel -> escena.
            string computedNext = null;
            if (currentLevel > 0 && currentLevel < SaveService.LevelCount)
                computedNext = SaveService.SceneNameForLevel(currentLevel + 1);
            string target = !string.IsNullOrEmpty(computedNext) ? computedNext : nextSceneName;

            // Ganar el ultimo nivel: epilogo + creditos
            if (currentLevel >= SaveService.LevelCount && currentLevel > 0)
            {
                var flow = StoryFlow.Get();
                string creditsTarget = flow != null ? flow.creditsSceneName : "CreditsScene";
                if (StoryQueue.TryEnqueueEnding(creditsTarget))
                {
                    StoryQueue.LoadStoryScene();
                    return;
                }
            }
            else if (currentLevel > 0)
            {
                int nextLevel = currentLevel + 1;
                if (StoryQueue.TryEnqueueBeforeLevel(nextLevel, target))
                {
                    StoryQueue.LoadStoryScene();
                    return;
                }
            }

            LoadWithFade(target, false);
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
