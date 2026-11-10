// Monta las 3 capas de fondo en una escena abierta: cielo (0), horizonte (+Y 10),
// ciudad (+Y 20). Usa parallax factors del PDF (horizonte 20%, ciudad 45%).
// Elimina las capas viejas Bg_* si existen.

using Kikiri9.World;
using UnityEditor;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class BackgroundsV2Builder
    {
        public enum Scene { DoloresHidalgo, Guanajuato }

        private const string Dir = "Assets/_Project/Art/Backgrounds_V2";

        public static void Build(Scene scene)
        {
            RemoveOldLayers();

            var camT = Camera.main != null ? Camera.main.transform : null;
            if (camT == null) { Debug.LogError("[Kikiri-9] No Main Camera"); return; }

            var cam = Camera.main;
            if (cam != null) cam.orthographicSize = 4.705f;

            // Sortings negativos para que todo el gameplay (suelo 0+, pollo 5+, enemigos 0+)
            // quede por delante de los fondos.
            SpawnSkyLayer("Bg_Sky", $"{Dir}/cielo_luna_tormenta.png", camT,
                sortingOrder: -30, lockedY: 0f);

            string horizonFile = scene == Scene.DoloresHidalgo
                ? $"{Dir}/dolores_hidalgo_horizonte.png"
                : $"{Dir}/guanajuato_horizonte.png";
            float horizonY = scene == Scene.DoloresHidalgo ? 0.75f : 1.75f;
            SpawnTiledLayer("Bg_Horizon", horizonFile, camT,
                sortingOrder: -20, lockedY: horizonY, parallax: 0.2f, tint: Color.white, tileFactor: 3f);

            string nearFile = scene == Scene.DoloresHidalgo
                ? $"{Dir}/dolores_hidalgo_plaza.png"
                : $"{Dir}/guanajuato_callejon.png";
            float nearY = scene == Scene.DoloresHidalgo ? 0f : 0.84f;
            Color nearTint = scene == Scene.DoloresHidalgo ? Color.white : new Color(0.757f, 0.824f, 0.949f);
            SpawnTiledLayer("Bg_Near", nearFile, camT,
                sortingOrder: -10, lockedY: nearY, parallax: 0.45f, tint: nearTint, tileFactor: 3f);
        }

        private static void SpawnSkyLayer(string name, string path, Transform camT,
            int sortingOrder, float lockedY)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) { Debug.LogWarning($"[Kikiri-9] falta sprite {path}"); return; }

            float natW = sprite.rect.width / sprite.pixelsPerUnit;
            float natH = sprite.rect.height / sprite.pixelsPerUnit;

            var go = new GameObject(name);
            go.transform.position = new Vector3(0f, lockedY, 10f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            sr.color = Color.white;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(natW, natH);

            var follow = go.AddComponent<Kikiri9.World.CameraFollowBg>();
            var so = new SerializedObject(follow);
            so.FindProperty("cameraTransform").objectReferenceValue = camT;
            so.FindProperty("lockedY").floatValue = lockedY;
            so.FindProperty("lockedZ").floatValue = 10f;
            so.ApplyModifiedProperties();
        }

        private static void SpawnTiledLayer(string name, string path, Transform camT,
            int sortingOrder, float lockedY, float parallax, Color tint, float tileFactor)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) { Debug.LogWarning($"[Kikiri-9] falta sprite {path}"); return; }

            float natW = sprite.rect.width / sprite.pixelsPerUnit;
            float natH = sprite.rect.height / sprite.pixelsPerUnit;

            var go = new GameObject(name);
            go.transform.position = new Vector3(0f, lockedY, 10f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            sr.color = tint;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(natW * tileFactor, natH);

            var pf = go.AddComponent<ParallaxLayer>();
            var so = new SerializedObject(pf);
            so.FindProperty("cameraTransform").objectReferenceValue = camT;
            so.FindProperty("parallaxFactor").floatValue = parallax;
            so.FindProperty("lockedY").floatValue = lockedY;
            so.FindProperty("tileWidth").floatValue = natW;
            so.ApplyModifiedProperties();
        }

        private static void RemoveOldLayers()
        {
            string[] names = { "Bg_Cielo", "Bg_Dolores", "Bg_Guanajuato", "Bg_Silueta",
                                "Bg_Sky", "Bg_Horizon", "Bg_Near" };
            foreach (var n in names)
            {
                var go = GameObject.Find(n);
                if (go != null) Object.DestroyImmediate(go);
            }
        }


    }
}

namespace Kikiri9.EditorTools
{
    using UnityEditor.SceneManagement;
    using UnityEngine;

    public static class BackgroundsV2SceneApplier
    {
        [MenuItem("Tools/Kikiri-9/Apply Backgrounds V2 to Level 1")]
        public static void ApplyToLevel1()
        {
            Apply("Assets/_Project/Scenes/Level01_Dolores.unity", BackgroundsV2Builder.Scene.DoloresHidalgo);
        }

        [MenuItem("Tools/Kikiri-9/Apply Backgrounds V2 to Level 2")]
        public static void ApplyToLevel2()
        {
            Apply("Assets/_Project/Scenes/Level02_Dolores.unity", BackgroundsV2Builder.Scene.DoloresHidalgo);
        }

        [MenuItem("Tools/Kikiri-9/Apply Backgrounds V2 to Level 3")]
        public static void ApplyToLevel3()
        {
            Apply("Assets/_Project/Scenes/Level03_Guanajuato.unity", BackgroundsV2Builder.Scene.Guanajuato);
        }

        private static void Apply(string scenePath, BackgroundsV2Builder.Scene variant)
        {
            BackgroundsV2Importer.ImportAll();
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            BackgroundsV2Builder.Build(variant);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[Kikiri-9] Backgrounds V2 aplicados a {scenePath}");
        }
    }
}
