// Importa y slicea los assets del paquete "enemigos_oleadas":
//  - Sprite sheets de mariachi, zombi_saltarin, xolo (1 fila horizontal)
//  - Banners UI oleada
//  - VFX combo_destello
//  - Audio SFX y musica
//
// Pivot (0.5, 0.125) para los enemigos, (0.5, 0.5) para VFX y UI.
// PPU 100 en todo.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class NewContentImporter
    {
        private const string EnemiesDir = "Assets/_Project/Art/Enemies";
        private const string UIWavesDir = "Assets/_Project/Art/UI/Waves";
        private const string VFXDir = "Assets/_Project/Art/VFX";
        private const string AudioSFXDir = "Assets/_Project/Audio/SFX";
        private const string AudioMusicDir = "Assets/_Project/Audio/Music";

        private struct SheetSpec
        {
            public string path;
            public int cellW;
            public int cellH;
            public int cols;
            public int rows;
            public Vector2 pivot;
            public SheetSpec(string p, int w, int h, int cols, int rows, Vector2 pivot)
            {
                path = p; cellW = w; cellH = h; this.cols = cols; this.rows = rows; this.pivot = pivot;
            }
        }

        [MenuItem("Tools/Kikiri-9/Import New Content (Enemies + Oleadas + Audio)")]
        public static void ImportAll()
        {
            var sheets = new List<SheetSpec>
            {
                new SheetSpec($"{EnemiesDir}/mariachi/float_idle.png", 96, 96, 6, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/mariachi/attack.png",     96, 96, 4, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/mariachi/defeat.png",     96, 96, 5, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/mariachi/hit_flash.png",  96, 96, 2, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/mariachi/pose.png",       96, 96, 1, 1, new Vector2(0.5f, 0.125f)),

                new SheetSpec($"{EnemiesDir}/zombi_saltarin/crouch_idle.png",  96, 96, 4, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/zombi_saltarin/jump_prepare.png", 96, 96, 3, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/zombi_saltarin/jump_air.png",     96, 96, 2, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/zombi_saltarin/land.png",         96, 96, 2, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/zombi_saltarin/defeat.png",       96, 96, 4, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/zombi_saltarin/hit_flash.png",    96, 96, 1, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/zombi_saltarin/pose.png",         96, 96, 1, 1, new Vector2(0.5f, 0.125f)),

                new SheetSpec($"{EnemiesDir}/xolo/run.png",        80, 48, 6, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/xolo/defeat.png",     80, 48, 4, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/xolo/hit_flash.png",  80, 48, 1, 1, new Vector2(0.5f, 0.125f)),
                new SheetSpec($"{EnemiesDir}/xolo/pose.png",       80, 48, 1, 1, new Vector2(0.5f, 0.125f)),

                new SheetSpec($"{VFXDir}/combo_destello.png", 128, 64, 5, 1, new Vector2(0.5f, 0.5f)),
                new SheetSpec("Assets/_Project/Resources/VFX/combo_destello.png", 128, 64, 5, 1, new Vector2(0.5f, 0.5f)),
            };

            foreach (var s in sheets) SliceSheet(s);

            // UI banners (single sprite)
            ImportSingle($"{UIWavesDir}/oleada_inicio.png", new Vector2(0.5f, 0.5f));
            ImportSingle($"{UIWavesDir}/oleada_completa.png", new Vector2(0.5f, 0.5f));

            ImportAudio(AudioSFXDir, loop: false);
            ImportAudio(AudioMusicDir, loop: true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Kikiri-9] Nuevos assets importados y sliceados.");
        }

        private static void SliceSheet(SheetSpec s)
        {
            var ti = AssetImporter.GetAtPath(s.path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"No importer para {s.path}"); return; }

            ti.textureType = TextureImporterType.Sprite;
            ti.spritePixelsPerUnit = 100f;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 2048;

            if (s.cols * s.rows > 1)
            {
                ti.spriteImportMode = SpriteImportMode.Multiple;
                var rects = new List<SpriteMetaData>();
                string name = System.IO.Path.GetFileNameWithoutExtension(s.path);
                int idx = 0;
                // Las hojas estan en 1 fila horizontal; si rows > 1 iteramos tambien filas.
                for (int row = 0; row < s.rows; row++)
                {
                    for (int col = 0; col < s.cols; col++)
                    {
                        var md = new SpriteMetaData
                        {
                            name = $"{name}_{idx:00}",
                            rect = new Rect(col * s.cellW, (s.rows - 1 - row) * s.cellH, s.cellW, s.cellH),
                            alignment = (int)SpriteAlignment.Custom,
                            pivot = s.pivot,
                        };
                        rects.Add(md);
                        idx++;
                    }
                }
                ti.spritesheet = rects.ToArray();
            }
            else
            {
                ti.spriteImportMode = SpriteImportMode.Single;
                var settings = new TextureImporterSettings();
                ti.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = s.pivot;
                settings.spriteMeshType = SpriteMeshType.FullRect;
                ti.SetTextureSettings(settings);
            }

            ti.SaveAndReimport();
            Debug.Log($"sliced {s.path} ({s.cols}x{s.rows})");
        }

        private static void ImportSingle(string path, Vector2 pivot)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;
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
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
            ti.SaveAndReimport();
        }

        private static void ImportAudio(string dir, bool loop)
        {
            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { dir });
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var ai = AssetImporter.GetAtPath(path) as AudioImporter;
                if (ai == null) continue;
                var set = ai.defaultSampleSettings;
                set.loadType = loop ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
                set.compressionFormat = AudioCompressionFormat.Vorbis;
                set.quality = 0.7f;
                ai.defaultSampleSettings = set;
                ai.SaveAndReimport();
            }
        }
    }
}
