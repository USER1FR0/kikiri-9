// Editor-only utility to slice Kikiri-9 sprite sheets from Assets/_Draft_2_Kikiri9/Art/.
// Reads cell sizes and pivots from the known layout documented in Docs/medidas_recorte.csv.
// Run from menu: Tools > Kikiri-9 > Slice Sprite Sheets.
//
// Produces one sub-sprite per cell named "<row>_<col>" (col is 1-based, two digits).
// Row order follows top-to-bottom as rendered in the atlas; Unity's texture origin is
// bottom-left, so the slicer flips the row index when computing rect Y.
//
// Enemies and the projectile are authored facing left; sprites are kept as-is. Flip at
// runtime via SpriteRenderer.flipX when the agent needs to face the other direction.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class Kikiri9SheetSlicer
    {
        private const string RootPath = "Assets/_Draft_2_Kikiri9/Art";
        private const int PixelsPerUnit = 128;

        private struct SheetDef
        {
            public string relPath;
            public int cellW;
            public int cellH;
            public int cols;
            public int rows;
            public float pivotX;
            public float pivotY;
            public string[] rowNames;
            public string[,] cellNames;   // override por celda [row, col]; null = usa rowNames
            public Vector4 slice9Border;  // L B R T; Vector4.zero = sin 9-slice
        }

        private static readonly SheetDef[] Sheets =
        {
            new SheetDef
            {
                relPath = "Characters/kikiri9.png",
                cellW = 512, cellH = 384, cols = 6, rows = 4,
                pivotX = 0.5f, pivotY = 0.125f,
                rowNames = new[] { "correr", "saltar", "deslizar", "lanzar" }
            },
            new SheetDef
            {
                relPath = "Characters/kikiri9_reposo_dano_derrota.png",
                cellW = 512, cellH = 384, cols = 4, rows = 3,
                pivotX = 0.5f, pivotY = 0.125f,
                rowNames = new[] { "reposo", "dano", "derrota" }
            },
            new SheetDef
            {
                relPath = "Characters/momia.png",
                cellW = 512, cellH = 384, cols = 6, rows = 2,
                pivotX = 0.5f, pivotY = 0.125f,
                rowNames = new[] { "caminar", "derrota" }
            },
            new SheetDef
            {
                relPath = "Characters/coronela.png",
                cellW = 512, cellH = 384, cols = 6, rows = 3,
                pivotX = 0.5f, pivotY = 0.125f,
                rowNames = new[] { "caminar", "ataque", "derrota" }
            },
            new SheetDef
            {
                relPath = "Characters/zombi.png",
                cellW = 512, cellH = 384, cols = 6, rows = 2,
                pivotX = 0.5f, pivotY = 0.125f,
                rowNames = new[] { "caminar", "derrota" }
            },
            new SheetDef
            {
                relPath = "Environment/obstaculos_decoracion.png",
                cellW = 464, cellH = 400, cols = 4, rows = 3,
                pivotX = 0.5f, pivotY = 0.08f,
                rowNames = new[] { "obstaculos", "decoracion_a", "decoracion_b" }
            },
            new SheetDef
            {
                relPath = "Items/objetos_efectos.png",
                cellW = 464, cellH = 384, cols = 4, rows = 3,
                pivotX = 0.5f, pivotY = 0.083333333f,
                rowNames = new[] { "objetos", "efectos_a", "efectos_b" }
            },
            new SheetDef
            {
                relPath = "UI/interfaz.png",
                cellW = 464, cellH = 304, cols = 4, rows = 4,
                pivotX = 0.5f, pivotY = 0.5f,
                rowNames = new[] { "hud", "controles", "botones", "paneles" }
            },
            new SheetDef
            {
                relPath = "Effects/electricidad_activacion_cempasuchil.png",
                cellW = 416, cellH = 448, cols = 4, rows = 3,
                pivotX = 0.5f, pivotY = 0.5f,
                rowNames = new[] { "relampago", "activacion", "cempasuchil" }
            },
            new SheetDef
            {
                relPath = "UI/interfaz.png",
                cellW = 320, cellH = 112, cols = 4, rows = 3,
                pivotX = 0.5f, pivotY = 0.5f,
                rowNames = new[] { "f0", "f1", "f2" },
                cellNames = new string[,]
                {
                    { "corazon_lleno", "corazon_vacio", "pan", "botella_verde" },
                    { "botella_dorada", "pausa", "sonido", "silencio" },
                    { "placa_vidas", "placa_puntos", "placa_tiempo", "base_pausa" }
                }
            },
            new SheetDef
            {
                relPath = "UI/botones.png",
                cellW = 320, cellH = 112, cols = 4, rows = 2,
                pivotX = 0.5f, pivotY = 0.5f,
                rowNames = new[] { "f0", "f1" },
                cellNames = new string[,]
                {
                    { "primario_normal", "primario_seleccionado", "primario_presionado", "primario_desactivado" },
                    { "secundario_normal", "secundario_seleccionado", "secundario_presionado", "secundario_desactivado" }
                },
                slice9Border = new Vector4(24, 24, 24, 24)
            },
            new SheetDef
            {
                relPath = "UI/controles_tactiles.png",
                cellW = 128, cellH = 128, cols = 3, rows = 2,
                pivotX = 0.5f, pivotY = 0.5f,
                rowNames = new[] { "f0", "f1" },
                cellNames = new string[,]
                {
                    { "saltar_normal", "deslizar_normal", "lanzar_normal" },
                    { "saltar_presionado", "deslizar_presionado", "lanzar_presionado" }
                }
            },
            new SheetDef
            {
                relPath = "UI/estados_nivel.png",
                cellW = 96, cellH = 96, cols = 2, rows = 1,
                pivotX = 0.5f, pivotY = 0.5f,
                rowNames = new[] { "f0" },
                cellNames = new string[,] { { "candado", "completado" } }
            },
            new SheetDef
            {
                relPath = "UI/selector_niveles.png",
                cellW = 320, cellH = 112, cols = 4, rows = 1,
                pivotX = 0.5f, pivotY = 0.5f,
                rowNames = new[] { "f0" },
                cellNames = new string[,] { { "nivel_disponible", "nivel_seleccionado", "nivel_bloqueado", "nivel_completado" } }
            },
        };

        [MenuItem("Tools/Kikiri-9/Slice Sprite Sheets")]
        public static void SliceAll()
        {
            int ok = 0;
            int fail = 0;
            foreach (var def in Sheets)
            {
                if (SliceOne(def))
                    ok++;
                else
                    fail++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Kikiri-9 slicer: {ok} hojas procesadas, {fail} fallidas.");
        }

        [MenuItem("Tools/Kikiri-9/Reset Import Settings (no slicing)")]
        public static void ResetImportSettingsOnly()
        {
            foreach (var def in Sheets)
                ApplyImportSettings(RootPath + "/" + def.relPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Kikiri-9: import settings aplicados sin tocar rects.");
        }

        private static bool SliceOne(SheetDef def)
        {
            string path = RootPath + "/" + def.relPath;

            if (!ApplyImportSettings(path))
                return false;

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null)
            {
                Debug.LogError($"[Kikiri-9] No TextureImporter: {path}");
                return false;
            }

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dp = factory.GetSpriteEditorDataProviderFromObject(importer);
            if (dp == null)
            {
                Debug.LogError($"[Kikiri-9] No DataProvider: {path}");
                return false;
            }

            dp.InitSpriteEditorDataProvider();

            var rects = new List<SpriteRect>();
            for (int r = 0; r < def.rows; r++)
            {
                string rowName = r < def.rowNames.Length ? def.rowNames[r] : $"row{r}";
                for (int c = 0; c < def.cols; c++)
                {
                    int yFromBottom = def.rows - 1 - r;
                    int x = c * def.cellW;
                    int y = yFromBottom * def.cellH;
                    string spriteName = (def.cellNames != null
                        && r < def.cellNames.GetLength(0)
                        && c < def.cellNames.GetLength(1)
                        && !string.IsNullOrEmpty(def.cellNames[r, c]))
                        ? def.cellNames[r, c]
                        : $"{rowName}_{(c + 1):00}";

                    rects.Add(new SpriteRect
                    {
                        name = spriteName,
                        rect = new Rect(x, y, def.cellW, def.cellH),
                        pivot = new Vector2(def.pivotX, def.pivotY),
                        alignment = SpriteAlignment.Custom,
                        spriteID = GUID.Generate(),
                        border = def.slice9Border,
                    });
                }
            }

            dp.SetSpriteRects(rects.ToArray());

            var nameFileIdProvider = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (nameFileIdProvider != null)
            {
                var pairs = new List<SpriteNameFileIdPair>(rects.Count);
                foreach (var sr in rects)
                    pairs.Add(new SpriteNameFileIdPair(sr.name, sr.spriteID));
                nameFileIdProvider.SetNameFileIdPairs(pairs);
            }

            dp.Apply();
            importer.SaveAndReimport();

            Debug.Log($"[Kikiri-9] {def.relPath}: {rects.Count} sprites");
            return true;
        }

        private static bool ApplyImportSettings(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[Kikiri-9] Falta hoja: {path}");
                return false;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 4096;
            importer.alphaIsTransparency = true;

            importer.SaveAndReimport();
            return true;
        }
    }
}
