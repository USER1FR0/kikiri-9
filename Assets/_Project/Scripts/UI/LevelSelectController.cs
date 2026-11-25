// Pantalla "Selector de Niveles" (#15). Se construye con LevelSelectBuilder
// (Tools > Kikiri-9 > Build Level Select). Nivel 1 siempre disponible; los demas
// se desbloquean segun SaveService.highestUnlockedLevel. Un nivel se pinta como
// "completado" en cuanto el siguiente ya esta desbloqueado.

using System;
using Kikiri9.Session;
using Kikiri9.UI.Story;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    public class LevelSelectController : MonoBehaviour
    {
        [Serializable]
        public class LevelCard
        {
            public int levelNumber;
            public string sceneName;
            public Button button;
            public Image cardImage;
            public Image badgeImage;
        }

        [SerializeField] private LevelCard[] cards;
        [SerializeField] private Sprite disponibleSprite;
        [SerializeField] private Sprite bloqueadoSprite;
        [SerializeField] private Sprite completadoSprite;
        [SerializeField] private Sprite candadoBadge;
        [SerializeField] private Sprite completadoBadge;
        [SerializeField] private string menuSceneName = "MainMenu";

        private void Start()
        {
            Refresh();
            EnsureReplayButton();
        }

        // Crea un boton flotante "VER HISTORIA" si no existe ya, sin tocar el
        // LevelSelectBuilder. Reproduce toda la narrativa en orden al presionarlo.
        private void EnsureReplayButton()
        {
            if (GameObject.Find("ReplayStoryCanvas") != null) return;

            var uiAssets = StoryUIAssets.Get();

            var canvasGo = new GameObject("ReplayStoryCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            SceneManager.MoveGameObjectToScene(canvasGo, SceneManager.GetActiveScene());

            var go = new GameObject("ReplayStoryButton",
                typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(canvasGo.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-40f, -40f);
            rt.sizeDelta = new Vector2(320f, 90f);

            var img = go.GetComponent<Image>();
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            if (uiAssets != null && uiAssets.buttonNormal != null)
            {
                img.sprite = uiAssets.buttonNormal;
                img.type = Image.Type.Simple;   // mismo render que JUGAR del MainMenu
                img.preserveAspect = false;
                img.color = Color.white;
                btn.transition = Selectable.Transition.SpriteSwap;
                btn.spriteState = new SpriteState
                {
                    highlightedSprite = uiAssets.buttonSelected,
                    pressedSprite = uiAssets.buttonPressed,
                    disabledSprite = uiAssets.buttonDisabled
                };
            }
            else
            {
                img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                img.type = Image.Type.Sliced;
                img.color = new Color(0.98f, 0.56f, 0.12f, 1f);
            }

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var lrt = (RectTransform)labelGo.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(16f, 10f);
            lrt.offsetMax = new Vector2(-16f, -10f);
            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = "VER HISTORIA";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.outlineColor = new Color(0.08f, 0.04f, 0.01f, 1f);
            tmp.outlineWidth = 0.28f;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 16f;
            tmp.fontSizeMax = 26f;
            if (uiAssets != null && uiAssets.font != null) tmp.font = uiAssets.font;

            btn.onClick.AddListener(OnReplayStoryClicked);
        }

        public void OnReplayStoryClicked()
        {
            string current = SceneManager.GetActiveScene().name;
            if (!StoryQueue.TryEnqueueFullReplay(current))
            {
                Debug.LogWarning("[ReplayStory] No se pudo encolar: StoryFlow.Get() devolvio null o estaba vacio. Corre Tools > Kikiri-9 > Build Story + Credits.");
                return;
            }
            StoryQueue.LoadStoryScene();
        }

        private void Refresh()
        {
            if (cards == null) return;
            var data = SaveService.Load();

            foreach (var card in cards)
            {
                bool unlocked = card.levelNumber <= data.highestUnlockedLevel;
                bool completed = card.levelNumber < data.highestUnlockedLevel;
                bool sceneExists = !string.IsNullOrEmpty(card.sceneName) && Application.CanStreamedLevelBeLoaded(card.sceneName);

                if (card.cardImage != null)
                    card.cardImage.sprite = !unlocked ? bloqueadoSprite : (completed ? completadoSprite : disponibleSprite);

                if (card.badgeImage != null)
                {
                    card.badgeImage.enabled = !unlocked || completed;
                    card.badgeImage.sprite = !unlocked ? candadoBadge : completadoBadge;
                }

                if (card.button != null)
                    card.button.interactable = unlocked && sceneExists;
            }
        }

        // Llamado por el Button.onClick de cada tarjeta (AddIntPersistentListener en
        // LevelSelectBuilder), pasando el numero de nivel de esa tarjeta.
        public void OnCardClicked(int levelNumber)
        {
            LevelCard card = null;
            foreach (var c in cards)
            {
                if (c.levelNumber == levelNumber) { card = c; break; }
            }

            if (card == null || string.IsNullOrEmpty(card.sceneName)) return;
            if (!Application.CanStreamedLevelBeLoaded(card.sceneName)) return;

            // Si este nivel tiene pantallas narrativas aun no vistas, pasan primero.
            if (StoryQueue.TryEnqueueBeforeLevel(levelNumber, card.sceneName))
            {
                StoryQueue.LoadStoryScene();
                return;
            }

            if (SceneFader.Instance != null) SceneFader.Instance.LoadSceneWithFade(card.sceneName);
            else SceneManager.LoadScene(card.sceneName);
        }

        public void OnBackClicked()
        {
            if (SceneFader.Instance != null && Application.CanStreamedLevelBeLoaded(menuSceneName))
                SceneFader.Instance.LoadSceneWithFade(menuSceneName);
            else if (Application.CanStreamedLevelBeLoaded(menuSceneName))
                SceneManager.LoadScene(menuSceneName);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
