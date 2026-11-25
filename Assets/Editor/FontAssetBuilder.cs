// Generates a TMP Font Asset from PressStart2P-Regular.ttf. Pre-generates glyphs and
// persists the atlas TEXTURES (plural, since TMP keeps an array) as sub-assets.

using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Kikiri9.EditorTools
{
    public static class FontAssetBuilder
    {
        private const string TtfPath = "Assets/_Project/Fonts/PressStart2P-Regular.ttf";
        public const string FontAssetPath = "Assets/_Project/Fonts/PressStart2P SDF.asset";

        private const string PreloadChars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
            " .,:;!?/-_+()[]|@#$%&*'\"<>=" +
            "áéíóúÁÉÍÓÚñÑ¿¡";

        [MenuItem("Tools/Kikiri-9/Build Font Asset")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(TtfPath);
            if (font == null) { Debug.LogError($"[Kikiri-9] TTF missing at {TtfPath}"); return; }

            Directory.CreateDirectory(Path.GetDirectoryName(FontAssetPath));

            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath) != null)
                AssetDatabase.DeleteAsset(FontAssetPath);

            var asset = TMP_FontAsset.CreateFontAsset(
                font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                AtlasPopulationMode.Dynamic, true);
            asset.name = "PressStart2P SDF";

            AssetDatabase.CreateAsset(asset, FontAssetPath);

            // Bakear los glifos (crea atlas texture en RAM)
            asset.TryAddCharacters(PreloadChars);
            asset.ReadFontAssetDefinition();

            // Persistir TODAS las atlas textures como subassets. TMP 3.2 usa array.
            if (asset.atlasTextures != null)
            {
                for (int i = 0; i < asset.atlasTextures.Length; i++)
                {
                    var tex = asset.atlasTextures[i];
                    if (tex == null) continue;
                    tex.name = $"{asset.name} Atlas {i}";
                    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(tex)))
                        AssetDatabase.AddObjectToAsset(tex, asset);
                    EditorUtility.SetDirty(tex);
                }
            }

            // Guardar material compartido como subasset
            if (asset.material != null)
            {
                asset.material.name = $"{asset.name} Material";
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset.material)))
                    AssetDatabase.AddObjectToAsset(asset.material, asset);
                EditorUtility.SetDirty(asset.material);
            }

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(FontAssetPath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh();

            int glyphs = asset.characterTable != null ? asset.characterTable.Count : 0;
            int atlases = asset.atlasTextures != null ? asset.atlasTextures.Length : 0;
            Debug.Log($"[Kikiri-9] Font asset creado: {glyphs} glyphs en {atlases} atlas texture(s), persistidas como subassets.");
        }
    }
}
