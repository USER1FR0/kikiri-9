using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kikiri9.UI
{
    // Controla un CanvasGroup negro que cubre toda la pantalla, usado para hacer
    // fundidos (fade in / fade out) entre escenas. Vive en DontDestroyOnLoad para
    // poder reusar el mismo overlay en Boot, MainMenu y los niveles.
    public class SceneFader : MonoBehaviour
    {
        public static SceneFader Instance { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.4f;

        private Coroutine activeFade;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            FadeIn();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Cada vez que termina de cargar una escena nueva (incluida la que
            // disparo LoadSceneWithFade), volvemos a mostrar la pantalla.
            FadeIn();
        }

        public void FadeIn()
        {
            StartFade(1f, 0f, null);
        }

        public void FadeOut(Action onComplete)
        {
            StartFade(0f, 1f, onComplete);
        }

        public void LoadSceneWithFade(string sceneName)
        {
            FadeOut(() => SceneManager.LoadScene(sceneName));
        }

        private void StartFade(float from, float to, Action onComplete)
        {
            if (canvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (activeFade != null)
            {
                StopCoroutine(activeFade);
            }

            activeFade = StartCoroutine(FadeRoutine(from, to, onComplete));
        }

        private IEnumerator FadeRoutine(float from, float to, Action onComplete)
        {
            canvasGroup.blocksRaycasts = true;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            canvasGroup.alpha = to;
            canvasGroup.blocksRaycasts = to > 0.5f;
            activeFade = null;
            onComplete?.Invoke();
        }
    }
}
