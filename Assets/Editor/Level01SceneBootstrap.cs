// Editor-only bootstrap for the Level01 (Dolores Hidalgo) scene. Mirrors SandboxBootstrap
// but adds: PlayerHealth(3), GameStateManager, ScoreTracker, WorldScroller, LevelController
// (ref SO), LevelSpawner (mixed pool), UI_Root prefab, Dolores background. Sandbox stays
// untouched; this is a separate scene with its own setup.
//
// Run order:
//   1. Tools > Kikiri-9 > Slice Sprite Sheets
//   2. Tools > Kikiri-9 > Build Player Animations
//   3. Tools > Kikiri-9 > Build Level 01 Assets
//   4. Tools > Kikiri-9 > Build UI Prefab
//   5. Tools > Kikiri-9 > Build Level 01 Scene   <-- this file

using System.IO;
using Kikiri9.CameraSystem;
using Kikiri9.Combat;
using Kikiri9.Enemies;
using Kikiri9.Levels;
using Kikiri9.Player;
using Kikiri9.Session;
using Kikiri9.World;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class Level01SceneBootstrap
    {
        private const string ArtRoot = "Assets/_Draft_2_Kikiri9/Art";
        private const string GroundSheet = ArtRoot + "/Environment/suelo.png";
        private const string PolloSheet = ArtRoot + "/Characters/kikiri9.png";
        private const string ItemsSheet = ArtRoot + "/Items/objetos_efectos.png";
        private const string BgCielo = ArtRoot + "/Backgrounds/cielo.png";
        private const string BgDolores = ArtRoot + "/Backgrounds/dolores_hidalgo.png";
        private const string BgSilueta = ArtRoot + "/Backgrounds/silueta_guanajuato.png";
        private const string ControllerPath = "Assets/_Project/Animations/Kikiri9.controller";
        private const string LevelSOPath = "Assets/_Project/ScriptableObjects/Levels/Level01_Dolores.asset";
        private const string DecorSheet = ArtRoot + "/Environment/obstaculos_decoracion.png";
        private const string SfxDir = "Assets/_Draft_2_Kikiri9/Audio/SFX";
        private const string UIPrefabPath = "Assets/_Project/Prefabs/UI/UI_Root.prefab";
        private const string ScenePath = "Assets/_Project/Scenes/Level01_Dolores.unity";

        [MenuItem("Tools/Kikiri-9/Build Level 01 Scene")]
        public static void Setup()
        {
            Directory.CreateDirectory("Assets/_Project/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cam = BuildCamera();
            BuildBackground(cam.transform);
            BuildDecoration(cam.transform);
            BuildGround();
            BuildGoalArch();
            var pollo = BuildPollo();

            if (pollo != null)
            {
                var follow = cam.gameObject.AddComponent<CameraFollow>();
                follow.SetTarget(pollo.transform);
            }

            var levelController = BuildSessionRoot();
            BuildSpawner(cam.transform, levelController);
            BuildUI();

            Selection.activeGameObject = pollo;
            SceneView.FrameLastActiveSceneView();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Kikiri-9] Level01 scene saved at {ScenePath}");
        }

        // -------- camera --------

        private static Camera BuildCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cam.backgroundColor = new Color(0.08f, 0.04f, 0.14f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            go.AddComponent<AudioListener>();
            go.transform.position = new Vector3(0f, 0f, -10f);
            return cam;
        }

        // -------- background (cielo + dolores parallax) --------

        private static void BuildBackground(Transform camT)
        {
            // Nuevo dolores_hidalgo.png (1672x941 PPU 128) sin banqueta pintada. Cielo opcional
            // atras y el suelo tiled al frente. Parallax medio para la ciudad.
            SpawnBgLayer("Bg_Cielo",   BgCielo,   camT, lockedY: -1.5f, scaleMul: 1.5f, parallax: 0.05f, sortingOrder: -30);
            SpawnBgLayer("Bg_Dolores", BgDolores, camT, lockedY: 0.3f, scaleMul: 1.0f, parallax: 0.4f, sortingOrder: -20);
        }

        private static void SpawnBgLayer(string name, string path, Transform camT, float lockedY, float scaleMul, float parallax, int sortingOrder)
        {
            var sprite = LoadBgSprite(path);
            if (sprite == null) return;

            float naturalW = sprite.rect.width / sprite.pixelsPerUnit;
            float naturalH = sprite.rect.height / sprite.pixelsPerUnit;
            float viewportH = 9f;
            float scale = (viewportH / naturalH) * scaleMul;

            var go = new GameObject(name);
            go.transform.position = new Vector3(0f, lockedY, 10f);
            go.transform.localScale = new Vector3(scale, scale, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(naturalW * 5f, naturalH);
            sr.sortingOrder = sortingOrder;

            float tileWidthWorld = naturalW * scale;

            var pf = go.AddComponent<ParallaxLayer>();
            var so = new SerializedObject(pf);
            so.FindProperty("cameraTransform").objectReferenceValue = camT;
            so.FindProperty("parallaxFactor").floatValue = parallax;
            so.FindProperty("lockedY").floatValue = lockedY;
            so.FindProperty("tileWidth").floatValue = tileWidthWorld;
            so.ApplyModifiedProperties();
        }

        private static Sprite LoadBgSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) { Debug.LogWarning($"[Kikiri-9] Missing bg: {path}"); return null; }
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 128f);
        }

        // -------- ground --------

        private static void BuildGround()
        {
            // Nuevo suelo.png (2048x229 PPU 128, pivot top-center 0.5/1.0): ancho natural
            // 16u, alto 1.789u. Lo repetimos tiled a lo largo del nivel. El pollo pisa el
            // borde SUPERIOR del sprite => GameObject.Y = altura de apoyo.
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GroundSheet);
            var go = new GameObject("Ground");
            // Start muy a la izquierda para cubrir desde antes del pollo (spawn X=-6)
            go.transform.position = new Vector3(400f, -2f, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(1000f, 1.789f);
            }
            sr.sortingOrder = -5;

            // Collider: superficie en el top (Y = GameObject.Y). Centro del box en
            // Y = -1 * size.y/2 (desde el pivote top).
            var bc = go.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(1000f, 1.789f);
            bc.offset = new Vector2(0f, -0.8945f);
        }

        // -------- pollo --------

        private static float LoadWorldSpeed()
        {
            var def = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelSOPath);
            return def != null ? def.worldSpeed : 5f;
        }

        private static GameObject BuildPollo()
        {
            var sprite = FindSubSprite(PolloSheet, "correr_01") ?? FindFirstSprite(PolloSheet);
            if (sprite == null) { Debug.LogError($"[Kikiri-9] No pollo sprite in {PolloSheet}"); return null; }

            var go = new GameObject("Kikiri9");
            go.tag = "Player";
            go.transform.position = new Vector3(-6f, 0f, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 20;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearDamping = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var cc = go.AddComponent<CapsuleCollider2D>();
            cc.direction = CapsuleDirection2D.Vertical;
            cc.size = new Vector2(1.3f, 1.9f);
            cc.offset = new Vector2(0f, 0.95f);

            var checkGo = new GameObject("GroundCheck");
            checkGo.transform.SetParent(go.transform, worldPositionStays: false);
            checkGo.transform.localPosition = new Vector3(0f, -0.1f, 0f);

            var muzzleGo = new GameObject("Muzzle");
            muzzleGo.transform.SetParent(go.transform, worldPositionStays: false);
            muzzleGo.transform.localPosition = new Vector3(1.0f, 1.3f, 0f);

            var animator = go.AddComponent<Animator>();
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller != null) animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            var launcher = go.AddComponent<BreadLauncher>();
            var breadSprite = FindSubSprite(ItemsSheet, "objetos_02");  // pan con efecto cinetico (proyectil)
            var splashSprite = FindSubSprite(ItemsSheet, "efectos_a_02");
            var soL = new SerializedObject(launcher);
            soL.FindProperty("muzzle").objectReferenceValue = muzzleGo.transform;
            if (breadSprite != null) soL.FindProperty("breadSprite").objectReferenceValue = breadSprite;
            if (splashSprite != null) soL.FindProperty("splashSprite").objectReferenceValue = splashSprite;
            soL.ApplyModifiedProperties();

            var pc = go.AddComponent<PlayerController>();
            var so = new SerializedObject(pc);
            so.FindProperty("groundCheck").objectReferenceValue = checkGo.transform;
            so.FindProperty("groundCheckRadius").floatValue = 0.18f;
            so.FindProperty("groundLayer").intValue = ~0;
            so.FindProperty("animator").objectReferenceValue = animator;
            so.FindProperty("breadLauncher").objectReferenceValue = launcher;
            // Single source of speed: pollo runs at worldSpeed, obstacles stay still.
            var runSpeedProp = so.FindProperty("runSpeed");
            if (runSpeedProp != null) runSpeedProp.floatValue = LoadWorldSpeed();
            so.ApplyModifiedProperties();

            // Level health (3 hearts). GameOverHandler yields to PlayerHealth automatically.
            var ph = go.AddComponent<PlayerHealth>();
            var soH = new SerializedObject(ph);
            soH.FindProperty("maxHealth").intValue = 3;
            soH.FindProperty("invulnerabilityDuration").floatValue = 1f;
            var smokeVfx = FindSubSprite(ItemsSheet, "efectos_a_04");
            if (smokeVfx != null) soH.FindProperty("damageVfxSprite").objectReferenceValue = smokeVfx;
            soH.ApplyModifiedProperties();

            go.AddComponent<GameOverHandler>();

            // Hit/Dead animations driven by PlayerHealth
            go.AddComponent<Kikiri9.Player.PlayerAnimatorBridge>();

            // Bread ammo (5 panes al inicio, max 20)
            go.AddComponent<Kikiri9.Player.PlayerAmmo>();

            // SFX: AudioSource + PlayerSfx with clips wired
            go.AddComponent<AudioSource>();
            var sfx = go.AddComponent<Kikiri9.Player.PlayerSfx>();
            var soSfx = new SerializedObject(sfx);
            void Wire(string field, string clipName)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/{clipName}.wav");
                if (clip != null) soSfx.FindProperty(field).objectReferenceValue = clip;
            }
            Wire("jumpClip", "salto");
            Wire("slideClip", "deslizar");
            Wire("throwClip", "lanzar_pan");
            Wire("damageClip", "dano");
            Wire("gameOverClip", "game_over");
            soSfx.ApplyModifiedProperties();

            return go;
        }

        private static void BuildGoalArch()
        {
            // Arco decorativo en la meta (500m desde la cámara inicial). Sprite obstaculos_03
            // del atlas de obstáculos. No colisiona, es visual.
            var sprite = FindSubSprite(DecorSheet, "obstaculos_03");
            if (sprite == null) return;

            var go = new GameObject("GoalArch");
            go.transform.position = new Vector3(500f, -2f, 1f);
            go.transform.localScale = Vector3.one * 1.5f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -2; // detras del pollo
        }

        private static void BuildDecoration(Transform camT)
        {
            // Static props scattered along the first 500m. No colliders, parallax slightly
            // slower than foreground so they slide at a nice depth. Row 'decoracion_a' and
            // 'decoracion_b' from obstaculos_decoracion.png.
            // Pocos props, bien separados, para no competir con enemigos y obstaculos.
            string[] names = { "decoracion_b_01", "decoracion_a_02", "decoracion_b_01",
                               "decoracion_a_02", "decoracion_b_01", "decoracion_b_04" };
            float[] positions = { 20f, 70f, 130f, 220f, 320f, 430f };
            int ni = 0;
            foreach (var x in positions)
            {
                var sprite = FindSubSprite(DecorSheet, names[ni % names.Length]);
                ni++;
                if (sprite == null) continue;
                var go = new GameObject($"Decor_{Mathf.FloorToInt(x)}");
                // Decoracion al nivel del suelo pero "detras" visualmente: Z mayor,
                // sortingOrder bajo, tint oscurecido para simular profundidad atmosferica.
                go.transform.position = new Vector3(x, -2f, 6f);
                go.transform.localScale = Vector3.one * 0.85f; // mas grandes
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = -4; // detras del pollo, enemigos y obstaculos
                sr.color = new Color(0.6f, 0.5f, 0.65f, 0.9f); // oscurecida como si estuviera lejos (fog)
            }
        }

        // -------- session root (managers + LevelController) --------

        private static LevelController BuildSessionRoot()
        {
            var def = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelSOPath);
            if (def == null) Debug.LogError($"[Kikiri-9] LevelDefinition missing at {LevelSOPath}. Run Build Level 01 Assets.");

            var root = new GameObject("Session");
            root.AddComponent<GameStateManager>();
            root.AddComponent<PauseInput>();
            root.AddComponent<ScoreTracker>();

            var ws = root.AddComponent<WorldScroller>();
            var soW = new SerializedObject(ws);
            soW.FindProperty("initialSpeed").floatValue = def != null ? def.worldSpeed : 5f;
            soW.ApplyModifiedProperties();

            var lc = root.AddComponent<LevelController>();
            var soL = new SerializedObject(lc);
            soL.FindProperty("definition").objectReferenceValue = def;
            soL.ApplyModifiedProperties();

            // Background music from the SO, controlled by MusicController (pausa con el estado)
            if (def != null && def.musicClip != null)
            {
                var music = root.AddComponent<AudioSource>();
                music.clip = def.musicClip;
                music.loop = true;
                music.playOnAwake = true;
                music.volume = 0.5f;
                root.AddComponent<MusicController>();
            }

            return lc;
        }

        // -------- spawner --------

        private static void BuildSpawner(Transform camT, LevelController level)
        {
            var go = new GameObject("LevelSpawner");
            var sp = go.AddComponent<LevelSpawner>();
            var so = new SerializedObject(sp);
            so.FindProperty("level").objectReferenceValue = level;
            so.FindProperty("spawnReference").objectReferenceValue = camT;
            so.FindProperty("spawnAheadDistance").floatValue = 15f;
            so.FindProperty("groundY").floatValue = -2f;
            so.ApplyModifiedProperties();
        }

        // -------- UI --------

        private static void BuildUI()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(UIPrefabPath);
            if (prefab == null) { Debug.LogWarning($"[Kikiri-9] UI prefab missing at {UIPrefabPath}. Run Build UI Prefab."); return; }
            PrefabUtility.InstantiatePrefab(prefab);
        }

        // -------- helpers --------

        private static Sprite FindSubSprite(string path, string spriteName)
        {
            var reps = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            foreach (var o in reps) if (o is Sprite s && s.name == spriteName) return s;
            return null;
        }

        private static Sprite FindFirstSprite(string path)
        {
            var reps = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            foreach (var o in reps) if (o is Sprite s) return s;
            return null;
        }
    }
}
