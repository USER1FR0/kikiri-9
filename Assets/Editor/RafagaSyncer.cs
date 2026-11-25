// Sincroniza la posicion del RafagaButton del Nivel 1 hacia Nivel 2, 3 y 4.
// Lees las propiedades de Rect (pos, size, anchors, pivot) del RafagaButton
// en Level01_Dolores.unity y las aplicas a los mismos nombres de GameObject
// en Level02_Dolores, Level03_Guanajuato y Level04_Guanajuato.
//
// Uso: muever el RafagaButton en L1 en el editor, guarda la escena, luego
// corre Tools > Kikiri-9 > Sync Rafaga L1 -> L2/L3/L4.

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class RafagaSyncer
    {
        private const string L1Path = "Assets/_Project/Scenes/Level01_Dolores.unity";
        private static readonly string[] Targets = {
            "Assets/_Project/Scenes/Level02_Dolores.unity",
            "Assets/_Project/Scenes/Level03_Guanajuato.unity",
            "Assets/_Project/Scenes/Level04_Guanajuato.unity",
        };

        [MenuItem("Tools/Kikiri-9/Sync Rafaga L1 -> L2/L3/L4")]
        public static void Sync()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Kikiri-9", "Sal de Play Mode antes de sincronizar.", "OK");
                return;
            }

            if (!File.Exists(L1Path))
            {
                EditorUtility.DisplayDialog("Kikiri-9", "No se encontro Level01_Dolores.unity.", "OK");
                return;
            }

            var l1 = File.ReadAllText(L1Path);
            var l1Rect = FindRafagaRect(l1);
            if (l1Rect == null)
            {
                EditorUtility.DisplayDialog("Kikiri-9", "No se encontro RafagaButton en L1.", "OK");
                return;
            }

            int updated = 0;
            foreach (var path in Targets)
            {
                if (!File.Exists(path)) continue;
                var content = File.ReadAllText(path);
                var patched = ApplyRafagaRect(content, l1Rect);
                if (patched != null && patched != content)
                {
                    File.WriteAllText(path, patched);
                    updated++;
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Kikiri-9",
                $"RafagaButton sincronizado desde L1 hacia {updated} nivel(es).\n\n" +
                $"Pos: {l1Rect.Value.pos}\nSize: {l1Rect.Value.size}\nPivot: {l1Rect.Value.pivot}\n" +
                $"AnchorMin: {l1Rect.Value.aMin}\nAnchorMax: {l1Rect.Value.aMax}",
                "OK");
        }

        private struct RafagaRect
        {
            public string pos, size, pivot, aMin, aMax;
        }

        // Encuentra el GameObject RafagaButton y su RectTransform, extrae sus props.
        private static RafagaRect? FindRafagaRect(string sceneContent)
        {
            // Encontrar fileID del GameObject llamado RafagaButton
            var goMatches = Regex.Matches(sceneContent,
                @"--- !u!1 &(\d+)\s*\r?\nGameObject:(.*?)(?=--- !u!|\Z)",
                RegexOptions.Singleline);

            foreach (Match go in goMatches)
            {
                if (!go.Groups[2].Value.Contains("m_Name: RafagaButton")) continue;
                var goBody = go.Groups[2].Value;
                var comps = Regex.Matches(goBody, @"- component: \{fileID: (\d+)\}");
                foreach (Match comp in comps)
                {
                    var fid = comp.Groups[1].Value;
                    var compMatch = Regex.Match(sceneContent,
                        $@"--- !u!(\d+) &{fid}\s*\r?\n(\w+):(.*?)(?=--- !u!|\Z)",
                        RegexOptions.Singleline);
                    if (!compMatch.Success) continue;
                    if (compMatch.Groups[2].Value == "RectTransform")
                    {
                        var body = compMatch.Groups[3].Value;
                        return new RafagaRect
                        {
                            pos  = ExtractVec2(body, "m_AnchoredPosition"),
                            size = ExtractVec2(body, "m_SizeDelta"),
                            pivot= ExtractVec2(body, "m_Pivot"),
                            aMin = ExtractVec2(body, "m_AnchorMin"),
                            aMax = ExtractVec2(body, "m_AnchorMax"),
                        };
                    }
                }
            }
            return null;
        }

        private static string ExtractVec2(string body, string field)
        {
            var m = Regex.Match(body, $@"{field}: \{{x: ([\-\d\.eE]+), y: ([\-\d\.eE]+)\}}");
            return m.Success ? $"({m.Groups[1].Value}, {m.Groups[2].Value})" : "?";
        }

        // Aplica a la escena destino los mismos valores del RafagaButton de L1.
        private static string ApplyRafagaRect(string sceneContent, RafagaRect? rect)
        {
            if (rect == null) return null;
            var r = rect.Value;
            // Parsear los valores extraidos
            var pPos = ParseVec2(r.pos);
            var pSize = ParseVec2(r.size);
            var pPivot = ParseVec2(r.pivot);
            var pMin = ParseVec2(r.aMin);
            var pMax = ParseVec2(r.aMax);

            // Localizar RafagaButton RectTransform en la escena destino
            var goMatches = Regex.Matches(sceneContent,
                @"--- !u!1 &(\d+)\s*\r?\nGameObject:(.*?)(?=--- !u!|\Z)",
                RegexOptions.Singleline);

            foreach (Match go in goMatches)
            {
                if (!go.Groups[2].Value.Contains("m_Name: RafagaButton")) continue;
                var goBody = go.Groups[2].Value;
                var comps = Regex.Matches(goBody, @"- component: \{fileID: (\d+)\}");
                foreach (Match comp in comps)
                {
                    var fid = comp.Groups[1].Value;
                    // Reemplazar en el bloque de ese RectTransform
                    sceneContent = ReplaceInRectTransform(sceneContent, fid, "m_AnchoredPosition", pPos);
                    sceneContent = ReplaceInRectTransform(sceneContent, fid, "m_SizeDelta", pSize);
                    sceneContent = ReplaceInRectTransform(sceneContent, fid, "m_Pivot", pPivot);
                    sceneContent = ReplaceInRectTransform(sceneContent, fid, "m_AnchorMin", pMin);
                    sceneContent = ReplaceInRectTransform(sceneContent, fid, "m_AnchorMax", pMax);
                }
            }
            return sceneContent;
        }

        private static (string x, string y) ParseVec2(string formatted)
        {
            var m = Regex.Match(formatted, @"\(([^,]+),\s*([^)]+)\)");
            return m.Success ? (m.Groups[1].Value, m.Groups[2].Value) : ("0","0");
        }

        private static string ReplaceInRectTransform(string content, string fid, string field, (string x,string y) val)
        {
            var pattern = new Regex(
                $@"(--- !u!\d+ &{fid}\s*\r?\n\w+:.*?{field}: \{{x: )[\-\d\.eE]+(, y: )[\-\d\.eE]+(\}})",
                RegexOptions.Singleline);
            return pattern.Replace(content, m => $"{m.Groups[1].Value}{val.x}{m.Groups[2].Value}{val.y}{m.Groups[3].Value}", 1);
        }
    }
}
