// V6: al bajar el suelo a Y=-3.2, ajusta TODO lo que depende de esa Y para que
// nada quede flotando: LevelSpawner.groundY, MiniBossSpawner.spawnY, GoalArch y
// todas las Decor_* del nivel. Tambien baja la camara base para que el pollo no
// quede pegado al centro vertical.

using Kikiri9.Player;
using Kikiri9.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public enum AdaptiveBgVariant { DoloresHidalgo, Guanajuato }

    public static class BackgroundsV3Builder
    {
        private static readonly Color DoloresGroundFill = new Color(0.18f, 0.12f, 0.26f, 1f);
        private static readonly Color GuanajuatoGroundFill = new Color(0.21f, 0.14f, 0.28f, 1f);

        private const float GroundY = -3.2f;
        private const float OriginalGroundY = -2f;
        private const float DeltaY = GroundY - OriginalGroundY; // -1.2

        public static void Build(AdaptiveBgVariant variant)
        {
            RemoveOldLayers();

            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[Kikiri-9] No Main Camera"); return; }
            var camT = cam.transform;

            cam.backgroundColor = variant == AdaptiveBgVariant.Guanajuato
                ? GuanajuatoGroundFill
                : DoloresGroundFill;
            cam.clearFlags = CameraClearFlags.SolidColor;

            // Bajar la camara para que el pollo no quede en el centro vertical
            var cp = camT.position;
            cp.y = -1f;
            camT.position = cp;

            LowerGround();
            LowerAllSpawnReferences();
            LowerAllDecorations();

            var linea = GameObject.Find("LineaSueloVisual");
            if (linea == null)
            {
                linea = new GameObject("LineaSueloVisual");
                linea.transform.position = new Vector3(0f, GroundY, 0f);
            }
            else
            {
                linea.transform.position = new Vector3(linea.transform.position.x, GroundY, 0f);
            }

            // Capas visuales
            var cieloSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{BackgroundsV3Importer.Dir}/cielo_panorama.png");
            float cNatH = cieloSprite != null ? cieloSprite.rect.height / cieloSprite.pixelsPerUnit : 7.24f;
            float cNatW = cieloSprite != null ? cieloSprite.rect.width / cieloSprite.pixelsPerUnit : 21.72f;
            float cieloScale = Mathf.Max((2f * cam.orthographicSize) / cNatH, (2f * cam.orthographicSize * cam.aspect) / cNatW) * 1.01f;
            CreateSkyLayer("Bg_Sky", cieloSprite, cieloScale, camT, sortingOrder: -30);

            string horizonFile = variant == AdaptiveBgVariant.DoloresHidalgo
                ? $"{BackgroundsV3Importer.Dir}/dolores_horizonte.png"
                : $"{BackgroundsV3Importer.Dir}/guanajuato_horizonte.png";
            string nearFile = variant == AdaptiveBgVariant.DoloresHidalgo
                ? $"{BackgroundsV3Importer.Dir}/dolores_plaza.png"
                : $"{BackgroundsV3Importer.Dir}/guanajuato_callejon.png";

            var horSprite = AssetDatabase.LoadAssetAtPath<Sprite>(horizonFile);
            var nearSprite = AssetDatabase.LoadAssetAtPath<Sprite>(nearFile);
            float facadeScale = variant == AdaptiveBgVariant.Guanajuato ? 1.05f : 1.1f;
            float horizonScale = variant == AdaptiveBgVariant.Guanajuato ? 1.15f : 1.2f;

            float nearAltoW = (nearSprite != null ? nearSprite.rect.height / nearSprite.pixelsPerUnit : 7.24f) * facadeScale;
            float horAltoW = (horSprite != null ? horSprite.rect.height / horSprite.pixelsPerUnit : 7.24f) * horizonScale;

            float nearLockedY = GroundY + 0.375f * nearAltoW;
            float horLockedY = GroundY + 0.375f * horAltoW + 0.5f;

            CreateParallaxLayer("Bg_Horizon", horizonFile, camT,
                sortingOrder: -20, tint: Color.white,
                parallax: 0.2f, lockedY: horLockedY,
                scale: horizonScale, tileFactor: 2f);

            Color nearTint = variant == AdaptiveBgVariant.DoloresHidalgo
                ? Color.white
                : new Color(0.824f, 0.863f, 0.969f);
            CreateParallaxLayer("Bg_Near", nearFile, camT,
                sortingOrder: -10, tint: nearTint,
                parallax: 0.45f, lockedY: nearLockedY,
                scale: facadeScale, tileFactor: 2f);

            CreateGroundFill(camT, linea.transform,
                variant == AdaptiveBgVariant.Guanajuato ? GuanajuatoGroundFill : DoloresGroundFill);
        }

        // -------- ajustes de nivel para que nada quede flotando --------

        private static void LowerGround()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null) return;
            var gp = ground.transform.position;
            gp.y = GroundY;
            ground.transform.position = gp;
        }

        private static void LowerAllDecorations()
        {
            // Decor_* y GoalArch (de BuildDecoration + BuildGoalArch de Level01SceneBootstrap)
            var all = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var sr in all)
            {
                var go = sr.gameObject;
                string n = go.name;
                if (n == "GoalArch" || n.StartsWith("Decor_"))
                {
                    var p = go.transform.position;
                    p.y += DeltaY; // mover la decoracion junto con el suelo
                    go.transform.position = p;
                }
            }
        }

        private static void LowerAllSpawnReferences()
        {
            // LevelSpawner.groundY
            var spawners = Object.FindObjectsByType<Kikiri9.Levels.LevelSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var sp in spawners)
            {
                var so = new SerializedObject(sp);
                var p = so.FindProperty("groundY");
                if (p != null)
                {
                    p.floatValue = GroundY + 0.2f; // un pelin arriba del suelo para que caigan
                    so.ApplyModifiedProperties();
                }
            }

            // MiniBossSpawner.spawnY
            var bossSps = Object.FindObjectsByType<Kikiri9.Levels.MiniBossSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var sp in bossSps)
            {
                var so = new SerializedObject(sp);
                var p = so.FindProperty("spawnY");
                if (p != null)
                {
                    p.floatValue = GroundY + 1.5f;
                    so.ApplyModifiedProperties();
                }
            }
        }

        // -------- capas --------

        private static void CreateSkyLayer(string name, Sprite sprite, float scale, Transform camT, int sortingOrder)
        {
            if (sprite == null) return;
            var go = new GameObject(name);
            go.transform.position = new Vector3(camT.position.x, 0f, 10f);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            sr.color = Color.white;
            sr.drawMode = SpriteDrawMode.Simple;

            var follow = go.AddComponent<BackgroundFollowCamera>();
            var so = new SerializedObject(follow);
            so.FindProperty("cameraTransform").objectReferenceValue = camT;
            so.FindProperty("lockedY").floatValue = 0f;
            so.ApplyModifiedProperties();
        }

        private static void CreateParallaxLayer(string name, string path, Transform camT,
            int sortingOrder, Color tint, float parallax, float lockedY,
            float scale, float tileFactor)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) { Debug.LogWarning($"[Kikiri-9] falta sprite {path}"); return; }

            float natW = sprite.rect.width / sprite.pixelsPerUnit;
            float natH = sprite.rect.height / sprite.pixelsPerUnit;

            var go = new GameObject(name);
            go.transform.position = new Vector3(0f, lockedY, 10f);
            go.transform.localScale = new Vector3(scale, scale, 1f);

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
            so.FindProperty("tileWidth").floatValue = natW * scale;
            so.ApplyModifiedProperties();
        }

        private static void CreateGroundFill(Transform camT, Transform linea, Color color)
        {
            var go = new GameObject("GroundFill");
            float fillY = linea.position.y - 15f;
            go.transform.position = new Vector3(camT.position.x, fillY, 9.5f);
            go.transform.localScale = new Vector3(120f, 30f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SolidSprite();
            sr.color = color;
            sr.sortingOrder = -12;
            sr.drawMode = SpriteDrawMode.Simple;

            var follow = go.AddComponent<BackgroundFollowCamera>();
            var so = new SerializedObject(follow);
            so.FindProperty("cameraTransform").objectReferenceValue = camT;
            so.FindProperty("lockedY").floatValue = fillY;
            so.ApplyModifiedProperties();
        }

        private static Sprite cachedSolidSprite;
        private static Sprite SolidSprite()
        {
            if (cachedSolidSprite != null) return cachedSolidSprite;
            var t = new Texture2D(2, 2);
            var px = new Color[4];
            for (int i = 0; i < 4; i++) px[i] = Color.white;
            t.SetPixels(px);
            t.Apply();
            cachedSolidSprite = Sprite.Create(t, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
            return cachedSolidSprite;
        }

        private static void RemoveOldLayers()
        {
            string[] names = { "Bg_Cielo", "Bg_Dolores", "Bg_Guanajuato", "Bg_Silueta",
                                "Bg_Sky", "Bg_Horizon", "Bg_Near", "BgRoot", "GroundFill" };
            foreach (var n in names)
            {
                var go = GameObject.Find(n);
                if (go != null) Object.DestroyImmediate(go);
            }
        }
    }

    public static class BackgroundsV3Applier
    {
        [MenuItem("Tools/Kikiri-9/Apply Adaptive Backgrounds V3 to Level 1")]
        public static void ApplyToLevel1()
            => Apply("Assets/_Project/Scenes/Level01_Dolores.unity", AdaptiveBgVariant.DoloresHidalgo);

        [MenuItem("Tools/Kikiri-9/Apply Adaptive Backgrounds V3 to Level 2")]
        public static void ApplyToLevel2()
            => Apply("Assets/_Project/Scenes/Level02_Dolores.unity", AdaptiveBgVariant.DoloresHidalgo);

        [MenuItem("Tools/Kikiri-9/Apply Adaptive Backgrounds V3 to Level 3")]
        public static void ApplyToLevel3()
            => Apply("Assets/_Project/Scenes/Level03_Guanajuato.unity", AdaptiveBgVariant.Guanajuato);

        [MenuItem("Tools/Kikiri-9/Apply Adaptive Backgrounds V3 to ALL Levels")]
        public static void ApplyToAll()
        {
            ApplyToLevel1(); ApplyToLevel2(); ApplyToLevel3();
        }

        [MenuItem("Tools/Kikiri-9/Add Punch Mechanic to ALL Levels")]
        public static void AddPunchToAll()
        {
            AddPunchTo("Assets/_Project/Scenes/Level01_Dolores.unity");
            AddPunchTo("Assets/_Project/Scenes/Level02_Dolores.unity");
            AddPunchTo("Assets/_Project/Scenes/Level03_Guanajuato.unity");
        }

        private static void AddPunchTo(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var pollo = GameObject.FindGameObjectWithTag("Player");
            if (pollo != null && pollo.GetComponent<PlayerPunch>() == null)
                pollo.AddComponent<PlayerPunch>();

            var session = GameObject.Find("Session");
            if (session != null && session.GetComponent<Kikiri9.UI.PunchChargeIndicatorInstaller>() == null)
                session.AddComponent<Kikiri9.UI.PunchChargeIndicatorInstaller>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[Kikiri-9] Punch mechanic agregado a {scenePath}");
        }

        private static void Apply(string scenePath, AdaptiveBgVariant variant)
        {
            BackgroundsV3Importer.ImportAll();
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            BackgroundsV3Builder.Build(variant);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[Kikiri-9] Adaptive V3 aplicado a {scenePath}");
        }
    }
}
