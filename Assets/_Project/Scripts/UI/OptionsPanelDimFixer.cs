// Fix runtime no-destructivo del panel de Opciones in-game. No regenera el
// UI_Root (eso moveria el HUD). Al cargar cualquier escena Level*, busca el
// OptionsPanel y aplica:
//  1. Dim opaco detras para ocultar HUD/gameplay.
//  2. Reposiciona el Titulo "OPCIONES" arriba del panel (evita el solape con el
//     slider de Musica).
//  3. Reposiciona los sliders y labels un poco mas abajo para dar aire.
//  4. Amplia el CloseButton ("Cerrar" -> "VOLVER") y lo hace del mismo tamano
//     que el VOLVER del MainMenu (380x110).
//
// Objetivo: que al abrir Opciones durante gameplay se vea como el Opciones del
// MainMenu, sin reconstruir el prefab UI_Root.

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    public static class OptionsPanelDimFixer
    {
        private const string DimName = "DimFixerBg";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryFix(SceneManager.GetActiveScene().name);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => TryFix(scene.name);

        private static void TryFix(string sceneName)
        {
            if (!sceneName.StartsWith("Level") || sceneName == "LevelSelect") return;

            var panels = Object.FindObjectsByType<OptionsPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var op in panels) ApplyFixes(op.gameObject);
        }

        private static void ApplyFixes(GameObject panelGo)
        {
            if (panelGo == null) return;

            var visualTransform = panelGo.transform.Find("Visual");
            var root = visualTransform != null ? visualTransform.gameObject : panelGo;

            EnsureDim(root);
            RepositionLayout(root);
            UpgradeCloseButton(root);
        }

        private static void EnsureDim(GameObject root)
        {
            if (root.transform.Find(DimName) != null) return;

            var dim = new GameObject(DimName, typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(root.transform, false);
            dim.transform.SetAsFirstSibling();

            var rt = (RectTransform)dim.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-2000f, -2000f);
            rt.offsetMax = new Vector2(2000f, 2000f);

            var img = dim.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.95f);
            img.raycastTarget = true;
        }

        // Mueve titulo arriba y sliders un poco mas abajo para evitar solape.
        private static void RepositionLayout(GameObject root)
        {
            MoveTo(root, "Title", new Vector2(0f, 230f), new Vector2(1000f, 60f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            MoveTo(root, "MusicLabel", new Vector2(-420f, 110f), new Vector2(260f, 40f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            MoveTo(root, "MusicSlider", new Vector2(120f, 110f), new Vector2(680f, 36f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            MoveTo(root, "SfxLabel", new Vector2(-420f, 40f), new Vector2(260f, 40f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            MoveTo(root, "SfxSlider", new Vector2(120f, 40f), new Vector2(680f, 36f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            MoveTo(root, "QualityButton", new Vector2(-220f, -60f), new Vector2(380f, 90f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            MoveTo(root, "ControlsButton", new Vector2(220f, -60f), new Vector2(380f, 90f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        }

        // Cambia el label "Cerrar" por "VOLVER" y agranda el boton (mismo tamano que el
        // VOLVER del MainMenu).
        private static void UpgradeCloseButton(GameObject root)
        {
            var close = Find(root.transform, "CloseButton");
            if (close == null) return;

            var rt = close as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -200f);
                rt.sizeDelta = new Vector2(400f, 120f);
            }

            var label = close.Find("Label");
            if (label != null)
            {
                var tmp = label.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    tmp.text = "VOLVER";
                    tmp.fontSize = 36f;
                    tmp.fontStyle = FontStyles.Bold;
                }
            }
        }

        // Helpers

        private static void MoveTo(GameObject root, string childName,
            Vector2 anchoredPos, Vector2 size,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            var t = Find(root.transform, childName) as RectTransform;
            if (t == null) return;
            t.anchorMin = anchorMin;
            t.anchorMax = anchorMax;
            t.pivot = pivot;
            t.anchoredPosition = anchoredPos;
            t.sizeDelta = size;
        }

        private static Transform Find(Transform parent, string name)
        {
            if (parent == null) return null;
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var r = Find(parent.GetChild(i), name);
                if (r != null) return r;
            }
            return null;
        }
    }
}
