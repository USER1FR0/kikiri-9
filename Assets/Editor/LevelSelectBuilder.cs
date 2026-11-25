using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Kikiri9.Session;
using Kikiri9.UI;
using TMPro;

namespace Kikiri9.EditorTools
{
    // Herramienta de editor que construye la escena LevelSelect.unity desde cero (#15):
    // 4 tarjetas de nivel usando selector_niveles.png (disponible/bloqueado/completado)
    // y estados_nivel.png (candado/completado como insignia pequena en la esquina).
    // Se puede volver a ejecutar para regenerar la escena si algo se rompe a mano.
    public static class LevelSelectBuilder
    {
        private const string SelectorSpriteSheetPath = "Assets/_Draft_2_Kikiri9/Art/UI/selector_niveles.png";
        private const string EstadosSpriteSheetPath = "Assets/_Draft_2_Kikiri9/Art/UI/estados_nivel.png";
        private const string ButtonsSpriteSheetPath = "Assets/_Draft_2_Kikiri9/Art/UI/botones.png";
        private const string MenuBackgroundPath = "Assets/_Draft_Kikiri9/Art/Backgrounds/menu_guanajuato.png";
        private const string LevelSelectScenePath = "Assets/_Project/Scenes/LevelSelect.unity";
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";

        [MenuItem("Tools/Kikiri-9/Build Level Select")]
        public static void BuildLevelSelectScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateEventSystem();
            CreateFaderObject();
            CreateBackgroundCamera();

            var canvasGo = CreateCanvas("Canvas");
            var canvasTransform = canvasGo.transform;

            CreateBackgroundImage(canvasTransform);

            var titleText = CreateText(canvasTransform, "TitleText", "SELECCIONA UN NIVEL", 48, TextAnchor.MiddleCenter);
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = new Color(1f, 0.78f, 0.32f);
            AddOutline(titleText.gameObject, new Color(0.15f, 0.05f, 0.2f), new Vector2(3f, -3f));
            SetAnchoredRect(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(1200f, 80f));

            var disponible = LoadSprite(SelectorSpriteSheetPath, "nivel_disponible");
            var bloqueado = LoadSprite(SelectorSpriteSheetPath, "nivel_bloqueado");
            var completado = LoadSprite(SelectorSpriteSheetPath, "nivel_completado");
            var candadoBadge = LoadSprite(EstadosSpriteSheetPath, "candado");
            var completadoBadge = LoadSprite(EstadosSpriteSheetPath, "completado");

            float[] xPositions = { -480f, -160f, 160f, 480f };
            string[] sceneNames =
            {
                SaveService.SceneNameForLevel(1),
                SaveService.SceneNameForLevel(2),
                SaveService.SceneNameForLevel(3),
                SaveService.SceneNameForLevel(4),
            };

            var cards = new List<LevelSelectController.LevelCard>();
            var cardButtons = new List<Button>();

            var controllerGo = new GameObject("LevelSelectController");
            var controller = controllerGo.AddComponent<LevelSelectController>();

            for (int i = 0; i < 4; i++)
            {
                int levelNumber = i + 1;
                var (button, cardImage, badgeImage) = CreateLevelCard(canvasTransform, levelNumber, xPositions[i], disponible);

                cards.Add(new LevelSelectController.LevelCard
                {
                    levelNumber = levelNumber,
                    sceneName = sceneNames[i],
                    button = button,
                    cardImage = cardImage,
                    badgeImage = badgeImage,
                });
                cardButtons.Add(button);

                UnityEventTools.AddIntPersistentListener(button.onClick, controller.OnCardClicked, levelNumber);
            }

            ChainNavigationHorizontal(cardButtons.ToArray());

            var normalSecondary = LoadSprite(ButtonsSpriteSheetPath, "secundario_normal");
            var highlightedSecondary = LoadSprite(ButtonsSpriteSheetPath, "secundario_seleccionado");
            var pressedSecondary = LoadSprite(ButtonsSpriteSheetPath, "secundario_presionado");
            var disabledSecondary = LoadSprite(ButtonsSpriteSheetPath, "secundario_desactivado");

            var backButton = CreateMenuButton(canvasTransform, "BackButton", "VOLVER", -320f, new Vector2(300f, 90f), 26, 0f,
                normalSecondary, highlightedSecondary, pressedSecondary, disabledSecondary);

            SetPrivateField(controller, "cards", cards.ToArray());
            SetPrivateField(controller, "disponibleSprite", disponible);
            SetPrivateField(controller, "bloqueadoSprite", bloqueado);
            SetPrivateField(controller, "completadoSprite", completado);
            SetPrivateField(controller, "candadoBadge", candadoBadge);
            SetPrivateField(controller, "completadoBadge", completadoBadge);
            SetPrivateField(controller, "menuSceneName", "MainMenu");

            UnityEventTools.AddPersistentListener(backButton.onClick, controller.OnBackClicked);

            EditorSceneManager.SaveScene(scene, LevelSelectScenePath, true);
            RegisterSceneInBuildSettings();

            Debug.Log("Listo: se genero LevelSelect.unity y se registro en Build Settings.");
        }

        private static (Button button, Image cardImage, Image badgeImage) CreateLevelCard(Transform parent, int levelNumber, float xOffset, Sprite initialSprite)
        {
            var go = new GameObject($"LevelCard_{levelNumber}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            SetAnchoredRect((RectTransform)go.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xOffset, 40f), new Vector2(300f, 105f));

            var cardImage = go.AddComponent<Image>();
            cardImage.sprite = initialSprite;
            cardImage.type = Image.Type.Simple;

            var button = go.AddComponent<Button>();
            button.targetGraphic = cardImage;
            button.transition = Selectable.Transition.None; // el estado visual lo maneja LevelSelectController, no SpriteSwap

            var labelText = CreateText(go.transform, "Label", $"NIVEL {levelNumber}", 28, TextAnchor.MiddleCenter);
            labelText.fontStyle = FontStyle.Bold;
            labelText.color = Color.white;
            AddOutline(labelText.gameObject, new Color(0.1f, 0.05f, 0.15f), new Vector2(2f, -2f));
            StretchFull(labelText.rectTransform);

            var badgeGo = new GameObject("Badge", typeof(RectTransform));
            badgeGo.transform.SetParent(go.transform, false);
            var badgeRect = (RectTransform)badgeGo.transform;
            badgeRect.anchorMin = new Vector2(1f, 1f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(1f, 1f);
            badgeRect.anchoredPosition = new Vector2(-6f, -6f);
            badgeRect.sizeDelta = new Vector2(44f, 44f);
            var badgeImage = badgeGo.AddComponent<Image>();
            badgeImage.raycastTarget = false;
            badgeImage.enabled = false;

            return (button, cardImage, badgeImage);
        }

        private static void RegisterSceneInBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == LevelSelectScenePath)) return;

            int insertIndex = scenes.FindIndex(s => s.path == MainMenuScenePath);
            insertIndex = insertIndex >= 0 ? insertIndex + 1 : scenes.Count;
            scenes.Insert(insertIndex, new EditorBuildSettingsScene(LevelSelectScenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static GameObject CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();

            var inputModule = go.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();

            return go;
        }

        private static GameObject CreateFaderObject()
        {
            var go = new GameObject("SceneFader");
            var canvasGo = CreateCanvas("FadeCanvas");
            canvasGo.transform.SetParent(go.transform);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.sortingOrder = 1000;

            var imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(canvasGo.transform);
            var image = imageGo.AddComponent<Image>();
            image.color = Color.black;
            StretchFull(imageGo.GetComponent<RectTransform>());

            var canvasGroup = imageGo.AddComponent<CanvasGroup>();

            var fader = go.AddComponent<SceneFader>();
            SetPrivateField(fader, "canvasGroup", canvasGroup);
            SetPrivateField(fader, "fadeDuration", 0.4f);

            return go;
        }

        private static void CreateBackgroundCamera()
        {
            var go = new GameObject("Main Camera");
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            camera.orthographic = true;
            go.tag = "MainCamera";
        }

        private static GameObject CreateCanvas(string name)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static void CreateBackgroundImage(Transform parent)
        {
            var go = new GameObject("Background", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.SetAsFirstSibling();

            var image = go.AddComponent<Image>();
            image.sprite = LoadSprite(MenuBackgroundPath, "menu_guanajuato_0");
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = false;

            StretchFull(go.GetComponent<RectTransform>());
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, float yOffset, Vector2 size, int fontSize, float floatPhase, Sprite normal, Sprite highlighted, Sprite pressed, Sprite disabled)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            SetAnchoredRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, yOffset), size);

            var image = go.AddComponent<Image>();
            image.sprite = normal;
            image.type = Image.Type.Simple;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;

            var spriteState = new SpriteState
            {
                highlightedSprite = highlighted,
                pressedSprite = pressed,
                disabledSprite = disabled
            };
            button.spriteState = spriteState;

            var textComponent = CreateText(go.transform, "Label", label, fontSize, TextAnchor.MiddleCenter);
            textComponent.fontStyle = FontStyle.Bold;
            StretchFull(textComponent.rectTransform);

            var floating = go.AddComponent<FloatingButton>();
            SetPrivateField(floating, "phaseOffset", floatPhase);

            return button;
        }

        private static void ChainNavigationHorizontal(params Button[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                var navigation = new Navigation { mode = Navigation.Mode.Explicit };
                if (i > 0) navigation.selectOnLeft = buttons[i - 1];
                if (i < buttons.Length - 1) navigation.selectOnRight = buttons[i + 1];
                buttons[i].navigation = navigation;
            }
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetAnchoredRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static Sprite LoadSprite(string path, string spriteName)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .FirstOrDefault(s => s.name == spriteName);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (field == null)
            {
                Debug.LogWarning($"LevelSelectBuilder: no se encontro el campo '{fieldName}' en {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
        }
    }
}
