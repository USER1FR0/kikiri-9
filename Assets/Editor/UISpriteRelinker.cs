// Re-liga por NOMBRE las referencias de sprites del UI_Root.prefab, sin tocar
// posiciones, tamanios, colores ni jerarquia. Para usar despues de re-slicing
// si quedaron Images en blanco por GUIDs huerfanos.
//
// Menu: Tools > Kikiri-9 > Relink UI Sprites

using System.Collections.Generic;
using Kikiri9.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.EditorTools
{
    public static class UISpriteRelinker
    {
        private const string InterfazPath = "Assets/_Draft_2_Kikiri9/Art/UI/interfaz.png";
        private const string UIPrefabPath = "Assets/_Project/Prefabs/UI/UI_Root.prefab";

        // GameObject.name -> nombre de sub-sprite en interfaz.png
        private static readonly Dictionary<string, string> NameMap = new()
        {
            { "Heart_0", "corazon_lleno" },
            { "Heart_1", "corazon_lleno" },
            { "Heart_2", "corazon_lleno" },
            { "PlacaVidas", "placa_vidas" },
            { "PlacaPuntos", "placa_puntos" },
            { "PlacaTiempo", "placa_tiempo" },
            { "PauseButton", "base_pausa" },
            // Bread_Counter > Icon -> pan ; Coins_Counter > Icon -> botella_verde
            // Los Icon se resuelven por el padre, mas abajo.
        };

        [MenuItem("Tools/Kikiri-9/Relink UI Sprites")]
        public static void Relink()
        {
            var sprites = LoadInterfazSprites();
            if (sprites.Count == 0) { Debug.LogError("[Kikiri-9] interfaz.png sin sprites. Corre Slice Sprite Sheets primero."); return; }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(UIPrefabPath);
            if (prefab == null) { Debug.LogError($"[Kikiri-9] UI prefab missing: {UIPrefabPath}"); return; }

            // Trabajar sobre una instancia editable del prefab
            var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            int touched = 0;

            foreach (var img in root.GetComponentsInChildren<Image>(true))
            {
                string targetName = ResolveTargetSpriteName(img);
                if (targetName == null) continue;
                if (!sprites.TryGetValue(targetName, out var sp)) continue;
                if (img.sprite == sp) continue;
                img.sprite = sp;
                touched++;
            }

            // HUDController: sprites serializados (relleno + vacio)
            var hud = root.GetComponentInChildren<HUDController>(true);
            if (hud != null)
            {
                var so = new SerializedObject(hud);
                var full = so.FindProperty("heartFullSprite");
                var empty = so.FindProperty("heartEmptySprite");
                if (full != null && sprites.TryGetValue("corazon_lleno", out var sFull)) full.objectReferenceValue = sFull;
                if (empty != null && sprites.TryGetValue("corazon_vacio", out var sEmpty)) empty.objectReferenceValue = sEmpty;
                so.ApplyModifiedPropertiesWithoutUndo();
                touched++;
            }

            PrefabUtility.SaveAsPrefabAsset(root, UIPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Kikiri-9] Relink completado. Images actualizadas: {touched}");
        }

        private static string ResolveTargetSpriteName(Image img)
        {
            if (NameMap.TryGetValue(img.gameObject.name, out var n)) return n;
            // Casos con jerarquia: <Padre>/Icon
            if (img.gameObject.name == "Icon" && img.transform.parent != null)
            {
                string parent = img.transform.parent.name;
                if (parent == "Bread_Counter") return "pan";
                if (parent == "Coins_Counter") return "botella_verde";
                if (parent == "PauseButton") return "pausa";
            }
            return null;
        }

        private static Dictionary<string, Sprite> LoadInterfazSprites()
        {
            var dict = new Dictionary<string, Sprite>();
            var reps = AssetDatabase.LoadAllAssetRepresentationsAtPath(InterfazPath);
            foreach (var o in reps)
                if (o is Sprite s && !string.IsNullOrEmpty(s.name)) dict[s.name] = s;
            return dict;
        }
    }
}
