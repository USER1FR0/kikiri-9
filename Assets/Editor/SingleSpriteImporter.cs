// Configures Single sprites with their exact import settings per asset pack README.
// Covers both backgrounds + ground + the single UI pieces (panel_resultados, barra_marco,
// barra_relleno). All PPU 128, Point filter, no mipmaps, no compression.

using UnityEditor;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class SingleSpriteImporter
    {
        private const string Dolores = "Assets/_Draft_2_Kikiri9/Art/Backgrounds/dolores_hidalgo.png";
        private const string Guanajuato = "Assets/_Draft_2_Kikiri9/Art/Backgrounds/guanajuato.png";
        private const string Suelo = "Assets/_Draft_2_Kikiri9/Art/Environment/suelo.png";
        private const string PanelResultados = "Assets/_Draft_2_Kikiri9/Art/UI/panel_resultados.png";
        private const string BarraMarco = "Assets/_Draft_2_Kikiri9/Art/UI/barra_marco.png";
        private const string BarraRelleno = "Assets/_Draft_2_Kikiri9/Art/UI/barra_relleno.png";

        [MenuItem("Tools/Kikiri-9/Configure Single Sprites")]
        public static void Configure()
        {
            // Backgrounds + suelo
            Apply(Dolores, new Vector2(0.5f, 0.5f), 128f, Vector4.zero);
            Apply(Guanajuato, new Vector2(0.5f, 0.5f), 128f, Vector4.zero);
            Apply(Suelo, new Vector2(0.5f, 1.0f), 128f, Vector4.zero);
            Apply("Assets/_Draft_2_Kikiri9/Art/Backgrounds/cielo.png", new Vector2(0.5f, 0.5f), 128f, Vector4.zero);

            // UI singles (PPU 100 segun README, con 9-slice en panel)
            Apply(PanelResultados, new Vector2(0.5f, 0.5f), 100f, new Vector4(20, 20, 20, 20));
            Apply(BarraMarco, new Vector2(0.5f, 0.5f), 100f, Vector4.zero);
            Apply(BarraRelleno, new Vector2(0.5f, 0.5f), 100f, Vector4.zero);

            AssetDatabase.Refresh();
            Debug.Log("[Kikiri-9] Single sprites configurados.");
        }

        private static void Apply(string path, Vector2 pivot, float ppu, Vector4 border)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogError($"[Kikiri-9] Importer missing for {path}"); return; }

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePivot = pivot;
            ti.spritePixelsPerUnit = ppu;
            ti.spriteBorder = border;
            ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.maxTextureSize = 2048;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spritePixelsPerUnit = ppu;
            settings.spriteBorder = border;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);

            ti.SaveAndReimport();
        }
    }
}
