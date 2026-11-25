// Importa los 5 PNG panoramicos de kikiri9_fondos_adaptativos (2172x724) con los
// ajustes del PDF: Sprite Single, PPU 100, Full Rect, Center, Max 4096, Clamp,
// sin compresion, Non Power of 2: None.
//
// Menu: Tools > Kikiri-9 > Import Backgrounds V3

using UnityEditor;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class BackgroundsV3Importer
    {
        public const string Dir = "Assets/_Project/Art/Backgrounds_V3";
        public static readonly string[] Files = {
            "cielo_panorama.png",
            "dolores_horizonte.png",
            "dolores_plaza.png",
            "guanajuato_horizonte.png",
            "guanajuato_callejon.png",
        };

        [MenuItem("Tools/Kikiri-9/Import Backgrounds V3")]
        public static void ImportAll()
        {
            foreach (var f in Files) ApplySettings($"{Dir}/{f}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Kikiri-9] Backgrounds V3 importados.");
        }

        private static void ApplySettings(string path)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[Kikiri-9] Falta: {path}"); return; }

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100f;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 4096;
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
