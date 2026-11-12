// V2 del installer de PowerUps. Fixes:
//  - Pickup_BreadBoost: escala 0.3 (el cuerpo del pan queda del tamaño del pan normal)
//  - Elimina PunchChargeIndicatorInstaller del Session (el "K" cuadrado viejo)
//  - HUD del pan dorado mas pequeño y arriba a la derecha (no en centro)
//  - Boton de rafaga mas chico (130x130) y bien pegado a la esquina

using Kikiri9.Player;
using Kikiri9.Visuales;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.EditorTools
{
    public static class PowerUpsInstaller
    {
        private const string PanDoradoDir = "Assets/_Project/Art/PowerUps/PanDorado";
        private const string RafagaDir = "Assets/_Project/Art/PowerUps/Rafaga";

        private const string PrefabBreadBoost = "Assets/_Project/Prefabs/Pickups/Pickup_BreadBoost.prefab";

        [MenuItem("Tools/Kikiri-9/PowerUps/Install All")]
        public static void InstallAll()
        {
            ApplyImports();
            UpdateBreadBoostPrefab();

            string[] scenes = {
                "Assets/_Project/Scenes/Level01_Dolores.unity",
                "Assets/_Project/Scenes/Level02_Dolores.unity",
                "Assets/_Project/Scenes/Level03_Guanajuato.unity",
            };
            foreach (var s in scenes) InstallInScene(s);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Kikiri-9] PowerUps v2 instalados en los 3 niveles.");
        }

        private static void ApplyImports()
        {
            string[] panFiles = {
                "pan_dorado_infinito.png","halo_dorado.png",
                "indicador_marco.png","mascara_barra.png","barra_dorada.png",
            };
            foreach (var f in panFiles) ImportSprite($"{PanDoradoDir}/{f}", 100f, FilterMode.Point, 2048);

            string[] rafFiles = {
                "boton_listo.png","boton_pulsado.png","mascara_recarga.png","estela_derecha.png",
            };
            foreach (var f in rafFiles) ImportSprite($"{RafagaDir}/{f}", 100f, FilterMode.Bilinear, 2048);
        }

        private static void ImportSprite(string path, float ppu, FilterMode filter, int maxSize)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = ppu;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = maxSize;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.filterMode = filter;

            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            ti.SetTextureSettings(settings);
            ti.SaveAndReimport();
        }

        private static void UpdateBreadBoostPrefab()
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBreadBoost);
            if (src == null) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(src);

            var oldAura = instance.GetComponent<Kikiri9.World.BreadBoostAura>();
            if (oldAura != null) Object.DestroyImmediate(oldAura);

            // Quitar todos los hijos viejos (Sparkle_*, Halo, Aura)
            for (int i = instance.transform.childCount - 1; i >= 0; i--)
            {
                var child = instance.transform.GetChild(i).gameObject;
                if (child.name.StartsWith("Sparkle_") || child.name == "Aura" || child.name == "Halo")
                    Object.DestroyImmediate(child);
            }

            var panInfSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{PanDoradoDir}/pan_dorado_infinito.png");
            var mainSr = instance.GetComponentInChildren<SpriteRenderer>();
            if (mainSr != null && panInfSprite != null)
            {
                mainSr.sprite = panInfSprite;
                mainSr.color = Color.white;
            }
            // Escala total: 0.3 para que el cuerpo del pan quede del tamaño del pan normal
            instance.transform.localScale = Vector3.one * 0.3f;

            PrefabUtility.SaveAsPrefabAsset(instance, PrefabBreadBoost);
            Object.DestroyImmediate(instance);
        }

        private static void InstallInScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            RemoveOldPunchIndicator();
            BuildRafagaButton();
            BuildPanDoradoHUD();
            AttachEstelaToPollo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void RemoveOldPunchIndicator()
        {
            // El K cuadrado viejo
            var oldCanvas = GameObject.Find("PunchChargeCanvas");
            if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

            // El installer en el Session (que lo recreaba cada frame)
            var session = GameObject.Find("Session");
            if (session != null)
            {
                var inst = session.GetComponent<Kikiri9.UI.PunchChargeIndicatorInstaller>();
                if (inst != null) Object.DestroyImmediate(inst);
            }
        }

        private static void BuildRafagaButton()
        {
            var oldRafaga = GameObject.Find("RafagaCanvas");
            if (oldRafaga != null) Object.DestroyImmediate(oldRafaga);

            var listoSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{RafagaDir}/boton_listo.png");
            var pulsadoSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{RafagaDir}/boton_pulsado.png");
            var mascaraSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{RafagaDir}/mascara_recarga.png");

            var canvasGo = new GameObject("RafagaCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var btnGo = new GameObject("RafagaButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(canvasGo.transform, false);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0f, 0f);
            btnRt.anchorMax = new Vector2(0f, 0f);
            btnRt.pivot = new Vector2(0f, 0f);
            btnRt.anchoredPosition = new Vector2(60f, 60f);
            btnRt.sizeDelta = new Vector2(140f, 140f);
            var btnImg = btnGo.GetComponent<Image>();
            btnImg.sprite = listoSprite;
            btnImg.preserveAspect = true;
            var btn = btnGo.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;

            var maskGo = new GameObject("Recarga", typeof(RectTransform), typeof(Image));
            maskGo.transform.SetParent(btnGo.transform, false);
            var maskRt = maskGo.GetComponent<RectTransform>();
            maskRt.anchorMin = Vector2.zero; maskRt.anchorMax = Vector2.one;
            maskRt.offsetMin = Vector2.zero; maskRt.offsetMax = Vector2.zero;
            var maskImg = maskGo.GetComponent<Image>();
            maskImg.sprite = mascaraSprite;
            maskImg.color = new Color(0.078f, 0.047f, 0.149f, 0.85f);
            maskImg.raycastTarget = false;

            var visual = btnGo.AddComponent<KikiriRafagaVisual>();
            var so = new SerializedObject(visual);
            so.FindProperty("boton").objectReferenceValue = btn;
            so.FindProperty("imagenBoton").objectReferenceValue = btnImg;
            so.FindProperty("mascaraRecarga").objectReferenceValue = maskImg;
            so.FindProperty("listo").objectReferenceValue = listoSprite;
            so.FindProperty("pulsado").objectReferenceValue = pulsadoSprite;
            so.ApplyModifiedProperties();

            canvasGo.AddComponent<Kikiri9.UI.RafagaVisualDriver>();
            btnGo.AddComponent<Kikiri9.UI.RafagaButtonHook>();
        }

        private static void BuildPanDoradoHUD()
        {
            var marco = AssetDatabase.LoadAssetAtPath<Sprite>($"{PanDoradoDir}/indicador_marco.png");
            var mascara = AssetDatabase.LoadAssetAtPath<Sprite>($"{PanDoradoDir}/mascara_barra.png");
            var barra = AssetDatabase.LoadAssetAtPath<Sprite>($"{PanDoradoDir}/barra_dorada.png");

            var oldCanvas = GameObject.Find("InfiniteBreadBadgeCanvas");
            if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);
            var oldBadge = GameObject.Find("InfiniteBreadBadge");
            if (oldBadge != null) Object.DestroyImmediate(oldBadge);
            var oldHud = GameObject.Find("PanDoradoHUD");
            if (oldHud != null) Object.DestroyImmediate(oldHud);

            var canvasGo = new GameObject("PanDoradoHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 410;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var indicadorGo = new GameObject("Indicador", typeof(RectTransform), typeof(Image));
            indicadorGo.transform.SetParent(canvasGo.transform, false);
            var indRt = indicadorGo.GetComponent<RectTransform>();
            // Esquina superior derecha, debajo del timer
            indRt.anchorMin = new Vector2(1f, 1f);
            indRt.anchorMax = new Vector2(1f, 1f);
            indRt.pivot = new Vector2(1f, 1f);
            indRt.anchoredPosition = new Vector2(-50f, -210f);
            indRt.sizeDelta = new Vector2(240f, 112f);
            var indImg = indicadorGo.GetComponent<Image>();
            indImg.sprite = marco;
            indImg.preserveAspect = true;
            indImg.raycastTarget = false;

            Vector2 anchorMin = new Vector2(0.14117647f, 0.2375f);
            Vector2 anchorMax = new Vector2(0.85882353f, 0.3375f);

            var pistaGo = new GameObject("Pista", typeof(RectTransform), typeof(Image));
            pistaGo.transform.SetParent(indicadorGo.transform, false);
            var pistaRt = pistaGo.GetComponent<RectTransform>();
            pistaRt.anchorMin = anchorMin;
            pistaRt.anchorMax = anchorMax;
            pistaRt.offsetMin = Vector2.zero; pistaRt.offsetMax = Vector2.zero;
            var pistaImg = pistaGo.GetComponent<Image>();
            pistaImg.sprite = mascara;
            pistaImg.color = new Color(0.098f, 0.047f, 0.18f, 1f);
            pistaImg.raycastTarget = false;

            var rellenoGo = new GameObject("Relleno", typeof(RectTransform), typeof(Image));
            rellenoGo.transform.SetParent(indicadorGo.transform, false);
            var relRt = rellenoGo.GetComponent<RectTransform>();
            relRt.anchorMin = anchorMin;
            relRt.anchorMax = anchorMax;
            relRt.offsetMin = Vector2.zero; relRt.offsetMax = Vector2.zero;
            var relImg = rellenoGo.GetComponent<Image>();
            relImg.sprite = barra;
            relImg.color = Color.white;
            relImg.type = Image.Type.Filled;
            relImg.fillMethod = Image.FillMethod.Horizontal;
            relImg.fillOrigin = 0;
            relImg.fillAmount = 0f;
            relImg.raycastTarget = false;

            var visual = canvasGo.AddComponent<KikiriPanDoradoVisual>();
            var so = new SerializedObject(visual);
            so.FindProperty("indicador").objectReferenceValue = indicadorGo;
            so.FindProperty("relleno").objectReferenceValue = relImg;
            so.FindProperty("avisoFinal").floatValue = 3f;
            so.ApplyModifiedProperties();

            canvasGo.AddComponent<Kikiri9.UI.PanDoradoDriver>();

            // El indicador arranca oculto
            indicadorGo.SetActive(false);
        }

        private static void AttachEstelaToPollo()
        {
            var pollo = GameObject.FindGameObjectWithTag("Player");
            if (pollo == null) return;

            var oldEstela = pollo.transform.Find("Estela");
            if (oldEstela != null) Object.DestroyImmediate(oldEstela.gameObject);

            var estelaSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{RafagaDir}/estela_derecha.png");
            if (estelaSprite == null) return;

            var estela = new GameObject("Estela");
            estela.transform.SetParent(pollo.transform, false);
            // Encima del pollo (sobre el cuerpo), no detras
            estela.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            estela.transform.localScale = Vector3.one * 0.035f;
            var sr = estela.AddComponent<SpriteRenderer>();
            sr.sprite = estelaSprite;
            sr.color = new Color(1f, 0.75f, 0.25f, 1f);
            var polloSr = pollo.GetComponent<SpriteRenderer>();
            sr.sortingOrder = (polloSr != null ? polloSr.sortingOrder : 5) + 1;
            estela.SetActive(false);

            // Pulso suave para enfatizar el efecto durante la rafaga
            var pulso = estela.AddComponent<Kikiri9.Visuales.KikiriPulsoSuave>();
            var psoPulso = new SerializedObject(pulso);
            var ampProp = psoPulso.FindProperty("amplitud");
            if (ampProp != null) ampProp.floatValue = 0.08f;
            var cpsProp = psoPulso.FindProperty("ciclosPorSegundo");
            if (cpsProp != null) cpsProp.floatValue = 12f;
            psoPulso.ApplyModifiedProperties();

            var driver = pollo.GetComponent<Kikiri9.Player.EstelaRafagaDriver>();
            if (driver == null) driver = pollo.AddComponent<Kikiri9.Player.EstelaRafagaDriver>();
            var so = new SerializedObject(driver);
            so.FindProperty("estela").objectReferenceValue = estela;
            so.ApplyModifiedProperties();
        }
    }
}
