// Copia el layout de los canvas de HUD (UI_Root, RafagaCanvas, PanDoradoHUD) desde una
// escena fuente a escenas destino. Recorre cada canvas y aplica anchorMin/Max, pivot,
// anchoredPosition, sizeDelta y offsetMin/Max de cada RectTransform (matching por path
// relativo al canvas). Preserva las referencias y componentes de las escenas destino.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class HUDLayoutCopier
    {
        private const string Level1 = "Assets/_Project/Scenes/Level01_Dolores.unity";
        private const string Level2 = "Assets/_Project/Scenes/Level02_Dolores.unity";
        private const string Level3 = "Assets/_Project/Scenes/Level03_Guanajuato.unity";

        // Canvases a copiar. Agrega aqui cualquier otro root que haya que replicar.
        private static readonly string[] CanvasRoots =
        {
            "UI_Root",
            "RafagaCanvas",
            "PanDoradoHUD",
        };

        private struct RectSnap
        {
            public string path;
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 pivot;
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
            public Vector2 offsetMin;
            public Vector2 offsetMax;
            public Vector3 localScale;
            public Vector3 localEulerAngles;
            public bool hasRect;
            public bool active;
        }

        [MenuItem("Tools/Kikiri-9/Copy HUD Layout from L3 -> L1 and L2")]
        public static void CopyL3ToL1L2()
        {
            var snaps = CaptureFromScene(Level3);
            if (snaps == null)
            {
                Debug.LogError("[Kikiri-9] No se pudo capturar el layout desde Level 3.");
                return;
            }
            ApplyToScene(Level1, snaps);
            ApplyToScene(Level2, snaps);
            Debug.Log($"[Kikiri-9] HUD layout de L3 replicado a L1 y L2 ({CountAllSnaps(snaps)} rects).");
        }

        [MenuItem("Tools/Kikiri-9/Copy HUD Layout from L3 -> L1")]
        public static void CopyL3ToL1()
        {
            var snaps = CaptureFromScene(Level3);
            if (snaps == null) return;
            ApplyToScene(Level1, snaps);
        }

        [MenuItem("Tools/Kikiri-9/Copy HUD Layout from L3 -> L2")]
        public static void CopyL3ToL2()
        {
            var snaps = CaptureFromScene(Level3);
            if (snaps == null) return;
            ApplyToScene(Level2, snaps);
        }

        private static Dictionary<string, List<RectSnap>> CaptureFromScene(string scenePath)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var result = new Dictionary<string, List<RectSnap>>();
            foreach (var rootName in CanvasRoots)
            {
                var root = GameObject.Find(rootName);
                if (root == null)
                {
                    Debug.LogWarning($"[Kikiri-9] {rootName} no encontrado en {scenePath}");
                    continue;
                }
                var list = new List<RectSnap>();
                CaptureRecursive(root.transform, "", list);
                result[rootName] = list;
                Debug.Log($"[Kikiri-9] Capturados {list.Count} rects de {rootName} en {scenePath}");
            }
            return result;
        }

        private static void CaptureRecursive(Transform t, string path, List<RectSnap> list)
        {
            var rt = t as RectTransform;
            var snap = new RectSnap
            {
                path = path,
                localScale = t.localScale,
                localEulerAngles = t.localEulerAngles,
                active = t.gameObject.activeSelf,
                hasRect = rt != null,
            };
            if (rt != null)
            {
                snap.anchorMin = rt.anchorMin;
                snap.anchorMax = rt.anchorMax;
                snap.pivot = rt.pivot;
                snap.anchoredPosition = rt.anchoredPosition;
                snap.sizeDelta = rt.sizeDelta;
                snap.offsetMin = rt.offsetMin;
                snap.offsetMax = rt.offsetMax;
            }
            list.Add(snap);

            foreach (Transform child in t)
            {
                string childPath = string.IsNullOrEmpty(path) ? child.name : path + "/" + child.name;
                CaptureRecursive(child, childPath, list);
            }
        }

        private static void ApplyToScene(string scenePath, Dictionary<string, List<RectSnap>> snaps)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int applied = 0, missed = 0;
            foreach (var kv in snaps)
            {
                var rootName = kv.Key;
                var root = GameObject.Find(rootName);
                if (root == null)
                {
                    Debug.LogWarning($"[Kikiri-9] {rootName} no existe en {scenePath}, se omite.");
                    continue;
                }
                foreach (var snap in kv.Value)
                {
                    Transform target = string.IsNullOrEmpty(snap.path)
                        ? root.transform
                        : FindByPath(root.transform, snap.path);
                    if (target == null) { missed++; continue; }

                    var rt = target as RectTransform;
                    if (rt != null && snap.hasRect)
                    {
                        rt.anchorMin = snap.anchorMin;
                        rt.anchorMax = snap.anchorMax;
                        rt.pivot = snap.pivot;
                        rt.anchoredPosition = snap.anchoredPosition;
                        rt.sizeDelta = snap.sizeDelta;
                        rt.offsetMin = snap.offsetMin;
                        rt.offsetMax = snap.offsetMax;
                    }
                    target.localScale = snap.localScale;
                    target.localEulerAngles = snap.localEulerAngles;
                    if (target.gameObject.activeSelf != snap.active)
                        target.gameObject.SetActive(snap.active);
                    applied++;
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[Kikiri-9] {scenePath}: aplicados {applied} rects, {missed} no encontrados.");
        }

        private static Transform FindByPath(Transform root, string path)
        {
            var parts = path.Split('/');
            Transform cur = root;
            foreach (var part in parts)
            {
                Transform next = null;
                foreach (Transform c in cur)
                {
                    if (c.name == part) { next = c; break; }
                }
                if (next == null) return null;
                cur = next;
            }
            return cur;
        }

        private static int CountAllSnaps(Dictionary<string, List<RectSnap>> d)
        {
            int n = 0;
            foreach (var l in d.Values) n += l.Count;
            return n;
        }
    }
}
