// Construye el prefab TouchControlsCanvas con los 3 botones tactiles en una
// posicion inicial razonable. Despues el usuario puede ARRASTRARLOS en el
// Inspector/Scene y guardar el prefab; el TouchControlsManager en runtime
// solo instancia el prefab tal cual esta en disco.
//
// Tools > Kikiri-9 > Build TouchControls Prefab
// (Reentrante: sobrescribe el prefab manteniendo el estilo pero RESETEANDO
//  las posiciones; usa con cuidado si ya acomodaste manualmente.)

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kikiri9.EditorTools
{
    public static class TouchControlsPrefabBuilder
    {
        private const string PrefabPath = "Assets/_Project/Resources/UI/TouchControlsCanvas.prefab";
        private const string SpriteSheet = "Assets/_Project/Resources/UI/controles_tactiles.png";

        [MenuItem("Tools/Kikiri-9/Build TouchControls Prefab")]
        public static void Build()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));

            var canvasGo = new GameObject("TouchControlsCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            BuildButton(canvasGo.transform, "TouchSlide", "deslizar_normal", "deslizar_presionado",
                new Vector2(1f, 0f), new Vector2(-340f, 60f), new Vector2(150f, 150f));
            BuildButton(canvasGo.transform, "TouchThrow", "lanzar_normal", "lanzar_presionado",
                new Vector2(1f, 0f), new Vector2(-340f, 230f), new Vector2(150f, 150f));
            BuildButton(canvasGo.transform, "TouchJump", "saltar_normal", "saltar_presionado",
                new Vector2(1f, 0f), new Vector2(-180f, 160f), new Vector2(170f, 170f));

            var prefab = PrefabUtility.SaveAsPrefabAsset(canvasGo, PrefabPath);
            Object.DestroyImmediate(canvasGo);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Kikiri-9",
                "Prefab TouchControlsCanvas creado en:\n" + PrefabPath +
                "\n\nAhora puedes abrirlo (doble clic en el prefab) y arrastrar los 3 botones a la posicion que quieras.",
                "OK");
            Selection.activeObject = prefab;
        }

        private static GameObject BuildButton(Transform parent, string name,
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

        private static Sprite LoadSprite(string name)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(SpriteSheet);
            foreach (var o in all)
                if (o is Sprite sp && sp.name == name) return sp;
            return null;
        }
    }
}
