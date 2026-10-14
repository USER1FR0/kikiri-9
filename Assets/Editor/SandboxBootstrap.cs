// Editor-only bootstrap for the Kikiri-9 sandbox scene.
// Builds: parallax background layers keeping their natural height (so the moon in the
// sky is visible), infinite ground follower, pollo with full action set, camera follow.
//
// Run order:
//   1. Tools > Kikiri-9 > Slice Sprite Sheets
//   2. Tools > Kikiri-9 > Build Player Animations
//   3. Tools > Kikiri-9 > Setup Sandbox Scene

using System.IO;
using Kikiri9.CameraSystem;
using Kikiri9.Combat;
using Kikiri9.Player;
using Kikiri9.World;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class SandboxBootstrap
    {
        private const string ArtRoot = "Assets/_Draft_2_Kikiri9/Art";
        private const string GroundSheet = ArtRoot + "/Environment/suelo.png";
        private const string PolloSheet = ArtRoot + "/Characters/kikiri9.png";
        private const string ItemsSheet = ArtRoot + "/Items/objetos_efectos.png";
        private const string BgCielo = ArtRoot + "/Backgrounds/cielo.png";
        private const string BgSilueta = ArtRoot + "/Backgrounds/silueta_guanajuato.png";
        private const string BgCiudad = ArtRoot + "/Backgrounds/guanajuato.png";
        private const string ControllerPath = "Assets/_Project/Animations/Kikiri9.controller";
        private const string ScenePath = "Assets/_Project/Scenes/Sandbox.unity";

        [MenuItem("Tools/Kikiri-9/Setup Sandbox Scene")]
        public static void Setup()
        {
            Directory.CreateDirectory("Assets/_Project/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cam = BuildCamera();
            BuildBackground(cam.transform);
            var ground = BuildGround();
            var pollo = BuildPollo();

            if (pollo != null)
            {
                var follow = cam.gameObject.AddComponent<CameraFollow>();
                follow.SetTarget(pollo.transform);
            }
            // Ground is static in world space; no follower, no Rigidbody2D needed.

            Selection.activeGameObject = pollo;
            SceneView.FrameLastActiveSceneView();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Kikiri-9] Sandbox scene saved at {ScenePath}");
        }

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

        private static void BuildBackground(Transform camT)
        {
            // Single guanajuato background that TILES horizontally and parallaxes with wrap.
            var sprite = LoadBgSprite(BgCiudad);
            if (sprite == null) return;

            float naturalW = sprite.rect.width / sprite.pixelsPerUnit;
            float naturalH = sprite.rect.height / sprite.pixelsPerUnit;
            float viewportH = 9f;
            float scale = (viewportH / naturalH) * 0.85f;

            var go = new GameObject("Bg_Guanajuato");
            go.transform.position = new Vector3(0f, -0.3f, 10f);
            go.transform.localScale = new Vector3(scale, scale, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(naturalW * 5f, naturalH);
            sr.sortingOrder = -20;

            float tileWidthWorld = naturalW * scale;

            var pf = go.AddComponent<ParallaxLayer>();
            var so = new SerializedObject(pf);
            so.FindProperty("cameraTransform").objectReferenceValue = camT;
            so.FindProperty("parallaxFactor").floatValue = 0.4f;
            so.FindProperty("lockedY").floatValue = -0.3f;
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


        private static GameObject BuildGround()
        {
            var sprite = FindSubSprite(GroundSheet, "suelo_02") ?? FindFirstSprite(GroundSheet);
            if (sprite == null) { Debug.LogError($"[Kikiri-9] No ground sprite in {GroundSheet}"); return null; }

            // Static very-wide ground (1000 units). Pollo at runSpeed=4 would need ~4min to
            // reach the end. Not infinite, but plenty for the sandbox. The ground does NOT
            // follow the camera: it stays in world coordinates so the camera sliding over
            // it reveals new tiles, giving the perceived movement.
            //
            // Pivot (0.5, 0.826) → visible top sits at GO.y + (1 - 0.826) * sizeY.
            // Collider is offset so its top aligns with the visible top line.
            var go = new GameObject("Ground");
            go.transform.position = new Vector3(400f, -3.1f, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(1000f, 2f);
            sr.sortingOrder = -5; // behind the pollo, in front of background

            var bc = go.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(1000f, 2f);
            bc.offset = new Vector2(0f, -0.652f);

            return go;
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
            rb.gravityScale = 3f;
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
            muzzleGo.transform.localPosition = new Vector3(0.7f, 1.2f, 0f);

            var animator = go.AddComponent<Animator>();
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller != null) animator.runtimeAnimatorController = controller;
            else Debug.LogWarning($"[Kikiri-9] Controller not found at {ControllerPath}. Run 'Build Player Animations' first.");
            animator.applyRootMotion = false;

            var launcher = go.AddComponent<BreadLauncher>();
            // objetos_02 = pan proyectil (fila 1, col 2 del atlas Items/objetos_efectos.png)
            var breadSprite = FindSubSprite(ItemsSheet, "objetos_02");
            if (breadSprite == null) Debug.LogWarning("[Kikiri-9] Pan projectile sprite 'objetos_02' not found.");
            var soL = new SerializedObject(launcher);
            soL.FindProperty("muzzle").objectReferenceValue = muzzleGo.transform;
            if (breadSprite != null) soL.FindProperty("breadSprite").objectReferenceValue = breadSprite;
            soL.ApplyModifiedProperties();

            var pc = go.AddComponent<PlayerController>();
            var so = new SerializedObject(pc);
            so.FindProperty("groundCheck").objectReferenceValue = checkGo.transform;
            so.FindProperty("groundCheckRadius").floatValue = 0.18f;
            so.FindProperty("groundLayer").intValue = ~0;
            so.FindProperty("animator").objectReferenceValue = animator;
            so.FindProperty("breadLauncher").objectReferenceValue = launcher;
            so.ApplyModifiedProperties();

            return go;
        }

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
