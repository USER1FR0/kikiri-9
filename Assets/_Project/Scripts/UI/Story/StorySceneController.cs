// Muestra pantallas narrativas con typewriter. Cuando una pantalla tiene varias
// paginas, las recorre con botones "<-" y "->" mostrando CONTINUAR solo en la
// ultima pagina. Al vaciar la cola carga StoryQueue.NextSceneName.

using System.Collections;
using System.Collections.Generic;
using Kikiri9.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kikiri9.UI.Story
{
    public class StorySceneController : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup backgroundFader;
        [SerializeField] private CanvasGroup textFader;
        [SerializeField] private TMP_Text narrativeText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private RectTransform effectsLayer;

        [Header("Typewriter")]
        [SerializeField] private float charsPerSecond = 42f;
        [SerializeField] private float punctuationPause = 0.22f;

        private readonly List<GameObject> spawnedEffects = new List<GameObject>();
        private Coroutine typingRoutine;
        private bool isTyping;
        private bool canAdvance;
        private string[] currentPages;
        private int currentPageIndex;

        private void Start()
        {
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
            if (nextPageButton != null) nextPageButton.onClick.AddListener(OnNextPageClicked);
            if (prevPageButton != null) prevPageButton.onClick.AddListener(OnPrevPageClicked);
            StartCoroutine(ShowNextScreen());
        }

        private IEnumerator ShowNextScreen()
        {
            if (StoryQueue.Pending.Count == 0) { LoadNextScene(); yield break; }

            var screen = StoryQueue.Pending.Dequeue();
            if (screen == null) { StartCoroutine(ShowNextScreen()); yield break; }

            canAdvance = false;
            SetButtonsInteractable(false);

            foreach (var go in spawnedEffects) if (go != null) Destroy(go);
            spawnedEffects.Clear();

            if (backgroundImage != null && screen.background != null)
                backgroundImage.sprite = screen.background;

            currentPages = screen.GetPages();
            currentPageIndex = 0;

            if (backgroundFader != null)
            {
                backgroundFader.alpha = 0f;
                float t = 0f;
                float dur = Mathf.Max(0.05f, screen.fadeInSeconds);
                while (t < dur) { t += Time.unscaledDeltaTime; backgroundFader.alpha = Mathf.Clamp01(t / dur); yield return null; }
                backgroundFader.alpha = 1f;
            }

            if (textFader != null) textFader.alpha = 1f;

            if (screen.effects != null && effectsLayer != null)
            {
                foreach (var fx in screen.effects)
                {
                    var view = StoryEffectView.Spawn(effectsLayer, fx);
                    if (view != null) spawnedEffects.Add(view.gameObject);
                }
            }

            canAdvance = true;
            ShowCurrentPage();
        }

        private void ShowCurrentPage()
        {
            if (typingRoutine != null) StopCoroutine(typingRoutine);
            typingRoutine = StartCoroutine(Typewriter(currentPages[currentPageIndex]));
            RefreshNavButtons();
        }

        private void RefreshNavButtons()
        {
            bool multi = currentPages != null && currentPages.Length > 1;
            bool isLast = currentPageIndex >= currentPages.Length - 1;

            if (prevPageButton != null) prevPageButton.gameObject.SetActive(multi && currentPageIndex > 0);
            if (nextPageButton != null) nextPageButton.gameObject.SetActive(multi && !isLast);
            if (continueButton != null) continueButton.gameObject.SetActive(isLast);

            SetButtonsInteractable(canAdvance);
        }

        private void SetButtonsInteractable(bool on)
        {
            if (continueButton != null) continueButton.interactable = on;
            if (nextPageButton != null) nextPageButton.interactable = on;
            if (prevPageButton != null) prevPageButton.interactable = on;
        }

        private IEnumerator Typewriter(string fullText)
        {
            isTyping = true;
            if (narrativeText == null) { isTyping = false; yield break; }

            float interval = charsPerSecond > 0f ? 1f / charsPerSecond : 0.025f;
            narrativeText.text = "";

            for (int i = 0; i < fullText.Length; i++)
            {
                narrativeText.text += fullText[i];
                char c = fullText[i];
                if (c == '.' || c == '?' || c == '!')
                    yield return new WaitForSecondsRealtime(punctuationPause);
                else if (c == ',' || c == ';')
                    yield return new WaitForSecondsRealtime(punctuationPause * 0.4f);
                else
                    yield return new WaitForSecondsRealtime(interval);
            }

            isTyping = false;
        }

        private bool TryCompleteTyping()
        {
            if (!isTyping) return false;
            if (typingRoutine != null) StopCoroutine(typingRoutine);
            if (narrativeText != null) narrativeText.text = currentPages[currentPageIndex];
            isTyping = false;
            return true;
        }

        public void OnContinueClicked()
        {
            if (!canAdvance) return;
            if (TryCompleteTyping()) return;
            canAdvance = false;
            StartCoroutine(FadeAndNext());
        }

        public void OnNextPageClicked()
        {
            if (!canAdvance) return;
            if (TryCompleteTyping()) return;
            if (currentPageIndex >= currentPages.Length - 1) return;
            currentPageIndex++;
            ShowCurrentPage();
        }

        public void OnPrevPageClicked()
        {
            if (!canAdvance) return;
            if (TryCompleteTyping()) return;
            if (currentPageIndex <= 0) return;
            currentPageIndex--;
            ShowCurrentPage();
        }

        private IEnumerator FadeAndNext()
        {
            float dur = 0.28f;
            if (backgroundFader != null)
            {
                float t = 0f;
                while (t < dur) { t += Time.unscaledDeltaTime; backgroundFader.alpha = 1f - Mathf.Clamp01(t / dur); yield return null; }
            }
            if (textFader != null) textFader.alpha = 0f;

            if (StoryQueue.Pending.Count > 0) StartCoroutine(ShowNextScreen());
            else LoadNextScene();
        }

        private void LoadNextScene()
        {
            if (StoryQueue.PendingLevelToMarkSeen > 0)
            {
                StoryQueue.MarkBeforeLevelSeen(StoryQueue.PendingLevelToMarkSeen);
                StoryQueue.PendingLevelToMarkSeen = -1;
            }

            string target = StoryQueue.NextSceneName;
            StoryQueue.NextSceneName = null;

            if (string.IsNullOrEmpty(target) || !Application.CanStreamedLevelBeLoaded(target))
            {
                // Fallback: regresar al selector para no dejar al jugador perdido
                string fallback = Application.CanStreamedLevelBeLoaded("LevelSelect") ? "LevelSelect" : "MainMenu";
                if (SceneFader.Instance != null && Application.CanStreamedLevelBeLoaded(fallback))
                    SceneFader.Instance.LoadSceneWithFade(fallback);
                else
                    SceneManager.LoadScene(fallback);
                return;
            }

            if (SceneFader.Instance != null) SceneFader.Instance.LoadSceneWithFade(target);
            else SceneManager.LoadScene(target);
        }
    }
}
