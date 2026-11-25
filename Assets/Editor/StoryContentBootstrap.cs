// Installer de narrativa + creditos (#14).
// - 9 StoryScreenDefinition (01-08 + puente L1-L2), con textos paginados cuando
//   el texto no cabe en el contenedor.
// - StoryFlow.asset con la distribucion por nivel.
// - StoryScene con panel compacto, 3 botones (Prev, Next pagina, Continuar)
//   y sprite primario_normal (mismo estilo que MainMenu).
// - CreditsScene con VOLVER AL MENU del mismo estilo.
// Reentrante. NO toca LevelDefinition, SaveService ni Level##.asset de niveles.

using System.Collections.Generic;
using System.IO;
using Kikiri9.UI.Story;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kikiri9.EditorTools
{
    public static class StoryContentBootstrap
    {
        private const string StoryResourcesDir = "Assets/_Project/Resources/Story";
        private const string ScenesDir = "Assets/_Project/Scenes";
        private const string StorySpritesDir = "Assets/_Project/Art/Story";
        private const string UiSpritesDir = "Assets/_Project/Art/UI/Story";
        private const string ElectricidadPath = "Assets/_Project/Art/VFX/Electricidad/electricidad_activacion_cempasuchil.png";
        private const string PanelPath = UiSpritesDir + "/panel_resultados.png";
        private const string ButtonsPath = UiSpritesDir + "/botones.png";

        [MenuItem("Tools/Kikiri-9/Build Story + Credits")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(StoryResourcesDir);
            Directory.CreateDirectory(ScenesDir);

            EnsureElectricidadSliced();
            EnsureUiSlicedAndImported();

            BuildUIAssets();
            var screens = BuildScreens();
            BuildFlow(screens);

            BuildStoryScene();
            BuildCreditsScene();
            AddScenesToBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Kikiri-9", "Historia + Creditos reconstruidos.", "OK");
        }

        private static void EnsureElectricidadSliced()
        {
            var txImp = AssetImporter.GetAtPath(ElectricidadPath) as TextureImporter;
            if (txImp == null) return;
            txImp.textureType = TextureImporterType.Sprite;
            txImp.spriteImportMode = SpriteImportMode.Multiple;
            txImp.filterMode = FilterMode.Point;
            txImp.SaveAndReimport();
        }

        private static void EnsureUiSlicedAndImported()
        {
            var panelImp = AssetImporter.GetAtPath(PanelPath) as TextureImporter;
            if (panelImp != null)
            {
                panelImp.textureType = TextureImporterType.Sprite;
                panelImp.spriteImportMode = SpriteImportMode.Single;
                panelImp.filterMode = FilterMode.Bilinear;
                var settings = new TextureImporterSettings();
                panelImp.ReadTextureSettings(settings);
                settings.spriteBorder = new Vector4(60f, 60f, 60f, 60f);
                panelImp.SetTextureSettings(settings);
                panelImp.SaveAndReimport();
            }

            // Respetar el slicing que ya viene del meta sincronizado con el draft
            // (8 sprites: primarios naranja en fila superior, secundarios morado en fila inferior).
            var btnImp = AssetImporter.GetAtPath(ButtonsPath) as TextureImporter;
            if (btnImp != null)
            {
                btnImp.textureType = TextureImporterType.Sprite;
                btnImp.spriteImportMode = SpriteImportMode.Multiple;
                btnImp.filterMode = FilterMode.Point;
            }
        }

        private static Sprite LoadStoryBg(string file) =>
            AssetDatabase.LoadAssetAtPath<Sprite>($"{StorySpritesDir}/{file}");

        private static Sprite LoadPanelSprite() =>
            AssetDatabase.LoadAssetAtPath<Sprite>(PanelPath);

        private static Sprite LoadButtonSprite(string spriteName)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(ButtonsPath);
            foreach (var o in all)
                if (o is Sprite s && s.name == spriteName) return s;
            return null;
        }

        private static Sprite[] LoadElectricidadFrames(string prefix)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(ElectricidadPath);
            var list = new List<Sprite>();
            foreach (var o in all)
                if (o is Sprite s && s.name.StartsWith(prefix)) list.Add(s);
            list.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            return list.ToArray();
        }

        private static StoryScreenDefinition GetOrCreate(string assetName)
        {
            string path = $"{StoryResourcesDir}/{assetName}.asset";
            var def = AssetDatabase.LoadAssetAtPath<StoryScreenDefinition>(path);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<StoryScreenDefinition>();
                AssetDatabase.CreateAsset(def, path);
            }
            return def;
        }

        private static StoryScreenDefinition[] BuildScreens()
        {
            var relampago = LoadElectricidadFrames("relampago");
            var activacion = LoadElectricidadFrames("activacion");
            var cempasuchil = LoadElectricidadFrames("cempasuchil");

            var s01 = GetOrCreate("Screen01_Tormenta");
            s01.background = LoadStoryBg("01_tormenta_guanajuato.png");
            s01.textEs = "";
            s01.pages = new[] {
                "Dolores Hidalgo. Noche del uno de noviembre.\n\nLa tormenta lleva horas sin ceder.\nEn cada ofrenda del pueblo falta lo mismo:\nel pan de muerto.",
                "Y bajo la tierra...\nalgo empieza a moverse."
            };
            s01.effects = new[]
            {
                new StoryEffect { id = "rayo_izq", frames = relampago, frameRate = 10f, loop = true, anchor = new Vector2(0.22f, 0.78f), size = new Vector2(320f, 480f), tint = new Color(1f, 1f, 1f, 0.9f), randomRepeatInterval = 3.5f },
                new StoryEffect { id = "rayo_der", frames = relampago, frameRate = 10f, loop = true, anchor = new Vector2(0.78f, 0.72f), size = new Vector2(280f, 420f), tint = new Color(0.9f, 0.9f, 1f, 0.85f), startDelay = 1.2f, randomRepeatInterval = 4.5f }
            };
            EditorUtility.SetDirty(s01);

            var s02 = GetOrCreate("Screen02_Laboratorio");
            s02.background = LoadStoryBg("02_laboratorio_inactivo.png");
            s02.textEs = "";
            s02.pages = new[] {
                "Al otro lado del pueblo,\nen un laboratorio apagado,\nun pollo mecanico duerme.",
                "Lo construyeron para correr mas rapido\nque cualquier cosa viva.\n\nLo apagaron antes de probarlo.\nLo olvidaron."
            };
            s02.effects = new[]
            {
                new StoryEffect { id = "cempasuchil_lab", frames = cempasuchil, frameRate = 4f, loop = true, anchor = new Vector2(0.12f, 0.22f), size = new Vector2(160f, 160f), tint = new Color(1f, 0.85f, 0.5f, 0.55f) }
            };
            EditorUtility.SetDirty(s02);

            var s03 = GetOrCreate("Screen03_Activacion");
            s03.background = LoadStoryBg("03_activacion_kikiri9.png");
            s03.textEs = "";
            s03.pages = new[] {
                "Un rayo cae en el pararrayos.\n\nLa corriente baja por los cables.\nEncuentra la jaula.\nEncuentra el circuito que nadie reactivo.",
                "Kikiri-9 abre los ojos.\n\nAfuera, los muertos tambien."
            };
            s03.effects = new[]
            {
                new StoryEffect { id = "rayo_centro", frames = relampago, frameRate = 12f, loop = true, anchor = new Vector2(0.5f, 0.78f), size = new Vector2(380f, 560f), tint = Color.white, randomRepeatInterval = 2.2f },
                new StoryEffect { id = "activacion_cuerpo", frames = activacion, frameRate = 10f, loop = true, anchor = new Vector2(0.52f, 0.42f), size = new Vector2(320f, 320f), tint = new Color(1f, 1f, 0.6f, 0.95f) },
                new StoryEffect { id = "cempasuchil_fuerte", frames = cempasuchil, frameRate = 7f, loop = true, anchor = new Vector2(0.14f, 0.28f), size = new Vector2(200f, 200f), tint = new Color(1f, 0.7f, 0.2f, 1f) }
            };
            EditorUtility.SetDirty(s03);

            var s04 = GetOrCreate("Screen04_Fuga");
            s04.background = LoadStoryBg("04_fuga_campus.png");
            s04.textEs = "";
            s04.pages = new[] {
                "Dolores ya no es Dolores.\n\nZombis en las bardas.\nZombis en los puestos de ceramica.\nZombis hasta en la parroquia.",
                "Una mochila olvidada en el suelo.\nDentro, panes de muerto envueltos en papel.\n\nAlgo le dice que valen\nmas que cualquier cosa."
            };
            EditorUtility.SetDirty(s04);

            var s05 = GetOrCreate("Screen05_DescubrimientoPan");
            s05.background = LoadStoryBg("05_descubrimiento_pan.png");
            s05.textEs = "";
            s05.pages = new[] {
                "Lanza el primero por accidente.\n\nEl zombi que lo acechaba se detiene.\nDobla las rodillas.\nSe deshace en polvo.",
                "El cempasuchil del suelo brilla mas fuerte.\n\nEl pan no es comida.\nEs lo unico que puede devolverlos\na donde deben estar."
            };
            s05.effects = new[]
            {
                new StoryEffect { id = "cempasuchil_descubrir", frames = cempasuchil, frameRate = 6f, loop = true, anchor = new Vector2(0.16f, 0.18f), size = new Vector2(200f, 200f), tint = new Color(1f, 0.75f, 0.25f, 1f) }
            };
            EditorUtility.SetDirty(s05);

            // PUENTE L1-L2
            var s05b = GetOrCreate("Screen05b_Puente");
            s05b.background = LoadStoryBg("01_tormenta_guanajuato.png");
            s05b.textEs = "";
            s05b.pages = new[] {
                "Dolores cambia.\n\nLos que cayeron primero\nempiezan a levantarse distintos.",
                "Algunos ya no mueren de un solo pan.\nOtros brincan paredes\nque antes los contenian.",
                "Kikiri-9 va a tener que correr mas rapido.\nY pegar mas fuerte."
            };
            s05b.effects = new[]
            {
                new StoryEffect { id = "rayos_puente", frames = relampago, frameRate = 11f, loop = true, anchor = new Vector2(0.5f, 0.75f), size = new Vector2(380f, 560f), tint = new Color(1f, 0.95f, 0.85f, 0.85f), randomRepeatInterval = 2.8f }
            };
            EditorUtility.SetDirty(s05b);

            var s06 = GetOrCreate("Screen06_Transicion");
            s06.background = LoadStoryBg("06_transicion_ciudades.png");
            s06.textEs = "";
            s06.pages = new[] {
                "Sale de Dolores por la carretera vieja.\n\nEl camino sube entre cerros.\nLa lluvia no lo sigue.",
                "Al fondo, los callejones de Guanajuato\nencienden sus faroles uno por uno.\n\nAlguien lo espera alla.\nTodavia no sabe quien."
            };
            EditorUtility.SetDirty(s06);

            var s07 = GetOrCreate("Screen07_Coronela");
            s07.background = LoadStoryBg("07_presentacion_coronela.png");
            s07.textEs = "";
            s07.pages = new[] {
                "En lo alto del Pipila,\nuna silueta vigila la ciudad.\n\nLa llaman la Coronela.",
                "Dicen que fue la primera en caer esa noche.\nDicen que tambien fue la primera\nen levantarse.",
                "Tiene el ultimo altar que importa.\nY falta un pan."
            };
            s07.effects = new[]
            {
                new StoryEffect { id = "rayo_lejano", frames = relampago, frameRate = 8f, loop = true, anchor = new Vector2(0.85f, 0.82f), size = new Vector2(220f, 340f), tint = new Color(0.85f, 0.7f, 1f, 0.55f), randomRepeatInterval = 5f, startDelay = 1.5f }
            };
            EditorUtility.SetDirty(s07);

            var s08 = GetOrCreate("Screen08_Amanecer");
            s08.background = LoadStoryBg("08_amanecer_final.png");
            s08.textEs = "";
            s08.pages = new[] {
                "El Callejon del Beso se vacia.\n\nLa Coronela recibe el ultimo pan\ncon las dos manos.",
                "El amanecer entra por los callejones.\nLimpia la ciudad despacio.",
                "Kikiri-9 cierra los ojos.\n\nEsta vez, nadie va a volver a encenderlo.\nEsta vez, nadie necesita hacerlo."
            };
            s08.effects = new[]
            {
                new StoryEffect { id = "cempasuchil_final", frames = cempasuchil, frameRate = 3f, loop = true, anchor = new Vector2(0.5f, 0.2f), size = new Vector2(240f, 240f), tint = new Color(1f, 0.85f, 0.4f, 0.85f) }
            };
            EditorUtility.SetDirty(s08);

            return new[] { s01, s02, s03, s04, s05, s05b, s06, s07, s08 };
        }

        private static void BuildFlow(StoryScreenDefinition[] s)
        {
            string path = $"{StoryResourcesDir}/StoryFlow.asset";
            var flow = AssetDatabase.LoadAssetAtPath<StoryFlow>(path);
            if (flow == null)
            {
                flow = ScriptableObject.CreateInstance<StoryFlow>();
                AssetDatabase.CreateAsset(flow, path);
            }
            flow.beforeLevel1 = new[] { s[0], s[1], s[2], s[3], s[4] };
            flow.beforeLevel2 = new[] { s[5] };
            flow.beforeLevel3 = new[] { s[6] };
            flow.beforeLevel4 = new[] { s[7] };
            flow.endingScreens = new[] { s[8] };
            flow.creditsSceneName = "CreditsScene";
            EditorUtility.SetDirty(flow);
        }

        private static void BuildUIAssets()
        {
            string path = $"{StoryResourcesDir}/StoryUIAssets.asset";
            var asset = AssetDatabase.LoadAssetAtPath<StoryUIAssets>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<StoryUIAssets>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.buttonNormal = LoadButtonSprite("primario_normal");
            asset.buttonPressed = LoadButtonSprite("primario_presionado");
            asset.buttonSelected = LoadButtonSprite("primario_seleccionado");
            asset.buttonDisabled = LoadButtonSprite("primario_desactivado");

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/PressStart2P SDF.asset");
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            asset.font = font;
            EditorUtility.SetDirty(asset);
        }

        private static void BuildStoryScene()
        {
            string path = $"{ScenesDir}/StoryScene.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = NewCamera();
            NewEventSystem();
            var canvasGo = NewCanvas();

            var bgRootGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasGroup));
            bgRootGo.transform.SetParent(canvasGo.transform, false);
            Stretch((RectTransform)bgRootGo.transform);
            var bgFader = bgRootGo.GetComponent<CanvasGroup>();
            bgFader.alpha = 0f;

            var bgImgGo = new GameObject("BG_Image", typeof(RectTransform), typeof(Image));
            bgImgGo.transform.SetParent(bgRootGo.transform, false);
            Stretch((RectTransform)bgImgGo.transform);
            var bgImg = bgImgGo.GetComponent<Image>();
            bgImg.preserveAspect = false;
            bgImg.raycastTarget = false;

            var fxGo = new GameObject("Effects", typeof(RectTransform));
            fxGo.transform.SetParent(bgRootGo.transform, false);
            Stretch((RectTransform)fxGo.transform);

            // Panel dialogo compacto abajo
            var panelGo = new GameObject("DialogPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelRt = (RectTransform)panelGo.transform;
            panelRt.anchorMin = new Vector2(0.5f, 0f);
            panelRt.anchorMax = new Vector2(0.5f, 0f);
            panelRt.pivot = new Vector2(0.5f, 0f);
            panelRt.anchoredPosition = new Vector2(0f, 40f);
            panelRt.sizeDelta = new Vector2(1200f, 300f);
            var panelImg = panelGo.GetComponent<Image>();
            panelImg.sprite = LoadPanelSprite();
            panelImg.type = Image.Type.Sliced;
            panelImg.color = new Color(1f, 1f, 1f, 0.95f);
            panelImg.raycastTarget = false;
            var textFader = panelGo.GetComponent<CanvasGroup>();
            textFader.alpha = 1f;

            // Area de texto DENTRO del panel, con margenes para dejar sitio a botones
            var textGo = new GameObject("Narrative", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(panelGo.transform, false);
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = new Vector2(0f, 0f);
            textRt.anchorMax = new Vector2(1f, 1f);
            textRt.offsetMin = new Vector2(90f, 90f);
            textRt.offsetMax = new Vector2(-90f, -40f);
            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.text = "";
            text.fontSize = 28f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.98f, 0.94f, 0.82f);
            text.enableWordWrapping = true;
            text.lineSpacing = 6f;
            text.fontStyle = FontStyles.Bold;
            text.outlineColor = new Color(0f, 0f, 0f, 0.9f);
            text.outlineWidth = 0.22f;

            // Prev en esquina inferior izquierda. Siguiente y Continuar COMPARTEN
            // el slot de esquina inferior derecha: solo uno visible segun aplique.
            var prev = CreateStyledButton(panelGo.transform, "PrevButton", "< ATRAS",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(24f, 20f), new Vector2(220f, 75f));
            var next = CreateStyledButton(panelGo.transform, "NextPageButton", "SIGUIENTE >",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-24f, 20f), new Vector2(260f, 75f));
            var cont = CreateStyledButton(panelGo.transform, "ContinueButton", "CONTINUAR",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-24f, 20f), new Vector2(260f, 75f));

            var ctrlGo = new GameObject("StoryController");
            var ctrl = ctrlGo.AddComponent<StorySceneController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("backgroundFader").objectReferenceValue = bgFader;
            so.FindProperty("textFader").objectReferenceValue = textFader;
            so.FindProperty("narrativeText").objectReferenceValue = text;
            so.FindProperty("continueButton").objectReferenceValue = cont.GetComponent<Button>();
            so.FindProperty("nextPageButton").objectReferenceValue = next.GetComponent<Button>();
            so.FindProperty("prevPageButton").objectReferenceValue = prev.GetComponent<Button>();
            so.FindProperty("effectsLayer").objectReferenceValue = (RectTransform)fxGo.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, path);
        }

        private static void BuildCreditsScene()
        {
            string path = $"{ScenesDir}/CreditsScene.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = NewCamera();
            NewEventSystem();
            var canvasGo = NewCanvas();

            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(canvasGo.transform, false);
            Stretch((RectTransform)bgGo.transform);
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{StorySpritesDir}/08_amanecer_final.png");
            bgImg.color = new Color(1f, 1f, 1f, 0.55f);
            bgImg.raycastTarget = false;

            var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dimGo.transform.SetParent(canvasGo.transform, false);
            Stretch((RectTransform)dimGo.transform);
            dimGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            dimGo.GetComponent<Image>().raycastTarget = false;

            var panelGo = new GameObject("CreditsPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelRt = (RectTransform)panelGo.transform;
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = Vector2.zero;
            panelRt.sizeDelta = new Vector2(900f, 780f);
            var panelImg = panelGo.GetComponent<Image>();
            panelImg.sprite = LoadPanelSprite();
            panelImg.type = Image.Type.Sliced;
            panelImg.raycastTarget = false;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportGo.transform.SetParent(panelGo.transform, false);
            var viewRt = (RectTransform)viewportGo.transform;
            viewRt.anchorMin = new Vector2(0f, 0f);
            viewRt.anchorMax = new Vector2(1f, 1f);
            viewRt.offsetMin = new Vector2(60f, 60f);
            viewRt.offsetMax = new Vector2(-60f, -60f);

            var contentGo = new GameObject("ScrollContent", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = (RectTransform)contentGo.transform;
            contentRt.anchorMin = new Vector2(0.5f, 0f);
            contentRt.anchorMax = new Vector2(0.5f, 0f);
            contentRt.pivot = new Vector2(0.5f, 0f);
            contentRt.anchoredPosition = new Vector2(0f, -1200f);
            contentRt.sizeDelta = new Vector2(760f, 1700f);

            var bodyGo = new GameObject("Body", typeof(RectTransform), typeof(TextMeshProUGUI));
            bodyGo.transform.SetParent(contentGo.transform, false);
            Stretch((RectTransform)bodyGo.transform);
            var body = bodyGo.GetComponent<TextMeshProUGUI>();
            body.text = "";
            body.fontSize = 34f;
            body.alignment = TextAlignmentOptions.Center;
            body.color = new Color(0.98f, 0.94f, 0.82f);
            body.enableWordWrapping = true;
            body.lineSpacing = 8f;
            body.fontStyle = FontStyles.Bold;
            body.outlineColor = new Color(0f, 0f, 0f, 0.9f);
            body.outlineWidth = 0.2f;

            var backGo = CreateStyledButton(canvasGo.transform, "BackButton", "VOLVER AL MENU",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(60f, 60f), new Vector2(380f, 110f));

            // Mensaje final fijo al centro del panel, oculto al inicio
            var endGo = new GameObject("EndMessage", typeof(RectTransform), typeof(TextMeshProUGUI));
            endGo.transform.SetParent(panelGo.transform, false);
            var endRt = (RectTransform)endGo.transform;
            endRt.anchorMin = new Vector2(0.5f, 0.5f);
            endRt.anchorMax = new Vector2(0.5f, 0.5f);
            endRt.pivot = new Vector2(0.5f, 0.5f);
            endRt.anchoredPosition = Vector2.zero;
            endRt.sizeDelta = new Vector2(780f, 120f);
            var endTxt = endGo.GetComponent<TextMeshProUGUI>();
            endTxt.text = "GRACIAS POR JUGAR";
            endTxt.fontSize = 56f;
            endTxt.alignment = TextAlignmentOptions.Center;
            endTxt.color = new Color(1f, 0.85f, 0.4f);
            endTxt.fontStyle = FontStyles.Bold;
            endTxt.outlineColor = new Color(0f, 0f, 0f, 0.95f);
            endTxt.outlineWidth = 0.3f;
            endGo.SetActive(false);

            var ctrlGo = new GameObject("CreditsController");
            var ctrl = ctrlGo.AddComponent<CreditsSceneController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("scrollContent").objectReferenceValue = contentRt;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("backButton").objectReferenceValue = backGo.GetComponent<Button>();
            so.FindProperty("viewport").objectReferenceValue = viewRt;
            so.FindProperty("endMessage").objectReferenceValue = endTxt;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, path);
        }

        // Boton con sprite primario_normal (mismo estilo que MainMenu) + SpriteSwap + label blanco outline.
        private static GameObject CreateStyledButton(Transform parent, string name, string labelText,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var normal = LoadButtonSprite("primario_normal");
            var pressed = LoadButtonSprite("primario_presionado");
            var selected = LoadButtonSprite("primario_seleccionado");
            var disabled = LoadButtonSprite("primario_desactivado");

            var img = go.GetComponent<Image>();
            img.sprite = normal;
            img.type = Image.Type.Simple;   // mismo render que el boton JUGAR del MainMenu
            img.preserveAspect = false;
            img.color = Color.white;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.spriteState = new SpriteState { highlightedSprite = selected, pressedSprite = pressed, disabledSprite = disabled };

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var lrt = (RectTransform)labelGo.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(14f, 8f);
            lrt.offsetMax = new Vector2(-14f, -8f);
            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.fontSize = 26f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.outlineColor = new Color(0.08f, 0.04f, 0.01f, 1f);
            tmp.outlineWidth = 0.28f;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 16f;
            tmp.fontSizeMax = 30f;

            return go;
        }

        // Helpers
        private static GameObject NewCamera()
        {
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            camGo.tag = "MainCamera";
            return camGo;
        }

        private static GameObject NewEventSystem()
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();
            return esGo;
        }

        private static GameObject NewCanvas()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvasGo;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void AddScenesToBuildSettings()
        {
            string[] toAdd = {
                "Assets/_Project/Scenes/StoryScene.unity",
                "Assets/_Project/Scenes/CreditsScene.unity",
                "Assets/_Project/Scenes/Level04_Guanajuato.unity"
            };

            var existing = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var existingPaths = new HashSet<string>();
            foreach (var s in existing) existingPaths.Add(s.path);

            foreach (var p in toAdd)
            {
                if (!File.Exists(p)) continue;
                if (existingPaths.Contains(p)) continue;
                existing.Add(new EditorBuildSettingsScene(p, true));
            }

            EditorBuildSettings.scenes = existing.ToArray();
        }
    }
}
