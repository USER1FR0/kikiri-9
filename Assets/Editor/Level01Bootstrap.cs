// Editor-only bootstrap for Nivel 1 (Dolores Hidalgo). Builds, in one menu action:
//   - Zombie animations and controller (reuse of EnemyAnimationBuilder's pattern).
//   - Prefabs: Enemy_Zombie, Obstacle_Tomb, Obstacle_Cart.
//   - ScriptableObject: Level01_Dolores (LevelDefinition) wired to the prefabs.
//
// Run order:
//   1. Tools > Kikiri-9 > Slice Sprite Sheets (asegurarse de que esta hecho)
//   2. Tools > Kikiri-9 > Build Level 01 Assets   <-- este archivo
//   3. Tools > Kikiri-9 > Build Level 01 Scene    (fase 4)

using System.Collections.Generic;
using System.IO;
using Kikiri9.Enemies;
using Kikiri9.Levels;
using Kikiri9.World;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class Level01Bootstrap
    {
        private const string ArtRoot = "Assets/_Draft_2_Kikiri9/Art";
        private const string ZombieSheet = ArtRoot + "/Characters/zombi.png";
        private const string ObstaclesSheet = ArtRoot + "/Environment/obstaculos_decoracion.png";

        private const string AnimDir = "Assets/_Project/Animations";
        private const string ZombieControllerPath = AnimDir + "/Zombi.controller";

        private const string EnemyPrefabDir = "Assets/_Project/Prefabs/Enemies";
        private const string ObstaclePrefabDir = "Assets/_Project/Prefabs/Obstacles";
        private const string LevelSoDir = "Assets/_Project/ScriptableObjects/Levels";

        private const string ZombiePrefabPath = EnemyPrefabDir + "/Enemy_Zombie.prefab";
        private const string TombPrefabPath = ObstaclePrefabDir + "/Obstacle_Tomb.prefab";
        private const string CartPrefabPath = ObstaclePrefabDir + "/Obstacle_Cart.prefab";
        private const string PickupPrefabDir = "Assets/_Project/Prefabs/Pickups";
        private const string BottleGreenPrefabPath = PickupPrefabDir + "/Pickup_Bottle_Green.prefab";
        private const string BottleGoldPrefabPath = PickupPrefabDir + "/Pickup_Bottle_Gold.prefab";
        private const string BreadPrefabPath = PickupPrefabDir + "/Pickup_Bread.prefab";
        private const string LevelAssetPath = LevelSoDir + "/Level01_Dolores.asset";
        private const string ItemsSheet = "Assets/_Draft_2_Kikiri9/Art/Items/objetos_efectos.png";

        [MenuItem("Tools/Kikiri-9/Build Level 01 Assets")]
        public static void Build()
        {
            Directory.CreateDirectory(AnimDir);
            Directory.CreateDirectory(EnemyPrefabDir);
            Directory.CreateDirectory(ObstaclePrefabDir);
            Directory.CreateDirectory(PickupPrefabDir);
            Directory.CreateDirectory(LevelSoDir);

            BuildZombieController();
            var zombie = BuildZombiePrefab();
            var tomb = BuildObstaclePrefab("Obstacle_Tomb", "obstaculos_01", TombPrefabPath);
            var cart = BuildObstaclePrefab("Obstacle_Cart", "obstaculos_02", CartPrefabPath);
            var green = BuildBottlePrefab("Pickup_Bottle_Green", "objetos_03", "efectos_a_03", 25, BottleGreenPrefabPath);
            var gold = BuildBottlePrefab("Pickup_Bottle_Gold", "objetos_04", "efectos_b_04", 100, BottleGoldPrefabPath);
            var bread = BuildBreadPickupPrefab();
            BuildLevelDefinition(zombie, new[] { tomb, cart }, new[] { green, gold, bread });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Kikiri-9] Level 01 assets built: controller, prefabs y SO listos.");
        }

        // -------- Zombie controller --------

        private static void BuildZombieController()
        {
            var walkClip = CreateClip("Zombi_Walk", ZombieSheet, "caminar", 10, loop: true);
            var defeatClip = CreateClip("Zombi_Defeat", ZombieSheet, "derrota", 8, loop: false);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ZombieControllerPath);
            controller.AddParameter("Defeat", AnimatorControllerParameterType.Trigger);

            var sm = controller.layers[0].stateMachine;
            var walk = sm.AddState("Walk");
            walk.motion = walkClip;
            var defeat = sm.AddState("Defeat");
            defeat.motion = defeatClip;
            sm.defaultState = walk;

            var t = walk.AddTransition(defeat);
            t.hasExitTime = false;
            t.duration = 0f;
            t.AddCondition(AnimatorConditionMode.If, 0f, "Defeat");
        }

        // -------- Enemy_Zombie prefab --------

        private static GameObject BuildZombiePrefab()
        {
            var idle = FindSubSprite(ZombieSheet, "caminar_01");
            if (idle == null) { Debug.LogError("[Kikiri-9] Zombi idle sprite missing. Correr slicer primero."); return null; }

            var go = new GameObject("Enemy_Zombie");
            go.tag = "Enemy";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = idle;
            sr.sortingOrder = 10;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Zombi pivote (0.5, 0.125). El pie queda cerca de y=0 del transform.
            var cc = go.AddComponent<CapsuleCollider2D>();
            cc.direction = CapsuleDirection2D.Vertical;
            cc.size = new Vector2(1.3f, 1.9f);
            cc.offset = new Vector2(0f, 0.95f);

            var animator = go.AddComponent<Animator>();
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ZombieControllerPath);
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            go.AddComponent<EnemyMovement>();
            go.AddComponent<EnemyIgnoresSiblings>();
            var hp = go.AddComponent<EnemyHealth>();
            var so = new SerializedObject(hp);
            so.FindProperty("maxHealth").intValue = 1;
            so.FindProperty("defeatPoints").intValue = 100;
            var crumbsVfx = FindSubSprite("Assets/_Draft_2_Kikiri9/Art/Items/objetos_efectos.png", "efectos_b_01");
            if (crumbsVfx != null) so.FindProperty("defeatVfxSprite").objectReferenceValue = crumbsVfx;
            so.ApplyModifiedProperties();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ZombiePrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // -------- Obstacle prefabs --------

        private static GameObject BuildObstaclePrefab(string prefabName, string spriteName, string prefabPath)
        {
            var sprite = FindSubSprite(ObstaclesSheet, spriteName);
            if (sprite == null) { Debug.LogError($"[Kikiri-9] Obstacle sprite missing: {spriteName}"); return null; }

            var go = new GameObject(prefabName);
            go.tag = "Enemy"; // misma puerta de daño que los enemigos

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 8;

            // Pivote (0.5, 0.08): la base del sprite queda muy cerca de y=0 del transform.
            // Collider cubre casi toda la altura visual (~1.8 u) centrado un poco arriba.
            var bc = go.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(1.6f, 1.8f);
            bc.offset = new Vector2(0f, 0.9f);

            go.AddComponent<ObstacleScroller>();
            go.AddComponent<Kikiri9.Enemies.EnemyIgnoresSiblings>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // -------- Pickup prefabs (botellas) --------

        private static GameObject BuildBottlePrefab(string name, string spriteName, string vfxSpriteName, int points, string prefabPath)
        {
            var sprite = FindSubSprite(ItemsSheet, spriteName);
            if (sprite == null) { Debug.LogError($"[Kikiri-9] Pickup sprite missing: {spriteName}"); return null; }

            var go = new GameObject(name);
            go.transform.localScale = Vector3.one * 0.55f; // pequeñas
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 14;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.55f;
            col.offset = new Vector2(0f, 0.4f);

            // Despawn cuando queda atras de la camara
            go.AddComponent<Kikiri9.World.ObstacleScroller>();
            // Floater: ligero sube-baja + pulse
            go.AddComponent<Kikiri9.World.PickupFloater>();

            var pickup = go.AddComponent<Kikiri9.World.BottlePickup>();
            var so = new SerializedObject(pickup);
            so.FindProperty("points").intValue = points;
            var vfx = FindSubSprite(ItemsSheet, vfxSpriteName);
            if (vfx != null) so.FindProperty("pickupVfxSprite").objectReferenceValue = vfx;
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                points >= 100
                    ? "Assets/_Draft_2_Kikiri9/Audio/SFX/botella_dorada.wav"
                    : "Assets/_Draft_2_Kikiri9/Audio/SFX/recoger_botella.wav");
            if (clip != null) so.FindProperty("pickupClip").objectReferenceValue = clip;
            so.ApplyModifiedProperties();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject BuildBreadPickupPrefab()
        {
            var sprite = FindSubSprite(ItemsSheet, "objetos_01");
            if (sprite == null) { Debug.LogError("[Kikiri-9] bread sprite missing"); return null; }

            var go = new GameObject("Pickup_Bread");
            go.transform.localScale = Vector3.one * 0.45f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 14;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;
            col.offset = new Vector2(0f, 0.4f);

            go.AddComponent<Kikiri9.World.ObstacleScroller>();
            go.AddComponent<Kikiri9.World.PickupFloater>();

            var pickup = go.AddComponent<Kikiri9.World.BreadPickup>();
            var so = new SerializedObject(pickup);
            so.FindProperty("amount").intValue = 1;
            var vfx = FindSubSprite(ItemsSheet, "efectos_a_02");
            if (vfx != null) so.FindProperty("pickupVfxSprite").objectReferenceValue = vfx;
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Draft_2_Kikiri9/Audio/SFX/recoger_pan.wav");
            if (clip != null) so.FindProperty("pickupClip").objectReferenceValue = clip;
            so.ApplyModifiedProperties();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, BreadPrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // -------- Level01_Dolores SO --------

        private static void BuildLevelDefinition(GameObject zombiePrefab, GameObject[] obstaclePrefabs, GameObject[] pickupPrefabs)
        {
            var def = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelAssetPath);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<LevelDefinition>();
                AssetDatabase.CreateAsset(def, LevelAssetPath);
            }

            def.levelName = "Dolores Hidalgo - Etapa 1";
            def.worldSpeed = 3.5f;
            def.maxSpeed = 6f;
            def.targetDistance = 500f;
            def.endingNarrative = "Primera etapa superada. Las calles de Dolores Hidalgo siguen llenas de zombis. El pollo recupera el aliento y continua.";
            def.spawnIntervalRange = new Vector2(2f, 3.5f);
            def.maxActiveEntities = 5;
            def.goldBottlesReward = 1;
            def.enemyPrefabs = new[] { zombiePrefab };
            def.obstaclePrefabs = obstaclePrefabs;
            def.pickupPrefabs = pickupPrefabs;

            def.backgroundSprite = LoadBg("Assets/_Draft_2_Kikiri9/Art/Backgrounds/dolores_hidalgo.png");
            def.musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Draft_2_Kikiri9/Audio/Music/callejon_suspenso.wav");

            EditorUtility.SetDirty(def);
        }

        // -------- helpers --------

        private static AnimationClip CreateClip(string clipName, string sheetPath, string rowPrefix, int fps, bool loop)
        {
            var sprites = LoadRowSprites(sheetPath, rowPrefix);
            if (sprites.Count == 0)
            {
                Debug.LogWarning($"[Kikiri-9] No sprites '{rowPrefix}_*' in {sheetPath}");
                return null;
            }

            var clip = new AnimationClip { frameRate = fps };
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i / (float)fps, value = sprites[i] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var assetPath = $"{AnimDir}/{clipName}.anim";
            AssetDatabase.CreateAsset(clip, assetPath);
            return clip;
        }

        private static List<Sprite> LoadRowSprites(string sheetPath, string rowPrefix)
        {
            var list = new List<Sprite>();
            var reps = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheetPath);
            foreach (var o in reps)
                if (o is Sprite s && s.name.StartsWith(rowPrefix + "_"))
                    list.Add(s);
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return list;
        }

        private static Sprite FindSubSprite(string path, string spriteName)
        {
            var reps = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            foreach (var o in reps) if (o is Sprite s && s.name == spriteName) return s;
            return null;
        }

        private static Sprite LoadBg(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 128f);
        }
    }
}
