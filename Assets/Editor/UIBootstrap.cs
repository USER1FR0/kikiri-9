// Editor-only. Builds Prefabs/UI/UI_Root.prefab with Canvas + HUD + 4 panels. Level
// bootstraps instantiate this prefab in-scene. All visuals use sprites already sliced
// from UI/interfaz.png (hud / controles / botones / paneles rows). Text uses TMP.
//
// Run from menu: Tools > Kikiri-9 > Build UI Prefab

using System.IO;
using Kikiri9.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Kikiri9.EditorTools
{
    public static class UIBootstrap
    {
        private const string UIPrefabDir = "Assets/_Project/Prefabs/UI";
        private const string UIPrefabPath = UIPrefabDir + "/UI_Root.prefab";

        private static TMP_FontAsset pixelFont;

        [MenuItem("Tools/Kikiri-9/Build UI Prefab")]
        public static void Build()
        {
            Directory.CreateDirectory(UIPrefabDir);

            // Sprites del nuevo atlas
            var heartFull = FindUI("corazon_lleno");
            var heartEmpty = FindUI("corazon_vacio");
            var breadIcon = FindUI("pan");
            var coinIcon = FindUI("botella_verde");
            var pauseIcon = FindUI("pausa");
            var basePausa = FindUI("base_pausa");
            var placaVidas = FindUI("placa_vidas");
            var placaPuntos = FindUI("placa_puntos");
            var placaTiempo = FindUI("placa_tiempo");

            // Botones primario (naranja) y secundario (morado) con 4 estados
            var btnPrimary = new ButtonSpriteSet(
                FindUI("primario_normal"),
                FindUI("primario_seleccionado"),
                FindUI("primario_presionado"),
                FindUI("primario_desactivado"));
            var btnSecondary = new ButtonSpriteSet(
                FindUI("secundario_normal"),
                FindUI("secundario_seleccionado"),
                FindUI("secundario_presionado"),
                FindUI("secundario_desactivado"));

            // Fuente pixel-art
            pixelFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetBuilder.FontAssetPath);

            // Panel y barras
            var panelBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Draft_2_Kikiri9/Art/UI/panel_resultados.png");
            var barraMarco = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Draft_2_Kikiri9/Art/UI/barra_marco.png");
            var barraRelleno = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Draft_2_Kikiri9/Art/UI/barra_relleno.png");

            // Root Canvas
            var root = new GameObject("UI_Root", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // EventSystem (lo crea quien instancie si no existe; aqui lo embedamos tambien)
            var eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule), typeof(EventSystemAutoBootstrap));
            eventSystem.transform.SetParent(root.transform, false);

            // HUD primero: queda como sibling temprano para que los paneles (dim incluido)
            // rendericen por encima cuando esten activos. En Unity UI el ultimo sibling gana.
            var hud = BuildHUD(root.transform, heartFull, heartEmpty, breadIcon, coinIcon,
                pauseIcon, basePausa, placaVidas, placaPuntos, placaTiempo,
                barraMarco, barraRelleno, btnSecondary);

            // Banner de META durante LevelOutro (encima del HUD, debajo de los paneles)
            BuildOutroBanner(root.transform);

            // Paneles con panel_resultados.png (9-sliced). Al ser los ultimos siblings,
            // sus dim+contenido tapan correctamente el HUD cuando el panel esta activo.
            BuildTutorialPanel(root.transform, panelBg, btnPrimary, btnSecondary);
            BuildPausePanel(root.transform, panelBg, btnPrimary, btnSecondary);
            BuildLevelCompletePanel(root.transform, panelBg, btnPrimary, btnSecondary);
            BuildGameOverPanel(root.transform, panelBg, btnPrimary, btnSecondary);

            // Guardar prefab limpio: borramos el anterior para no dejar scripts huerfanos.
            if (AssetDatabase.LoadAssetAtPath<GameObject>(UIPrefabPath) != null)
                AssetDatabase.DeleteAsset(UIPrefabPath);
            PrefabUtility.SaveAsPrefabAsset(root, UIPrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Kikiri-9] UI prefab built at {UIPrefabPath}");
        }

        // -------- HUD --------

        private static HUDController BuildHUD(Transform parent, Sprite heartFull, Sprite heartEmpty,
            Sprite breadIcon, Sprite coinIcon, Sprite pauseIcon, Sprite basePausaSprite,
            Sprite placaVidas, Sprite placaPuntos, Sprite placaTiempo,
            Sprite barraMarco, Sprite barraRelleno, ButtonSpriteSet btnSecondary)
        {
            var hudGo = new GameObject("HUD", typeof(RectTransform));
            hudGo.transform.SetParent(parent, false);
            StretchFill((RectTransform)hudGo.transform);

            // ------ TOP BAR: tres placas (vidas, puntos, tiempo+pausa) ------
            var top = new GameObject("TopBar", typeof(RectTransform));
            top.transform.SetParent(hudGo.transform, false);
            Anchor((RectTransform)top.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(-40f, 140f));

            // Placa vidas (320x112 => aspect ~2.86)
            var placaV = NewImage("PlacaVidas", top.transform, placaVidas);
            Anchor((RectTransform)placaV.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(30f, 0f), new Vector2(420f, 120f));
            placaV.preserveAspect = false;
            // Container 65x65 con preserveAspect => render ~65x60. Spacing 115 centros.
            // Centros 65, 180, 295 dentro de placa 420 wide => mas aire a los lados y entre.
            var hearts = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var h = NewImage($"Heart_{i}", placaV.transform, heartFull);
                h.preserveAspect = true;
                Anchor((RectTransform)h.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(65f + i * 115f, 0f), new Vector2(65f, 65f));
                hearts[i] = h;
            }

            // Placa puntos centro: mas compacta para dar aire al layout.
            var placaP = NewImage("PlacaPuntos", top.transform, placaPuntos);
            Anchor((RectTransform)placaP.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(320f, 120f));
            var score = NewText("ScoreText", placaP.transform, "00000", 38, TextAlignmentOptions.Center);
            StretchFill((RectTransform)score.transform);
            score.color = Color.white;

            // Placa tiempo + boton pausa (right). Placa mas corta y pause mas compacto.
            var placaT = NewImage("PlacaTiempo", top.transform, placaTiempo);
            Anchor((RectTransform)placaT.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-30f, 0f), new Vector2(420f, 120f));
            var time = NewText("TimeText", placaT.transform, "00:00", 32, TextAlignmentOptions.Center);
            Anchor((RectTransform)time.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(40f, 0f), new Vector2(200f, 60f));
            // base_pausa como fondo del boton: 100x100 compacto, pegado al borde derecho.
            var pauseBtnGo = new GameObject("PauseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            pauseBtnGo.transform.SetParent(placaT.transform, false);
            var pbImg = pauseBtnGo.GetComponent<Image>();
            pbImg.sprite = basePausaSprite;
            pbImg.type = Image.Type.Simple;
            Anchor((RectTransform)pauseBtnGo.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-35f, 0f), new Vector2(80f, 80f));
            // Icono pausa con padding proporcional: icon final ~60x60 dentro de base 80x80.
            var pauseIcoGo = NewImage("Icon", pauseBtnGo.transform, pauseIcon);
            pauseIcoGo.preserveAspect = true;
            StretchFill((RectTransform)pauseIcoGo.transform, new Vector2(-15f, -15f));
            pauseBtnGo.AddComponent<PauseButtonHelper>();

            // ------ BOTTOM BAR: barra_marco + relleno + contadores ------
            var bot = new GameObject("BottomBar", typeof(RectTransform));
            bot.transform.SetParent(hudGo.transform, false);
            Anchor((RectTransform)bot.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 20f), new Vector2(-40f, 110f));

            // Metros (texto solo, bottom-left)
            var meters = NewText("MetersText", bot.transform, "0 / 500 m", 34, TextAlignmentOptions.Left);
            Anchor((RectTransform)meters.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(30f, 0f), new Vector2(280f, 50f));
            meters.color = Color.white;

            // Barra de progreso: marco + relleno, centrada. Thin para que no domine.
            var barraGo = NewImage("ProgressFrame", bot.transform, barraMarco);
            Anchor((RectTransform)barraGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(560f, 28f));
            var slider = BuildFilledSlider(barraGo.transform, barraRelleno);
            var sliderRT = (RectTransform)slider.transform;
            sliderRT.anchorMin = Vector2.zero; sliderRT.anchorMax = Vector2.one; sliderRT.pivot = new Vector2(0.5f, 0.5f);
            sliderRT.anchoredPosition = Vector2.zero;
            sliderRT.offsetMin = new Vector2(8f, 8f);
            sliderRT.offsetMax = new Vector2(-8f, -8f);

            // Contadores pan + botella (bottom-right). Wrap 200 wide => separacion 220.
            TMP_Text breadText, coinsText;
            BuildIconCounter("Bread", bot.transform, breadIcon, "0", new Vector2(-240f, 0f), out breadText);
            BuildIconCounter("Coins", bot.transform, coinIcon, "0", new Vector2(-20f, 0f), out coinsText);

            var ctrl = hudGo.AddComponent<HUDController>();
            var so = new SerializedObject(ctrl);
            var heartArr = so.FindProperty("heartImages");
            heartArr.arraySize = 3;
            for (int i = 0; i < 3; i++) heartArr.GetArrayElementAtIndex(i).objectReferenceValue = hearts[i];
            so.FindProperty("heartFullSprite").objectReferenceValue = heartFull;
            so.FindProperty("heartEmptySprite").objectReferenceValue = heartEmpty;
            so.FindProperty("scoreText").objectReferenceValue = score;
            so.FindProperty("timeText").objectReferenceValue = time;
            so.FindProperty("metersText").objectReferenceValue = meters;
            so.FindProperty("breadText").objectReferenceValue = breadText;
            so.FindProperty("coinsText").objectReferenceValue = coinsText;
            so.FindProperty("progressSlider").objectReferenceValue = slider;
            so.ApplyModifiedProperties();

            return ctrl;
        }

        private static Slider BuildFilledSlider(Transform parent, Sprite fillSprite)
        {
            var go = new GameObject("ProgressSlider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var slider = go.GetComponent<Slider>();
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f;
            slider.transition = Selectable.Transition.None;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            StretchFill((RectTransform)fillArea.transform);

            var fill = NewImage("Fill", fillArea.transform, fillSprite);
            fill.type = Image.Type.Simple;
            StretchFill((RectTransform)fill.transform);

            slider.fillRect = (RectTransform)fill.transform;
            slider.targetGraphic = fill;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }



        private static void BuildIconCounter(string name, Transform parent, Sprite icon, string initial, Vector2 anchoredPos, out TMP_Text txt)
        {
            var wrap = new GameObject($"{name}_Counter", typeof(RectTransform));
            wrap.transform.SetParent(parent, false);
            Anchor((RectTransform)wrap.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                anchoredPos, new Vector2(200f, 90f));

            // Icono compacto 60x60: con sprite recortado al contenido queda bien visible
            // sin dominar el bottom bar. Texto a su derecha, aligned left, mismo peso visual.
            var ic = NewImage("Icon", wrap.transform, icon);
            ic.preserveAspect = true;
            Anchor((RectTransform)ic.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0f), new Vector2(60f, 60f));

            txt = NewText("Count", wrap.transform, initial, 38, TextAlignmentOptions.Left);
            Anchor((RectTransform)txt.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(70f, 0f), new Vector2(100f, 50f));
            txt.color = Color.white;
        }

        // -------- Paneles --------

        private static void BuildOutroBanner(Transform parent)
        {
            var container = new GameObject("OutroBanner", typeof(RectTransform));
            container.transform.SetParent(parent, false);
            StretchFill((RectTransform)container.transform);

            var visual = new GameObject("Visual", typeof(RectTransform));
            visual.transform.SetParent(container.transform, false);
            Anchor((RectTransform)visual.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1200f, 240f));

            var txt = NewText("Label", visual.transform, "¡META!", 110, TextAlignmentOptions.Center);
            StretchFill((RectTransform)txt.transform);
            txt.color = new Color(1f, 0.7f, 0.15f, 1f);
            // Shadow effect via another text behind
            var shadow = NewText("Shadow", visual.transform, "¡META!", 110, TextAlignmentOptions.Center);
            StretchFill((RectTransform)shadow.transform);
            shadow.color = new Color(0f, 0f, 0f, 0.7f);
            // Shadow debe estar DETRAS: mover al principio del child list
            shadow.transform.SetSiblingIndex(0);
            // Desplazar el shadow 8px abajo-derecha
            var srt = (RectTransform)shadow.transform;
            srt.anchoredPosition = new Vector2(8f, -8f);

            var ob = container.AddComponent<OutroBanner>();
            var so = new SerializedObject(ob);
            so.FindProperty("root").objectReferenceValue = visual;
            so.FindProperty("label").objectReferenceValue = txt;
            so.ApplyModifiedProperties();
        }

        private static void BuildTutorialPanel(Transform parent, Sprite panelBg, ButtonSpriteSet btnPri, ButtonSpriteSet btnSec)
        {
            var (container, visual) = BuildPanelFrame("TutorialPanel", parent, panelBg, "COMO JUGAR", out var body);
            body.text = "ESPACIO / W / Flecha arriba: Saltar\nS / Flecha abajo / Shift: Deslizar\nJ / Click: Lanzar pan\n\nMeta: 500 metros sin perder los 3 corazones.";
            var btn = NewButtonSet("UnderstoodButton", visual.transform, btnPri, null, "Entendido");
            Anchor((RectTransform)btn.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 130f), new Vector2(360f, 90f));

            var tp = container.AddComponent<TutorialPanel>();
            var so = new SerializedObject(tp);
            so.FindProperty("root").objectReferenceValue = visual;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.ApplyModifiedProperties();

            var h = btn.AddComponent<ButtonActionHelper>();
            h.target = container; h.action = "OnUnderstoodClicked";
            // Button auto-wires via helper.Awake()
        }

        private static void BuildPausePanel(Transform parent, Sprite panelBg, ButtonSpriteSet btnPri, ButtonSpriteSet btnSec)
        {
            var (container, visual) = BuildPanelFrame("PausePanel", parent, panelBg, "PAUSA", out var _);
            var pp = container.AddComponent<PausePanel>();

            string[] labels = { "Reanudar", "Reiniciar", "Opciones", "Menu" };
            string[] actions = { "OnResumeClicked", "OnRestartClicked", "OnOptionsClicked", "OnMenuClicked" };
            for (int i = 0; i < 4; i++)
            {
                // 2 columnas x 2 filas en panel 1200x650. Fila 1 Y=+50, fila 2 Y=-80
                // para dejar gap limpio con el titulo (bottom en Y=125) y entre filas.
                float x = (i % 2 == 0) ? -200f : 200f;
                float y = (i < 2) ? 50f : -80f;
                var set = (i == 0) ? btnPri : btnSec;
                var b = NewButtonSet($"Btn_{labels[i]}", visual.transform, set, null, labels[i]);
                Anchor((RectTransform)b.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(x, y), new Vector2(420f, 120f));
                var h = b.AddComponent<ButtonActionHelper>();
                h.target = container; h.action = actions[i];
                // Button auto-wires via helper.Awake()
            }

            var so = new SerializedObject(pp);
            so.FindProperty("root").objectReferenceValue = visual;
            so.ApplyModifiedProperties();
        }

        private static void BuildLevelCompletePanel(Transform parent, Sprite panelBg, ButtonSpriteSet btnPri, ButtonSpriteSet btnSec)
        {
            var (container, visual) = BuildPanelFrame("LevelCompletePanel", parent, panelBg, "NIVEL COMPLETADO", out var body);
            body.text = "";

            // Score/Time dentro del nuevo panel 1200x650 (half 325). Titulo spans Y=125..205.
            // Score Y=85 => spans 65..105 (gap 20 titulo). Time Y=35 => spans 15..55 (gap 10 score).
            // Body default (Y=-55 size 1050x110) spans Y=-110..0 => gap 15 con time.
            var scoreT = NewText("ScoreText", visual.transform, "Puntaje: 0", 28, TextAlignmentOptions.Center);
            Anchor((RectTransform)scoreT.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 85f), new Vector2(600f, 40f));
            var timeT = NewText("TimeText", visual.transform, "Tiempo: 00:00", 28, TextAlignmentOptions.Center);
            Anchor((RectTransform)timeT.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 35f), new Vector2(600f, 40f));

            var lcp = container.AddComponent<LevelCompletePanel>();

            // Botones mismo tamano (320x90) y simetricos (X=+-170) para que lean como par.
            var next = NewButtonSet("NextButton", visual.transform, btnPri, null, "Siguiente");
            Anchor((RectTransform)next.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-170f, 130f), new Vector2(320f, 90f));
            var menu = NewButtonSet("MenuButton", visual.transform, btnSec, null, "Menu");
            Anchor((RectTransform)menu.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(170f, 130f), new Vector2(320f, 90f));

            WireBtn(next, container, "OnNextClicked");
            WireBtn(menu, container, "OnMenuClicked");

            var so = new SerializedObject(lcp);
            so.FindProperty("root").objectReferenceValue = visual;
            so.FindProperty("scoreText").objectReferenceValue = scoreT;
            so.FindProperty("timeText").objectReferenceValue = timeT;
            so.FindProperty("narrativeText").objectReferenceValue = body;
            so.ApplyModifiedProperties();
        }

        private static void BuildGameOverPanel(Transform parent, Sprite panelBg, ButtonSpriteSet btnPri, ButtonSpriteSet btnSec)
        {
            var (container, visual) = BuildPanelFrame("GameOverPanel", parent, panelBg, "GAME OVER", out var body);
            // GameOver no necesita el body generico: ponemos un mensaje breve arriba y las
            // dos columnas abajo. El body "narrativa" se vacia.
            body.text = "El pollo cayo. Intenta de nuevo.";
            Anchor((RectTransform)body.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 80f), new Vector2(680f, 44f));
            var gop = container.AddComponent<GameOverPanel>();

            // Distancia / Puntaje en dos columnas, Y=20 label y Y=-30 value
            var distLabel = NewText("DistLabel", visual.transform, "Distancia", 26, TextAlignmentOptions.Center);
            Anchor((RectTransform)distLabel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-140f, 10f), new Vector2(220f, 32f));
            distLabel.color = new Color(1f, 0.75f, 0.3f, 1f);
            var distValue = NewText("DistValue", visual.transform, "0 / 500 m", 34, TextAlignmentOptions.Center);
            Anchor((RectTransform)distValue.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-140f, -30f), new Vector2(240f, 40f));
            var scoreLabel = NewText("ScoreLabel", visual.transform, "Puntaje", 26, TextAlignmentOptions.Center);
            Anchor((RectTransform)scoreLabel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(140f, 10f), new Vector2(220f, 32f));
            scoreLabel.color = new Color(1f, 0.75f, 0.3f, 1f);
            var scoreValue = NewText("ScoreValue", visual.transform, "00000", 34, TextAlignmentOptions.Center);
            Anchor((RectTransform)scoreValue.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(140f, -30f), new Vector2(240f, 40f));

            // Botones mismo tamano (320x90) y simetricos (X=+-170) para que lean como par.
            var retry = NewButtonSet("RetryButton", visual.transform, btnPri, null, "Reintentar");
            Anchor((RectTransform)retry.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-170f, 130f), new Vector2(320f, 90f));
            var menu = NewButtonSet("MenuButton", visual.transform, btnSec, null, "Menu");
            Anchor((RectTransform)menu.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(170f, 130f), new Vector2(320f, 90f));

            WireBtn(retry, container, "OnRetryClicked");
            WireBtn(menu, container, "OnMenuClicked");

            var so = new SerializedObject(gop);
            so.FindProperty("root").objectReferenceValue = visual;
            so.FindProperty("distanceText").objectReferenceValue = distValue;
            so.FindProperty("scoreText").objectReferenceValue = scoreValue;
            so.ApplyModifiedProperties();
        }


        private static (GameObject container, GameObject visual) BuildPanelFrame(string name, Transform parent, Sprite panelBg, string title, out TMP_Text body)
        {
            // Container: always active, carries the script and subscribes to events.
            var container = new GameObject(name, typeof(RectTransform));
            container.transform.SetParent(parent, false);
            StretchFill((RectTransform)container.transform);

            // Visual: the actual panel UI; this is what gets activated/deactivated.
            var visual = new GameObject("Visual", typeof(RectTransform));
            visual.transform.SetParent(container.transform, false);
            // Panel 1200x650 (ratio ~1.85): mas compacto, deja aire al HUD arriba y abajo
            // sin dejar de respetar la silueta del sprite panel_resultados.png (9-sliced).
            Anchor((RectTransform)visual.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1200f, 650f));

            // Dim screen behind panel
            var dim = NewImage("Dim", visual.transform, null);
            dim.color = new Color(0f, 0f, 0f, 0.65f);
            StretchParent((RectTransform)dim.transform, new Vector2(5000f, 5000f));

            // Panel bg: panel_resultados.png con 9-slice (border 20) para escalar sin deformar
            var bg = NewImage("Bg", visual.transform, panelBg);
            bg.type = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 1f;
            StretchFill((RectTransform)bg.transform);

            // Title (dentro del area util del sprite paneles_01)
            var titleT = NewText("Title", visual.transform, title, 44, TextAlignmentOptions.Center);
            Anchor((RectTransform)titleT.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -120f), new Vector2(1000f, 80f));

            // Body (word wrap para narrativas largas). Dimensionado para el nuevo panel 1200x650:
            // Y=-55 size 1050x110 => spans Y=-110..0, deja gap con time arriba y botones abajo.
            // GameOverPanel reposiciona el body a Y=+80 explicitamente.
            body = NewText("Body", visual.transform, "", 20, TextAlignmentOptions.Center);
            body.textWrappingMode = TMPro.TextWrappingModes.Normal;
            Anchor((RectTransform)body.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -55f), new Vector2(1050f, 110f));

            return (container, visual);
        }

        // -------- helpers --------

        private static void WireBtn(GameObject buttonGo, GameObject target, string action)
        {
            var h = buttonGo.AddComponent<ButtonActionHelper>();
            h.target = target; h.action = action;
            // Button auto-wires via helper.Awake()
        }

        private static Image NewImage(string name, Transform parent, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            return img;
        }

        private static TMP_Text NewText(string name, Transform parent, string text, int size, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            // Si existe la fuente Press Start 2P, usarla con un size un poco menor (es alta).
            if (pixelFont != null)
            {
                t.font = pixelFont;
                t.fontSize = size;
            }
            else
            {
                t.fontSize = size;
                t.fontStyle = FontStyles.Bold;
            }
            t.alignment = align;
            t.color = Color.white;
            t.textWrappingMode = TextWrappingModes.NoWrap; // la mayoria son labels cortos
            t.overflowMode = TextOverflowModes.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static GameObject NewButtonSet(string name, Transform parent, ButtonSpriteSet set, Sprite icon, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = set.normal;
            img.type = Image.Type.Sliced; // los sprites tienen borde 24, se escalan sin deformar
            img.pixelsPerUnitMultiplier = 1f;

            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.targetGraphic = img;
            var ss = new SpriteState
            {
                highlightedSprite = set.highlighted,
                pressedSprite = set.pressed,
                selectedSprite = set.highlighted,
                disabledSprite = set.disabled
            };
            btn.spriteState = ss;

            if (icon != null)
            {
                var ic = NewImage("Icon", go.transform, icon);
                StretchFill((RectTransform)ic.transform, new Vector2(-20f, -20f));
            }
            if (!string.IsNullOrEmpty(label))
            {
                var t = NewText("Label", go.transform, label, 22, TextAlignmentOptions.Center);
                StretchFill((RectTransform)t.transform);
                t.margin = new Vector4(0, 0, 0, 8);
            }
            return go;
        }

        private static void Anchor(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        private static void StretchFill(RectTransform rt, Vector2? offset = null)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = offset ?? Vector2.zero;
        }

        private static void StretchParent(RectTransform rt, Vector2 oversize)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = oversize;
        }

        private struct ButtonSpriteSet
        {
            public Sprite normal, highlighted, pressed, disabled;
            public ButtonSpriteSet(Sprite n, Sprite h, Sprite p, Sprite d)
            {
                normal = n; highlighted = h; pressed = p; disabled = d;
            }
        }

        private static readonly string[] UISheets = {
            "Assets/_Draft_2_Kikiri9/Art/UI/interfaz.png",
            "Assets/_Draft_2_Kikiri9/Art/UI/botones.png",
            "Assets/_Draft_2_Kikiri9/Art/UI/controles_tactiles.png",
            "Assets/_Draft_2_Kikiri9/Art/UI/estados_nivel.png",
            "Assets/_Draft_2_Kikiri9/Art/UI/selector_niveles.png",
        };

        private static Sprite FindUI(string spriteName)
        {
            foreach (var sheet in UISheets)
            {
                var reps = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheet);
                foreach (var o in reps) if (o is Sprite s && s.name == spriteName) return s;
            }
            Debug.LogWarning($"[Kikiri-9] Sprite UI faltante: {spriteName}");
            return null;
        }
    }
}
