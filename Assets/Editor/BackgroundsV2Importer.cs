// Importa los 5 PNG de kikiri9_fondos_parallax_completo con los ajustes del PDF:
// Sprite Single, PPU 100, Center, FullRect, MaxSize 2048, sin compresion, Clamp.
//
// Menu: Tools > Kikiri-9 > Import Backgrounds V2

using UnityEditor;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class BackgroundsV2Importer
    {
        private const string Dir = "Assets/_Project/Art/Backgrounds_V2";
        private static readonly string[] Files = {
            "cielo_luna_tormenta.png",
            "dolores_hidalgo_horizonte.png",
            "dolores_hidalgo_plaza.png",
            "guanajuato_horizonte.png",
            "guanajuato_callejon.png",
        };

        [MenuItem("Tools/Kikiri-9/Import Backgrounds V2")]
        public static void ImportAll()
        {
            foreach (var f in Files) ApplySettings($"{Dir}/{f}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Kikiri-9] Backgrounds V2 importados.");
        }

        private static void ApplySettings(string path)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[Kikiri-9] Falta: {path}"); return; }

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100f;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 2048;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.filterMode = FilterMode.Bilinear;

            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            ti.SetTextureSettings(settings);

            ti.SaveAndReimport();
        }
    }
}
