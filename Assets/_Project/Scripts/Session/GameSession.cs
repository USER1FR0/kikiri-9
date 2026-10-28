using UnityEngine;

namespace Kikiri9.Session
{
    // Singleton que vive durante toda la partida (no se destruye al cambiar de escena).
    // Guarda que nivel se esta jugando y el puntaje actual, para que el menu y los
    // niveles puedan leerlos y escribirlos sin depender de PlayerPrefs ni de una escena en concreto.
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        [SerializeField] private string defaultLevelSceneName = "Sandbox";

        public string CurrentLevelSceneName { get; private set; }
        public int Score { get; private set; }
        public bool HasSavedProgress { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (string.IsNullOrEmpty(CurrentLevelSceneName))
            {
                CurrentLevelSceneName = defaultLevelSceneName;
            }
        }

        public void SetCurrentLevel(string sceneName)
        {
            CurrentLevelSceneName = sceneName;
            HasSavedProgress = true;
        }

        public void SetScore(int newScore)
        {
            Score = newScore;
        }

        public void AddScore(int amount)
        {
            Score += amount;
        }

        public void ResetSession()
        {
            Score = 0;
            HasSavedProgress = false;
            CurrentLevelSceneName = defaultLevelSceneName;
        }
    }
}
