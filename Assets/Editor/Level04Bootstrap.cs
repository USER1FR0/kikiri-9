// Construye el Nivel 4 (#14): recorrido 2000m + Coronela como jefe final. Sigue el
// mismo patron que Levels2And3Bootstrap.cs (copia Level01, reemplaza LevelDefinition,
// fondos adaptativos V3) y que MiniBossMomia/MiniBossSpawner (#12) para la parte de
// jefe, pero sin reanudar la carrera al derrotarla: es el final del juego.
//
// Menu: Tools > Kikiri-9 > Build Level 4

using System.IO;
using System.Linq;
using Kikiri9.Enemies;
using Kikiri9.Levels;
using Kikiri9.Session;
using Kikiri9.UI;
using Kikiri9.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class Level04Bootstrap
    {
        private const string CoronelaSheetPath = "Assets/_Draft_2_Kikiri9/Art/Characters/coronela.png";
        private const string PrefabBread = "Assets/_Project/Prefabs/Pickups/Pickup_Bread.prefab";
        private const string PrefabBottleGreen = "Assets/_Project/Prefabs/Pickups/Pickup_Bottle_Green.prefab";
        private const string PrefabBottleGold = "Assets/_Project/Prefabs/Pickups/Pickup_Bottle_Gold.prefab";
        private const string PrefabBreadBoost = "Assets/_Project/Prefabs/Pickups/Pickup_BreadBoost.prefab";
        private const string PrefabObstacleTomb = "Assets/_Project/Prefabs/Obstacles/Obstacle_Tomb.prefab";
        private const string PrefabLowBranch = "Assets/_Project/Prefabs/Obstacles/Obstacle_LowBranch.prefab";

        private const string PrefabEnemyMummy = "Assets/_Project/Prefabs/Enemies/Enemy_Mummy.prefab";
        private const string PrefabEnemyZombie = "Assets/_Project/Prefabs/Enemies/Enemy_Zombie.prefab";
        private const string PrefabMummyResistant = "Assets/_Project/Prefabs/Enemies/Enemy_MummyResistant.prefab";
        private const string PrefabZombieResistant = "Assets/_Project/Prefabs/Enemies/Enemy_ZombieResistant.prefab";
        private const string PrefabMariachi = "Assets/_Project/Prefabs/Enemies/Enemy_Mariachi.prefab";
        private const string PrefabXolo = "Assets/_Project/Prefabs/Enemies/Enemy_Xolo.prefab";
        private const string PrefabZombiSaltarin = "Assets/_Project/Prefabs/Enemies/Enemy_ZombiSaltarin.prefab";

        private const string PrefabCoronelaBoss = "Assets/_Project/Prefabs/Enemies/Enemy_CoronelaBoss.prefab";
        private const string PrefabCoronelaProjectile = "Assets/_Project/Prefabs/Enemies/Coronela_SpectralProjectile.prefab";

        private const string Level04SO = "Assets/_Project/ScriptableObjects/Levels/Level04_Guanajuato.asset";
        private const string Level01Scene = "Assets/_Project/Scenes/Level01_Dolores.unity";
        private const string Level03Scene = "Assets/_Project/Scenes/Level03_Guanajuato.unity";
        private const string Level04Scene = "Assets/_Project/Scenes/Level04_Guanajuato.unity";

        [MenuItem("Tools/Kikiri-9/Build Level 4")]
        public static void BuildAll()
        {
            if (!File.Exists(Level01Scene))
            { Debug.LogError("[Kikiri-9] Level01 scene missing."); return; }

            CreateCoronelaProjectile();
            CreateCoronelaBoss();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CreateLevelDefinition();

            BuildLevel04Scene();
            PatchLevel03NextScene();
            RegisterSceneInBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Kikiri-9] Nivel 4 listo: escena, jefe final y enlace desde el Nivel 3.");
        }

        // -------- Prefabs --------

        private const string GhostOrbSpritePath = "Assets/_Project/Generated/Coronela_GhostOrb.png";

        private static void CreateCoronelaProjectile()
        {
            // No reusamos un frame del cuerpo de la Coronela (coronela.png) para el
            // proyectil: esos frames tienen el pivote cerca de los pies (pensado para
            // caminar sobre el piso), asi que un CircleCollider2D centrado en el
            // transform quedaria descentrado respecto a lo que se ve dibujado (el
            // circulo de colision se extiende por debajo del dibujo, donde no hay
            // nada visible, y puede "tocar" al jugador aunque el fantasma ya haya
            // pasado por arriba visualmente). En vez de eso generamos un sprite
            // circular propio: su pivote queda exacto al centro, igual que el
            // CircleCollider2D, as que lo que se ve coincide con lo que da dano.
            var sprite = GetOrCreateGhostOrbSprite();

            var go = new GameObject("Coronela_SpectralProjectile", typeof(SpriteRenderer), typeof(CircleCollider2D));
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 14;
            go.transform.localScale = Vector3.one;

            var col = go.GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.28f; // un poco mas chico que el sprite (radio 0.25) para que el golpe se sienta justo

            go.AddComponent<CoronelaSpectralProjectile>();

            if (File.Exists(PrefabCoronelaProjectile)) AssetDatabase.DeleteAsset(PrefabCoronelaProjectile);
            PrefabUtility.SaveAsPrefabAsset(go, PrefabCoronelaProjectile);
            Object.DestroyImmediate(go);
        }

        private static Sprite GetOrCreateGhostOrbSprite()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(GhostOrbSpritePath);
            if (existing != null) return existing;

            var dir = Path.GetDirectoryName(GhostOrbSpritePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            const int size = 64; // 64px / 128 ppu = 0.5 unidades de diametro (radio 0.25)
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var purple = new Color(0.65f, 0.35f, 0.95f);
            float center = (size - 1) / 2f;
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = Mathf.Clamp01(1f - dist / radius);
                    float alpha = Mathf.SmoothStep(0f, 1f, t);
                    tex.SetPixel(x, y, new Color(purple.r, purple.g, purple.b, alpha));
                }
            }
            tex.Apply();

            File.WriteAllBytes(GhostOrbSpritePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(GhostOrbSpritePath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(GhostOrbSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 128;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(GhostOrbSpritePath);
        }

        private static void CreateCoronelaBoss()
        {
            var caminar = LoadFrames("caminar", 6);
            var ataque = LoadFrames("ataque", 6);
            var derrota = LoadFrames("derrota", 6);

            var go = new GameObject("Enemy_CoronelaBoss", typeof(SpriteRenderer), typeof(CapsuleCollider2D));
            go.tag = "Enemy";
            go.transform.localScale = Vector3.one * 1.3f;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = caminar.Length > 0 ? caminar[0] : null;
            sr.sortingOrder = 10;

            var col = go.GetComponent<CapsuleCollider2D>();
            col.size = new Vector2(1.4f, 2f);
            col.isTrigger = false;

            var health = go.AddComponent<EnemyHealth>();
            var soHealth = new SerializedObject(health);
            soHealth.FindProperty("maxHealth").intValue = 10;
            soHealth.FindProperty("defeatPoints").intValue = 2000;
            // Mucho mas que endDelay + duracion de la animacion de derrota (ver CoronelaBoss):
            // si EnemyHealth destruye el GameObject antes de que termine su propia
            // secuencia de victoria, SetState(LevelComplete) nunca se llamaria.
            soHealth.FindProperty("destroyAfterDefeat").floatValue = 15f;
            soHealth.ApplyModifiedProperties();

            go.AddComponent<EnemyDamageFlash>();
            go.AddComponent<EnemyHealthBar>();

            var boss = go.AddComponent<CoronelaBoss>();
            var soBoss = new SerializedObject(boss);
            soBoss.FindProperty("projectilePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabCoronelaProjectile);
            soBoss.FindProperty("breadDropPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBread);
            SetSpriteArray(soBoss, "caminarFrames", caminar);
            SetSpriteArray(soBoss, "ataqueFrames", ataque);
            SetSpriteArray(soBoss, "derrotaFrames", derrota);
            soBoss.ApplyModifiedProperties();

            if (File.Exists(PrefabCoronelaBoss)) AssetDatabase.DeleteAsset(PrefabCoronelaBoss);
            PrefabUtility.SaveAsPrefabAsset(go, PrefabCoronelaBoss);
            Object.DestroyImmediate(go);
        }

        private static Sprite[] LoadFrames(string prefix, int count)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
                frames[i] = FindSubSprite(CoronelaSheetPath, $"{prefix}_{(i + 1):00}");
            return frames;
        }

        private static void SetSpriteArray(SerializedObject so, string propertyName, Sprite[] sprites)
        {
            var prop = so.FindProperty(propertyName);
            prop.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static Sprite FindSubSprite(string path, string spriteName)
        {
            var reps = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            foreach (var o in reps) if (o is Sprite sp && sp.name == spriteName) return sp;
            return null;
        }

        // -------- Level Definition --------

        private static void CreateLevelDefinition()
        {
            var enemyPrefabs = new[] { PrefabEnemyMummy, PrefabEnemyZombie, PrefabMummyResistant, PrefabZombieResistant, PrefabMariachi, PrefabXolo, PrefabZombiSaltarin }
                .Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p)).Where(g => g != null).ToArray();
            var obstaclePrefabs = new[] { PrefabObstacleTomb, PrefabLowBranch }
                .Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p)).Where(g => g != null).ToArray();
            var pickupPrefabs = new[] { PrefabBread, PrefabBread, PrefabBottleGreen, PrefabBottleGold, PrefabBreadBoost }
                .Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p)).Where(g => g != null).ToArray();

            var def = ScriptableObject.CreateInstance<LevelDefinition>();
            def.levelName = "Guanajuato - Etapa Final";
            def.worldSpeed = 8.5f;
            def.maxSpeed = 11f;
            // Por encima de triggerMeters (2000): el mundo se congela al llegar ahi
            // (BossArenaManager.IsActive) y la unica forma de terminar el nivel es
            // derrotando a la Coronela (CoronelaBoss llama a SetState(LevelComplete)
            // directamente). Este valor nunca se alcanza corriendo (el mundo ya esta
            // congelado desde los 2000m).
            def.targetDistance = 5000f;
            def.enemyPrefabs = enemyPrefabs;
            def.obstaclePrefabs = obstaclePrefabs;
            def.pickupPrefabs = pickupPrefabs;
            def.spawnIntervalRange = new Vector2(0.8f, 1.3f);
            def.maxActiveEntities = 8;
            def.pickupHighChance = 0.5f;
            def.minSpawnSeparation = 6f;
            def.maxJumpsInAir = 1;
            def.breadLowRatio = 1.5f;
            def.enemyWeight = 70f;
            def.obstacleWeight = 8f;
            def.pickupWeight = 22f;
            def.safeStartMeters = 40f;
            def.goldBottlesReward = 2;
            def.tutorialText = "Ultima etapa: enemigos combinados a maxima velocidad.\n"
                + "Al llegar a 2000m el camino se corta: aparece la Coronela.\n"
                + "Lanzale pan (J / Click) para quitarle sus 10 puntos de vida.\n"
                + "Su proyectil espectral quita un corazon: cuidado.";
            def.endingNarrative = "La Coronela cae entre las primeras luces del amanecer.\n"
                + "Kikiri-9 por fin puede descansar: el ultimo pan ha llegado a quien lo esperaba.\n"
                + "Fin.";

            if (AssetDatabase.LoadAssetAtPath<LevelDefinition>(Level04SO) != null) AssetDatabase.DeleteAsset(Level04SO);
            AssetDatabase.CreateAsset(def, Level04SO);
        }

        // -------- Scene --------

        private static void BuildLevel04Scene()
        {
            AssetDatabase.CopyAsset(Level01Scene, Level04Scene);
            var scene = EditorSceneManager.OpenScene(Level04Scene, OpenSceneMode.Single);
            var def = AssetDatabase.LoadAssetAtPath<LevelDefinition>(Level04SO);

            var session = GameObject.Find("Session");
            if (session != null)
            {
                if (session.GetComponent<BossArenaManager>() == null)
                    session.AddComponent<BossArenaManager>();

                var hook = session.GetComponent<LevelIntroHook>();
                if (hook == null) hook = session.AddComponent<LevelIntroHook>();
                var soh = new SerializedObject(hook);
                soh.FindProperty("levelNumberLabel").stringValue = "NIVEL 4";
                var panSprite = FindSubSprite("Assets/_Draft_2_Kikiri9/Art/UI/interfaz.png", "pan");
                if (panSprite != null)
                    soh.FindProperty("breadIconSprite").objectReferenceValue = panSprite;
                soh.ApplyModifiedProperties();

                var bossSp = session.GetComponent<CoronelaBossSpawner>();
                if (bossSp == null) bossSp = session.AddComponent<CoronelaBossSpawner>();
                var boss = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabCoronelaBoss);
                var sob = new SerializedObject(bossSp);
                sob.FindProperty("bossPrefab").objectReferenceValue = boss;
                sob.FindProperty("triggerMeters").floatValue = 2000f;
                sob.ApplyModifiedProperties();

                var lc = session.GetComponent<LevelController>();
                if (lc != null && def != null)
                {
                    var so = new SerializedObject(lc);
                    so.FindProperty("definition").objectReferenceValue = def;
                    so.ApplyModifiedProperties();
                }

                var ws = session.GetComponent<WorldScroller>();
                if (ws != null && def != null)
                {
                    var so = new SerializedObject(ws);
                    so.FindProperty("initialSpeed").floatValue = def.worldSpeed;
                    so.ApplyModifiedProperties();
                }
            }

            var panels = Object.FindObjectsByType<LevelCompletePanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var p in panels)
            {
                // No hay "siguiente nivel" despues de la Coronela: tanto Siguiente como
                // Menu regresan al MainMenu.
                var so = new SerializedObject(p);
                so.FindProperty("nextSceneName").stringValue = "MainMenu";
                so.ApplyModifiedProperties();
            }

            BackgroundsV3Builder.Build(AdaptiveBgVariant.Guanajuato);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, Level04Scene);
        }

        // Conecta el "Siguiente" del Nivel 3 (hoy apunta a MainMenu como placeholder,
        // porque el Nivel 4 todavia no existia) hacia el Nivel 4 real.
        private static void PatchLevel03NextScene()
        {
            if (!File.Exists(Level03Scene)) return;

            var scene = EditorSceneManager.OpenScene(Level03Scene, OpenSceneMode.Single);
            var panels = Object.FindObjectsByType<LevelCompletePanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool changed = false;
            foreach (var p in panels)
            {
                var so = new SerializedObject(p);
                var prop = so.FindProperty("nextSceneName");
                if (prop.stringValue != "Level04_Guanajuato")
                {
                    prop.stringValue = "Level04_Guanajuato";
                    so.ApplyModifiedProperties();
                    changed = true;
                }
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, Level03Scene);
            }
        }

        private static void RegisterSceneInBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(s => s.path == Level04Scene);
            scenes.Add(new EditorBuildSettingsScene(Level04Scene, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
