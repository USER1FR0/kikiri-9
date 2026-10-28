using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Kikiri9.Session;
using Kikiri9.UI;

namespace Kikiri9.EditorTools
{
    // Herramienta de editor que construye las escenas Boot y MainMenu desde cero:
    // Canvas, EventSystem, botones con Sprite Swap (usando interfaz.png) y el
    // cableado de MainMenuController. Se puede volver a ejecutar para regenerar
    // las escenas si algo se rompe a mano.
    public static class MenuSceneBuilder
    {
        private const string InterfaceSpriteSheetPath = "Assets/_Draft_2_Kikiri9/Art/UI/interfaz.png";
        private const string BootScenePath = "Assets/_Project/Scenes/Boot.unity";
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";

        [MenuItem("Tools/Kikiri-9/Build Main Menu")]
        public static void BuildMainMenuScenes()
        {
            BuildBootScene();
            BuildMainMenuScene();
            RegisterScenesInBuildSettings();

            Debug.Log("Listo: se generaron Boot.unity y MainMenu.unity y se registraron en Build Settings.");
        }

        private static void BuildBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateEventSystem();
            CreateFaderObject();
            CreateBackgroundCamera();

            var canvasGo = CreateCanvas("Canvas");
            var logoText = CreateText(canvasGo.transform, "LogoText", "KIKIRI-9", 48, TextAnchor.MiddleCenter);
            StretchFull(logoText.rectTransform);

            var bootGo = new GameObject("BootSplash");
            var bootSplash = bootGo.AddComponent<BootSplash>();
            SetPrivateField(bootSplash, "nextSceneName", "MainMenu");

            EditorSceneManager.SaveScene(scene, BootScenePath, true);
        }

        private static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateEventSystem();
            CreateFaderObject();
            EnsureGameSessionExists();
            CreateBackgroundCamera();

            var canvasGo = CreateCanvas("Canvas");
            var canvasTransform = canvasGo.transform;

            var titleText = CreateText(canvasTransform, "TitleText", "KIKIRI-9: EL ULTIMO PAN", 40, TextAnchor.MiddleCenter);
            SetAnchoredRect(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(900f, 100f));

            var normal = LoadSprite(InterfaceSpriteSheetPath, "botones_01");
            var highlighted = LoadSprite(InterfaceSpriteSheetPath, "botones_02");
            var pressed = LoadSprite(InterfaceSpriteSheetPath, "botones_03");
            var disabled = LoadSprite(InterfaceSpriteSheetPath, "botones_04");

            var playButton = CreateMenuButton(canvasTransform, "PlayButton", "JUGAR", 120f, normal, highlighted, pressed, disabled);
            var continueButton = CreateMenuButton(canvasTransform, "ContinueButton", "CONTINUAR", 10f, normal, highlighted, pressed, disabled);
            var optionsButton = CreateMenuButton(canvasTransform, "OptionsButton", "OPCIONES", -100f, normal, highlighted, pressed, disabled);
            var creditsButton = CreateMenuButton(canvasTransform, "CreditsButton", "CREDITOS", -210f, normal, highlighted, pressed, disabled);
            var exitButton = CreateMenuButton(canvasTransform, "ExitButton", "SALIR", -320f, normal, highlighted, pressed, disabled);

            ChainNavigationVertical(playButton, continueButton, optionsButton, creditsButton, exitButton);

            var optionsPanel = CreateOverlayPanel(canvasTransform, "OptionsPanel", "OPCIONES\n\n(Proximamente)");
            var creditsPanel = CreateOverlayPanel(canvasTransform, "CreditsPanel", "CREDITOS\n\nManuel Mata Campos\nJuan Francisco Rodriguez Guerrero");
            var exitMessagePanel = CreateOverlayPanel(canvasTransform, "ExitMessagePanel", "Gracias por jugar. Puedes cerrar esta pestana.");

            AddCloseButton(optionsPanel.transform);
            AddCloseButton(creditsPanel.transform);

            optionsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            exitMessagePanel.SetActive(false);

            var controllerGo = new GameObject("MainMenuController");
            var controller = controllerGo.AddComponent<MainMenuController>();
            SetPrivateField(controller, "optionsPanel", optionsPanel);
            SetPrivateField(controller, "creditsPanel", creditsPanel);
            SetPrivateField(controller, "continueButton", continueButton.gameObject);
            SetPrivateField(controller, "exitMessagePanel", exitMessagePanel);
            SetPrivateField(controller, "defaultLevelSceneName", "Sandbox");
            SetPrivateField(controller, "firstSelectedButton", playButton.gameObject);

            UnityEventTools.AddPersistentListener(playButton.onClick, controller.OnPlayPressed);
            UnityEventTools.AddPersistentListener(continueButton.onClick, controller.OnContinuePressed);
            UnityEventTools.AddPersistentListener(optionsButton.onClick, controller.OnOptionsPressed);
            UnityEventTools.AddPersistentListener(creditsButton.onClick, controller.OnCreditsPressed);
            UnityEventTools.AddPersistentListener(exitButton.onClick, controller.OnExitPressed);

            foreach (var closeButton in optionsPanel.GetComponentsInChildren<Button>(true))
            {
                if (closeButton.gameObject.name == "CloseButton")
                {
                    UnityEventTools.AddPersistentListener(closeButton.onClick, controller.OnClosePanelPressed);
                }
            }

            foreach (var closeButton in creditsPanel.GetComponentsInChildren<Button>(true))
            {
                if (closeButton.gameObject.name == "CloseButton")
                {
                    UnityEventTools.AddPersistentListener(closeButton.onClick, controller.OnClosePanelPressed);
                }
            }

            EditorSceneManager.SaveScene(scene, MainMenuScenePath, true);
        }

        private static void RegisterScenesInBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(s => s.path == BootScenePath || s.path == MainMenuScenePath);

            scenes.Insert(0, new EditorBuildSettingsScene(MainMenuScenePath, true));
            scenes.Insert(0, new EditorBuildSettingsScene(BootScenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static GameObject CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();

            var inputModule = go.AddComponent<InputSystemUIInputModule>();
            // Al crear el componente por codigo no se asigna automaticamente el mapa de
            // controles (mouse, teclado, gamepad, tactil). AssignDefaultActions() crea y
            // asigna el set por defecto, igual que si lo hubieramos agregado a mano desde
            // el menu de Unity.
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

        private static void EnsureGameSessionExists()
        {
            var go = new GameObject("GameSession");
            go.AddComponent<GameSession>();
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

        private static Button CreateMenuButton(Transform parent, string name, string label, float yOffset, Sprite normal, Sprite highlighted, Sprite pressed, Sprite disabled)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            SetAnchoredRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, yOffset), new Vector2(460f, 90f));

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

            var textComponent = CreateText(go.transform, "Label", label, 32, TextAnchor.MiddleCenter);
            StretchFull(textComponent.rectTransform);

            return button;
        }

        private static GameObject CreateOverlayPanel(Transform parent, string name, string message)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            StretchFull(rect);

            var image = go.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.85f);

            var text = CreateText(go.transform, "Message", message, 30, TextAnchor.MiddleCenter);
            SetAnchoredRect(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 400f));

            return go;
        }

        private static void AddCloseButton(Transform panelTransform)
        {
            var go = new GameObject("CloseButton", typeof(RectTransform));
            go.transform.SetParent(panelTransform, false);

            var rect = go.GetComponent<RectTransform>();
            SetAnchoredRect(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(240f, 56f));

            var image = go.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            go.AddComponent<Button>();

            var text = CreateText(go.transform, "Label", "VOLVER", 24, TextAnchor.MiddleCenter);
            StretchFull(text.rectTransform);
        }

        private static void ChainNavigationVertical(params Button[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                var navigation = new Navigation { mode = Navigation.Mode.Explicit };

                if (i > 0)
                {
                    navigation.selectOnUp = buttons[i - 1];
                }

                if (i < buttons.Length - 1)
                {
                    navigation.selectOnDown = buttons[i + 1];
                }

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
                Debug.LogWarning($"MenuSceneBuilder: no se encontro el campo '{fieldName}' en {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
        }
    }
}
