// Tool dedicado: SOLO importa el sprite pan_indicator.png como Sprite PPU 100 pivot center.
// No toca escenas, prefabs, HUD, UI_Root ni ningun otro asset. Idempotente.
// Los HUD badges (PanBombaHUDBadge y PanBarredorHUDBadge) cargan este sprite por
// Resources.Load en runtime, asi que al correr este tool y recargar escena, veras el
// icono en los contadores de pan bomba y pan barredor.

using UnityEditor;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class PanIndicatorAssetImporter
    {
        private const string SpritePath = "Assets/_Project/Resources/PowerUps/pan_indicator.png";

        [MenuItem("Tools/Kikiri-9/Pan Indicator - Import Sprite Only")]
        public static void Import()
        {
            var ti = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            if (ti == null)
            {
                Debug.LogError($"[Kikiri-9] No se encontro {SpritePath}");
                return;
            }

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100f;
            ti.filterMode = FilterMode.Bilinear;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 2048;

            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);

            ti.SaveAndReimport();
            AssetDatabase.Refresh();
            Debug.Log("[Kikiri-9] pan_indicator.png importado como Sprite. Los HUD badges lo recogen automaticamente.");
        }
    }
}
