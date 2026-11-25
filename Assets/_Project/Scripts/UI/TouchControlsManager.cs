// Controles tactiles de movil. Al cargar una escena de nivel:
//  1. Si existe el prefab Resources/UI/TouchControlsCanvas, lo INSTANCIA
//     tal cual esta (asi puedes acomodar los botones manualmente en el prefab).
//  2. Si no existe el prefab, construye por codigo como fallback.
//
// Despues engancha los botones (por nombre) a los metodos publicos del
// PlayerController. Visibilidad controlada por GameOptions.ShowControls.

using System.Collections.Generic;
using Kikiri9.Player;
using Kikiri9.Session;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    public class TouchControlsManager : MonoBehaviour
    {
        private const string CanvasName = "TouchControlsCanvas";
        private const string PrefabResourcesPath = "UI/TouchControlsCanvas";
        private const string SpriteSheetResourcesPath = "UI/controles_tactiles";

        private static Sprite[] loadedAll;
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        private Canvas canvas;
        private PlayerController player;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryInstallFor(SceneManager.GetActiveScene().name);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => TryInstallFor(scene.name);

        private static void TryInstallFor(string sceneName)
        {
            if (!sceneName.StartsWith("Level") || sceneName == "LevelSelect") return;
            if (GameObject.Find(CanvasName) != null) return;

            var go = new GameObject("TouchControlsManager");
            go.AddComponent<TouchControlsManager>();
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
        }

        private void Start()
        {
            SetupCanvas();
            EnsureEventSystem();
            FindPlayer();
            WireButtons();
            RefreshVisibility();
        }

        private void Update()
        {
            if (canvas != null && canvas.enabled != GameOptions.ShowControls)
                RefreshVisibility();
        }

        private void SetupCanvas()
        {
            var prefab = Resources.Load<GameObject>(PrefabResourcesPath);
            GameObject canvasGo;
            if (prefab != null)
            {
                canvasGo = Instantiate(prefab, transform);
                canvasGo.name = CanvasName;
            }
            else
            {
                canvasGo = BuildCanvasByCode();
            }
            canvas = canvasGo.GetComponent<Canvas>();
        }

        private GameObject BuildCanvasByCode()
        {
            var cgo = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            cgo.transform.SetParent(transform, false);
            var c = cgo.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 40;
            var scaler = cgo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            BuildFallbackButton(cgo.transform, "TouchSlide", "deslizar_normal", "deslizar_presionado",
                new Vector2(1f, 0f), new Vector2(-340f, 60f), new Vector2(150f, 150f));
            BuildFallbackButton(cgo.transform, "TouchThrow", "lanzar_normal", "lanzar_presionado",
                new Vector2(1f, 0f), new Vector2(-340f, 230f), new Vector2(150f, 150f));
            BuildFallbackButton(cgo.transform, "TouchJump", "saltar_normal", "saltar_presionado",
                new Vector2(1f, 0f), new Vector2(-180f, 160f), new Vector2(170f, 170f));

            return cgo;
        }

        private GameObject BuildFallbackButton(Transform parent, string name,
            string normalSpriteName, string pressedSpriteName,
            Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(EventTrigger));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var normal = LoadSprite(normalSpriteName);
            var pressed = LoadSprite(pressedSpriteName);

            var img = go.GetComponent<Image>();
            img.sprite = normal;
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.raycastTarget = true;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.spriteState = new SpriteState
            {
                pressedSprite = pressed,
                highlightedSprite = normal,
                selectedSprite = normal
            };
            return go;
        }

        private void WireButtons()
        {
            if (canvas == null) return;
            WireButton("TouchJump",  jumpPress: true,  jumpRelease: true);
            WireButton("TouchSlide", slidePress: true);
            WireButton("TouchThrow", throwPress: true);
        }

        private void WireButton(string name, bool jumpPress = false, bool jumpRelease = false,
            bool slidePress = false, bool throwPress = false)
        {
            var t = canvas.transform.Find(name);
            if (t == null) return;
            var go = t.gameObject;
            var trig = go.GetComponent<EventTrigger>();
            if (trig == null) trig = go.AddComponent<EventTrigger>();
            trig.triggers.Clear();

            if (jumpPress)   AddTrigger(trig, EventTriggerType.PointerDown, _ => DoJumpPress());
            if (jumpRelease)
            {
                AddTrigger(trig, EventTriggerType.PointerUp,   _ => DoJumpRelease());
                AddTrigger(trig, EventTriggerType.PointerExit, _ => DoJumpRelease());
            }
            if (slidePress)  AddTrigger(trig, EventTriggerType.PointerDown, _ => DoSlidePress());
            if (throwPress)  AddTrigger(trig, EventTriggerType.PointerDown, _ => DoThrowPress());
        }

        private static void AddTrigger(EventTrigger trig, EventTriggerType type, System.Action<BaseEventData> handler)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(handler));
            trig.triggers.Add(entry);
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
            SceneManager.MoveGameObjectToScene(es, SceneManager.GetActiveScene());
            #if ENABLE_INPUT_SYSTEM
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            #else
            es.AddComponent<StandaloneInputModule>();
            #endif
        }

        private void FindPlayer()
        {
            if (player != null) return;
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.GetComponent<PlayerController>();
        }

        private void RefreshVisibility()
        {
            if (canvas != null) canvas.enabled = GameOptions.ShowControls;
        }

        private void DoJumpPress()   { if (EnsurePlayer()) player.OnTouchJumpPressed(); }
        private void DoJumpRelease() { if (EnsurePlayer()) player.OnTouchJumpReleased(); }
        private void DoSlidePress()  { if (EnsurePlayer()) player.OnTouchSlidePressed(); }
        private void DoThrowPress()  { if (EnsurePlayer()) player.OnTouchThrowPressed(); }

        private bool EnsurePlayer() { if (player == null) FindPlayer(); return player != null; }

        private static Sprite LoadSprite(string name)
        {
            if (SpriteCache.TryGetValue(name, out var cached) && cached != null) return cached;
            if (loadedAll == null) loadedAll = Resources.LoadAll<Sprite>(SpriteSheetResourcesPath);
            if (loadedAll != null)
                foreach (var sp in loadedAll)
                    if (sp != null && sp.name == name) { SpriteCache[name] = sp; return sp; }
            return null;
        }
    }
}
