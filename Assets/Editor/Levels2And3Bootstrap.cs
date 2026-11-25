// Construye Niveles 2 y 3. Usa el sistema V3 de fondos adaptativos (una sola instancia
// por capa, auto-escala al viewport). Mini-boss estilo Metal Slug (freeza mapa en el arena).
//
// Menu: Tools > Kikiri-9 > Build Levels 2 And 3

using System.IO;
using System.Linq;
using Kikiri9.Enemies;
using Kikiri9.Levels;
using Kikiri9.UI;
using Kikiri9.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class Levels2And3Bootstrap
    {
        private const string PrefabEnemyZombie = "Assets/_Project/Prefabs/Enemies/Enemy_Zombie.prefab";
        private const string PrefabEnemyMummy  = "Assets/_Project/Prefabs/Enemies/Enemy_Mummy.prefab";
        private const string PrefabObstacleCart = "Assets/_Project/Prefabs/Obstacles/Obstacle_Cart.prefab";
        private const string PrefabObstacleTomb = "Assets/_Project/Prefabs/Obstacles/Obstacle_Tomb.prefab";
        private const string PrefabBread = "Assets/_Project/Prefabs/Pickups/Pickup_Bread.prefab";
        private const string PrefabBottleGreen = "Assets/_Project/Prefabs/Pickups/Pickup_Bottle_Green.prefab";
        private const string PrefabBottleGold  = "Assets/_Project/Prefabs/Pickups/Pickup_Bottle_Gold.prefab";

        private const string PrefabZombieResistant = "Assets/_Project/Prefabs/Enemies/Enemy_ZombieResistant.prefab";
        private const string PrefabMummyResistant  = "Assets/_Project/Prefabs/Enemies/Enemy_MummyResistant.prefab";
        private const string PrefabLowBranch = "Assets/_Project/Prefabs/Obstacles/Obstacle_LowBranch.prefab";
        private const string PrefabBreadBoost = "Assets/_Project/Prefabs/Pickups/Pickup_BreadBoost.prefab";
        private const string PrefabMiniBoss = "Assets/_Project/Prefabs/Enemies/Enemy_MiniBossMomia.prefab";

        private const string Level01SO = "Assets/_Project/ScriptableObjects/Levels/Level01_Dolores.asset";
        private const string Level02SO = "Assets/_Project/ScriptableObjects/Levels/Level02_Dolores.asset";
        private const string Level03SO = "Assets/_Project/ScriptableObjects/Levels/Level03_Guanajuato.asset";

        private const string Level01Scene = "Assets/_Project/Scenes/Level01_Dolores.unity";
        private const string Level02Scene = "Assets/_Project/Scenes/Level02_Dolores.unity";
        private const string Level03Scene = "Assets/_Project/Scenes/Level03_Guanajuato.unity";

        private static readonly Color ZombieResistantTint = new Color(0.55f, 1f, 0.55f, 1f);
        private static readonly Color MummyResistantTint  = new Color(1f, 0.85f, 0.4f, 1f);

        [MenuItem("Tools/Kikiri-9/Build Levels 2 And 3")]
        public static void BuildAll()
        {
            if (!File.Exists(Level01Scene))
            { Debug.LogError("[Kikiri-9] Level01 scene missing."); return; }

            BackgroundsV3Importer.ImportAll();

            CreateResistantVariant(PrefabEnemyZombie, PrefabZombieResistant, ZombieResistantTint, 2, 200);
            CreateResistantVariant(PrefabEnemyMummy, PrefabMummyResistant, MummyResistantTint, 2, 200);
            CreateLowBranch();
            CreateBreadBoost();
            CreateMiniBoss();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CreateLevelDefinition(Level02SO,
                levelName: "Dolores Hidalgo - Etapa 2",
                worldSpeed: 7.5f, maxSpeed: 10f, targetDistance: 750f,
                enemyPrefabs: new[] { PrefabEnemyZombie, PrefabEnemyZombie, PrefabEnemyZombie, PrefabZombieResistant, PrefabZombieResistant },
                obstaclePrefabs: new[] { PrefabObstacleCart, PrefabLowBranch, PrefabLowBranch },
                pickupPrefabs: new[] { PrefabBread, PrefabBread, PrefabBottleGreen, PrefabBottleGold, PrefabBreadBoost },
                spawnInterval: new Vector2(1.0f, 1.7f),
                maxActive: 7,
                pickupHighChance: 0.35f,
                minSeparation: 7f,
                maxJumpsInAir: 1,
                breadLowRatio: 1.5f,
                enemyWeight: 60f,
                obstacleWeight: 10f,
                pickupWeight: 30f,
                safeStart: 30f,
                tutorial: "Nueva etapa: zombis verdes (2 panes) y ramas bajas (deslizate con S o Shift).\nAlgunas monedas flotan: saltalas. Pan dorado = pan infinito por 10s.\nMeta: 750 metros.",
                ending: "Dolores queda atras. Guanajuato espera entre callejones.");

            CreateLevelDefinition(Level03SO,
                levelName: "Guanajuato - Etapa 3",
                worldSpeed: 9f, maxSpeed: 12f, targetDistance: 1200f,
                enemyPrefabs: new[] { PrefabEnemyMummy, PrefabEnemyMummy, PrefabEnemyMummy, PrefabEnemyZombie, PrefabMummyResistant, PrefabMummyResistant, PrefabZombieResistant },
                obstaclePrefabs: new[] { PrefabObstacleTomb, PrefabLowBranch },
                pickupPrefabs: new[] { PrefabBread, PrefabBread, PrefabBottleGreen, PrefabBottleGold, PrefabBreadBoost },
                spawnInterval: new Vector2(0.9f, 1.5f),
                maxActive: 7,
                pickupHighChance: 0.45f,
                minSeparation: 6.5f,
                maxJumpsInAir: 1,
                breadLowRatio: 1.5f,
                enemyWeight: 65f,
                obstacleWeight: 8f,
                pickupWeight: 27f,
                safeStart: 40f,
                tutorial: "Momias doradas aguantan 2 panes. Al llegar a 1000m aparece la Momia Jefe: el mapa se congela y pelea en el arena.\nMeta: 1200 metros. Pan dorado dura 10s.",
                ending: "El Callejon del Beso queda atras. Kikiri-9 llega a su proximo horizonte.");

            BuildLevelSceneFromLevel01(Level02Scene, Level02SO, nextSceneName: "Level03_Guanajuato", variant: AdaptiveBgVariant.DoloresHidalgo);
            BuildLevelSceneFromLevel01(Level03Scene, Level03SO, nextSceneName: "MainMenu", variant: AdaptiveBgVariant.Guanajuato);

            RegisterNewScenesInBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Kikiri-9] Levels 2 y 3 listos con fondos adaptativos V3.");
        }

        // -------- Prefab variants --------

        private static void CreateResistantVariant(string sourcePath, string destPath, Color tint, int maxHealth, int defeatPoints)
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (src == null) { Debug.LogError($"[Kikiri-9] Source prefab missing: {sourcePath}"); return; }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(src);
            if (instance == null) return;
            instance.name = Path.GetFileNameWithoutExtension(destPath);

            var sr = instance.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = tint;

            var health = instance.GetComponent<EnemyHealth>();
            if (health != null)
            {
                var so = new SerializedObject(health);
                so.FindProperty("maxHealth").intValue = maxHealth;
                so.FindProperty("defeatPoints").intValue = defeatPoints;
                so.ApplyModifiedProperties();
            }

            if (instance.GetComponent<EnemyDamageFlash>() == null) instance.AddComponent<EnemyDamageFlash>();
            if (instance.GetComponent<EnemyHealthBar>() == null) instance.AddComponent<EnemyHealthBar>();

            PrefabUtility.SaveAsPrefabAsset(instance, destPath);
            Object.DestroyImmediate(instance);
        }

        private static void CreateLowBranch()
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabObstacleCart);
            if (src == null) { Debug.LogError("[Kikiri-9] Obstacle_Cart missing"); return; }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(src);
            instance.name = "Obstacle_LowBranch";

            var box = instance.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.offset = new Vector2(0f, 1.6f);
                box.size = new Vector2(1.6f, 1.2f);
            }
            var sr = instance.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.85f, 0.7f, 0.5f, 1f);

            PrefabUtility.SaveAsPrefabAsset(instance, PrefabLowBranch);
            Object.DestroyImmediate(instance);
        }

        private static void CreateBreadBoost()
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBread);
            if (src == null) { Debug.LogError("[Kikiri-9] Pickup_Bread missing"); return; }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(src);
            instance.name = "Pickup_BreadBoost";

            var normal = instance.GetComponent<BreadPickup>();
            if (normal != null) Object.DestroyImmediate(normal);
            var boost = instance.GetComponent<BreadBoostPickup>();
            if (boost == null) boost = instance.AddComponent<BreadBoostPickup>();
            var soBoost = new SerializedObject(boost);
            soBoost.FindProperty("duration").floatValue = 10f;
            soBoost.ApplyModifiedProperties();

            // Sprite base del pan intacto (sin tint): le vuelan destellos de cempasuchil alrededor
            var normalSr = instance.GetComponentInChildren<SpriteRenderer>();
            if (normalSr != null) normalSr.color = Color.white;

            var aura = instance.GetComponent<Kikiri9.World.BreadBoostAura>();
            if (aura == null) aura = instance.AddComponent<Kikiri9.World.BreadBoostAura>();

            // Chispas: usar cempasuchil (marigold Dia de Muertos) del sheet de electricidad/activacion/cempasuchil
            var sparkle = FindSubSprite("Assets/_Draft_2_Kikiri9/Art/Effects/electricidad_activacion_cempasuchil.png", "cempasuchil_01");
            if (sparkle == null) sparkle = FindSubSprite("Assets/_Draft_2_Kikiri9/Art/Effects/electricidad_activacion_cempasuchil.png", "cempasuchil_02");
            if (sparkle != null) aura.SetSparkleSprite(sparkle);

            PrefabUtility.SaveAsPrefabAsset(instance, PrefabBreadBoost);
            Object.DestroyImmediate(instance);
        }

        private static void CreateMiniBoss()
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabEnemyMummy);
            if (src == null) { Debug.LogError("[Kikiri-9] Enemy_Mummy missing"); return; }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(src);
            instance.name = "Enemy_MiniBossMomia";
            instance.transform.localScale = instance.transform.localScale * 1.3f;

            var sr = instance.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1f, 0.6f, 0.2f, 1f);

            var health = instance.GetComponent<EnemyHealth>();
            if (health != null)
            {
                var so = new SerializedObject(health);
                so.FindProperty("maxHealth").intValue = 6;
                so.FindProperty("defeatPoints").intValue = 500;
                so.ApplyModifiedProperties();
            }

            var mov = instance.GetComponent<EnemyMovement>();
            if (mov != null) mov.enabled = false;

            if (instance.GetComponent<EnemyDamageFlash>() == null) instance.AddComponent<EnemyDamageFlash>();
            if (instance.GetComponent<EnemyHealthBar>() == null) instance.AddComponent<EnemyHealthBar>();

            var boss = instance.GetComponent<MiniBossMomia>();
            if (boss == null) boss = instance.AddComponent<MiniBossMomia>();
            var breadRef = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBread);
            var boostRef = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBreadBoost);
            var bottleRef = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBottleGreen);
            var soBoss = new SerializedObject(boss);
            soBoss.FindProperty("breadDropPrefab").objectReferenceValue = breadRef;
            soBoss.FindProperty("breadBoostPrefab").objectReferenceValue = boostRef;
            soBoss.FindProperty("bottleDropPrefab").objectReferenceValue = bottleRef;
            soBoss.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(instance, PrefabMiniBoss);
            Object.DestroyImmediate(instance);
        }

        // -------- Level Definitions --------

        private static void CreateLevelDefinition(string path, string levelName,
            float worldSpeed, float maxSpeed, float targetDistance,
            string[] enemyPrefabs, string[] obstaclePrefabs, string[] pickupPrefabs,
            Vector2 spawnInterval, int maxActive, string tutorial, string ending,
            float pickupHighChance = 0f, float minSeparation = 7f,
            int maxJumpsInAir = 2, float breadLowRatio = 0f,
            float enemyWeight = 0f, float obstacleWeight = 0f, float pickupWeight = 0f,
            float safeStart = 0f)
        {
            var baseDef = AssetDatabase.LoadAssetAtPath<LevelDefinition>(Level01SO);
            var def = ScriptableObject.CreateInstance<LevelDefinition>();
            def.levelName = levelName;
            def.musicClip = baseDef != null ? baseDef.musicClip : null;
            def.worldSpeed = worldSpeed;
            def.maxSpeed = maxSpeed;
            def.targetDistance = targetDistance;
            def.enemyPrefabs = enemyPrefabs.Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p)).Where(g => g != null).ToArray();
            def.obstaclePrefabs = obstaclePrefabs.Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p)).Where(g => g != null).ToArray();
            def.pickupPrefabs = pickupPrefabs.Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p)).Where(g => g != null).ToArray();
            def.spawnIntervalRange = spawnInterval;
            def.maxActiveEntities = maxActive;
            def.tutorialText = tutorial;
            def.endingNarrative = ending;
            def.goldBottlesReward = 1;
            def.pickupHighChance = pickupHighChance;
            def.minSpawnSeparation = minSeparation;
            def.maxJumpsInAir = maxJumpsInAir;
            def.breadLowRatio = breadLowRatio;
            def.enemyWeight = enemyWeight;
            def.obstacleWeight = obstacleWeight;
            def.pickupWeight = pickupWeight;
            def.safeStartMeters = safeStart;

            if (AssetDatabase.LoadAssetAtPath<LevelDefinition>(path) != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(def, path);
        }

        public static Sprite FindSubSprite(string path, string spriteName)
        {
            var reps = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            foreach (var o in reps) if (o is Sprite sp && sp.name == spriteName) return sp;
            return null;
        }

        // -------- Scenes --------

        private static void BuildLevelSceneFromLevel01(string destPath, string levelSOPath, string nextSceneName, AdaptiveBgVariant variant)
        {
            AssetDatabase.CopyAsset(Level01Scene, destPath);
            var scene = EditorSceneManager.OpenScene(destPath, OpenSceneMode.Single);
            var def = AssetDatabase.LoadAssetAtPath<LevelDefinition>(levelSOPath);

            var session = GameObject.Find("Session");
            if (session != null)
            {
                // BossArenaManager en todas las escenas (idempotente)
                if (session.GetComponent<Kikiri9.Session.BossArenaManager>() == null)
                    session.AddComponent<Kikiri9.Session.BossArenaManager>();

                var hook = session.GetComponent<Kikiri9.UI.LevelIntroHook>();
                if (hook == null) hook = session.AddComponent<Kikiri9.UI.LevelIntroHook>();
                string label = destPath.Contains("Level02") ? "NIVEL 2" : destPath.Contains("Level03") ? "NIVEL 3" : "NIVEL ?";
                var soh = new SerializedObject(hook);
                soh.FindProperty("levelNumberLabel").stringValue = label;
                var panSprite = FindSubSprite("Assets/_Draft_2_Kikiri9/Art/UI/interfaz.png", "pan");
                if (panSprite != null)
                    soh.FindProperty("breadIconSprite").objectReferenceValue = panSprite;
                soh.ApplyModifiedProperties();

                if (destPath.Contains("Level03"))
                {
                    var bossSp = session.GetComponent<Kikiri9.Levels.MiniBossSpawner>();
                    if (bossSp == null) bossSp = session.AddComponent<Kikiri9.Levels.MiniBossSpawner>();
                    var boss = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabMiniBoss);
                    var sob = new SerializedObject(bossSp);
                    sob.FindProperty("bossPrefab").objectReferenceValue = boss;
                    sob.FindProperty("triggerMeters").floatValue = 1000f;
                    sob.ApplyModifiedProperties();
                }
            }

            if (session != null && def != null)
            {
                var lc = session.GetComponent<LevelController>();
                if (lc != null)
                {
                    var so = new SerializedObject(lc);
                    so.FindProperty("definition").objectReferenceValue = def;
                    so.ApplyModifiedProperties();
                }
                var ws = session.GetComponent<WorldScroller>();
                if (ws != null)
                {
                    var so = new SerializedObject(ws);
                    so.FindProperty("initialSpeed").floatValue = def.worldSpeed;
                    so.ApplyModifiedProperties();
                }
            }

            var panels = Object.FindObjectsByType<LevelCompletePanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var p in panels)
            {
                var so = new SerializedObject(p);
                so.FindProperty("nextSceneName").stringValue = nextSceneName;
                so.ApplyModifiedProperties();
            }

            // Fondos adaptativos V3
            BackgroundsV3Builder.Build(variant);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, destPath);
        }

        private static void RegisterNewScenesInBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(s => s.path == Level02Scene || s.path == Level03Scene);
            scenes.Add(new EditorBuildSettingsScene(Level02Scene, true));
            scenes.Add(new EditorBuildSettingsScene(Level03Scene, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
