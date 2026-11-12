// Installer de Etapa 3: crea prefabs de enemigos nuevos, Pickup_PanBomba,
// 3 WaveDefinition assets, agrega WaveManager a Level03, desactiva MiniBossSpawner,
// asigna sprites y audio donde puede. Idempotente.

using System.Collections.Generic;
using System.IO;
using Kikiri9.Combat.Bomba;
using Kikiri9.Enemies;
using Kikiri9.Enemies.New;
using Kikiri9.Levels.Waves;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class Etapa3Installer
    {
        private const string EnemiesPrefabDir = "Assets/_Project/Prefabs/Enemies";
        private const string PickupsPrefabDir = "Assets/_Project/Prefabs/Pickups";
        private const string WavesDir = "Assets/_Project/ScriptableObjects/Waves";
        private const string AnimDir = "Assets/_Project/Animations/New";
        private const string EnemiesArtDir = "Assets/_Project/Art/Enemies";
        private const string UIWavesDir = "Assets/_Project/Art/UI/Waves";
        private const string AudioSFXDir = "Assets/_Project/Audio/SFX";
        private const string AudioMusicDir = "Assets/_Project/Audio/Music";

        private const string Level3Scene = "Assets/_Project/Scenes/Level03_Guanajuato.unity";
        private const string Level1Scene = "Assets/_Project/Scenes/Level01_Dolores.unity";
        private const string Level2Scene = "Assets/_Project/Scenes/Level02_Dolores.unity";

        [MenuItem("Tools/Kikiri-9/Etapa 3 - Install All")]
        public static void InstallAll()
        {
            try
            {
                EnsureDir(EnemiesPrefabDir);
                EnsureDir(PickupsPrefabDir);
                EnsureDir(WavesDir);
                EnsureDir(AnimDir);

                NewContentImporter.ImportAll();

                var mariachi = BuildMariachiPrefab();
                var zombiSalt = BuildZombiSaltarinPrefab();
                var xolo = BuildXoloPrefab();
                var panBombaPickup = BuildPanBombaPickup();

                var oldZombie = LoadPrefab($"{EnemiesPrefabDir}/Enemy_Zombie.prefab");
                var oldMummy = LoadPrefab($"{EnemiesPrefabDir}/Enemy_Mummy.prefab");
                var oldMummyR = LoadPrefab($"{EnemiesPrefabDir}/Enemy_MummyResistant.prefab");
                var breadPickup = LoadPrefab($"{PickupsPrefabDir}/Pickup_Bread.prefab");
                var greenBottle = LoadPrefab($"{PickupsPrefabDir}/Pickup_Bottle_Green.prefab");
                var breadBoost = LoadPrefab($"{PickupsPrefabDir}/Pickup_BreadBoost.prefab");

                var e1 = new List<WaveEntry>();
                AddEntry(e1, oldZombie, 2);
                AddEntry(e1, oldMummy, 2);
                AddEntry(e1, zombiSalt, 1);
                // 1 reward por enemigo (2+2+1 = 5)
                var r1 = new List<WaveReward>();
                AddReward(r1, breadPickup, 3);
                AddReward(r1, greenBottle, 2);
                var w1 = BuildWaveDefinition("Wave_L3_01", 400f, "OLEADA 1/3", 0.9f, e1, r1, false);

                var e2 = new List<WaveEntry>();
                AddEntry(e2, oldMummyR, 3);
                AddEntry(e2, mariachi, 3);
                // 1 reward por enemigo (2+2+2 = 6)
                var r2 = new List<WaveReward>();
                AddReward(r2, breadPickup, 3);
                AddReward(r2, greenBottle, 2);
                AddReward(r2, panBombaPickup, 1);
                var w2 = BuildWaveDefinition("Wave_L3_02", 750f, "OLEADA 2/3", 0.8f, e2, r2, false);

                // Wave 3 mega: 14 enemigos variados en flujo continuo
                // Wave 3 eliminada: en lugar de oleada final, los ultimos 400m del nivel
                // tienen spawn intenso automaticamente (LevelSpawner.intenseFromMeters).

                SetupLevel3Scene(new[] { w1, w2 });
                AddEnemiesToLevel3Definition(new[] { mariachi, zombiSalt });
                AssignPanBombaSprites(panBombaPickup);
                AssignLevelMusic();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[Kikiri-9] Etapa 3 instalada OK.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[Kikiri-9] Etapa 3 fallo: " + ex);
            }
        }

        private static void EnsureDir(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        private static GameObject LoadPrefab(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(path);

        private static void AddEntry(List<WaveEntry> list, GameObject prefab, int count)
        {
            if (prefab == null) return;
            list.Add(new WaveEntry { enemyPrefab = prefab, count = count });
        }
        private static void AddReward(List<WaveReward> list, GameObject prefab, int count)
        {
            if (prefab == null) return;
            list.Add(new WaveReward { prefab = prefab, count = count });
        }

        // ---------- PREFABS ----------

        private static GameObject BuildMariachiPrefab()
        {
            string path = $"{EnemiesPrefabDir}/Enemy_Mariachi.prefab";
            var sprite = GetSprite($"{EnemiesArtDir}/mariachi/pose.png");
            var go = new GameObject("Enemy_Mariachi");
            go.tag = "Enemy";
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.8f, 1.0f);
            col.offset = new Vector2(0f, 0.3f);
            go.AddComponent<MariachiGhost>();
            var eh = go.AddComponent<EnemyHealth>();
            SetFieldInt(eh, "maxHealth", 1);
            SetFieldInt(eh, "defeatPoints", 150);
            go.AddComponent<EnemyDamageFlash>();
            var vuln = go.AddComponent<EnemyVulnerability>();
            SetFieldBool(vuln, "bread", true);
            SetFieldBool(vuln, "slide", false);
            SetFieldBool(vuln, "rafaga", false);
            SetFieldBool(vuln, "breadOnlyInAir", true);
            SetFieldFloat(vuln, "breadAirThreshold", -2.5f);
            var acon = BuildAnimator("Enemy_Mariachi",
                new AnimSpec("float_idle", GetSprites($"{EnemiesArtDir}/mariachi/float_idle.png"), 8, true),
                new AnimSpec("attack", GetSprites($"{EnemiesArtDir}/mariachi/attack.png"), 10, false),
                new AnimSpec("hit_flash", GetSprites($"{EnemiesArtDir}/mariachi/hit_flash.png"), 18, false),
                new AnimSpec("defeat", GetSprites($"{EnemiesArtDir}/mariachi/defeat.png"), 10, false));
            if (acon != null)
            {
                var a = go.AddComponent<Animator>();
                a.runtimeAnimatorController = acon;
            }
            // Mariachi: escala mayor para que sea imponente volando por arriba
            go.transform.localScale = Vector3.one * 3.0f;
            var trompeta = GetAudio($"{AudioSFXDir}/mariachi_trompeta.wav");
            SetFieldObject(go.GetComponent<MariachiGhost>(), "trumpetSfx", trompeta);
            var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return saved;
        }

        private static GameObject BuildZombiSaltarinPrefab()
        {
            string path = $"{EnemiesPrefabDir}/Enemy_ZombiSaltarin.prefab";
            var sprite = GetSprite($"{EnemiesArtDir}/zombi_saltarin/pose.png");
            var go = new GameObject("Enemy_ZombiSaltarin");
            go.tag = "Enemy";
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2.5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.75f, 0.95f);
            col.offset = new Vector2(0f, 0.3f);
            go.AddComponent<ZombiJumping>();
            var eh = go.AddComponent<EnemyHealth>();
            SetFieldInt(eh, "maxHealth", 1);
            SetFieldInt(eh, "defeatPoints", 120);
            go.AddComponent<EnemyDamageFlash>();
            var vuln = go.AddComponent<EnemyVulnerability>();
            SetFieldBool(vuln, "bread", true);
            SetFieldBool(vuln, "slide", true);
            SetFieldBool(vuln, "rafaga", true);
            SetFieldBool(vuln, "breadOnlyInAir", false);
            var acon = BuildAnimator("Enemy_ZombiSaltarin",
                new AnimSpec("crouch_idle", GetSprites($"{EnemiesArtDir}/zombi_saltarin/crouch_idle.png"), 6, true),
                new AnimSpec("jump_prepare", GetSprites($"{EnemiesArtDir}/zombi_saltarin/jump_prepare.png"), 10, false),
                new AnimSpec("jump_air", GetSprites($"{EnemiesArtDir}/zombi_saltarin/jump_air.png"), 10, true),
                new AnimSpec("land", GetSprites($"{EnemiesArtDir}/zombi_saltarin/land.png"), 12, false),
                new AnimSpec("hit_flash", GetSprites($"{EnemiesArtDir}/zombi_saltarin/hit_flash.png"), 18, false),
                new AnimSpec("defeat", GetSprites($"{EnemiesArtDir}/zombi_saltarin/defeat.png"), 10, false));
            if (acon != null)
            {
                var a = go.AddComponent<Animator>();
                a.runtimeAnimatorController = acon;
            }
            go.transform.localScale = Vector3.one * 2.0f;
            var aviso = GetAudio($"{AudioSFXDir}/zombi_aviso.wav");
            var aterrizaje = GetAudio($"{AudioSFXDir}/zombi_aterrizaje.wav");
            SetFieldObject(go.GetComponent<ZombiJumping>(), "warnSfx", aviso);
            SetFieldObject(go.GetComponent<ZombiJumping>(), "landSfx", aterrizaje);
            var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return saved;
        }

        private static GameObject BuildXoloPrefab()
        {
            string path = $"{EnemiesPrefabDir}/Enemy_Xolo.prefab";
            var sprite = GetSprite($"{EnemiesArtDir}/xolo/pose.png");
            var go = new GameObject("Enemy_Xolo");
            go.tag = "Enemy";
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CapsuleCollider2D>();
            col.direction = CapsuleDirection2D.Horizontal;
            col.size = new Vector2(0.9f, 0.4f);
            col.offset = new Vector2(0f, 0.1f);
            go.AddComponent<XoloRunner>();
            var eh = go.AddComponent<EnemyHealth>();
            SetFieldInt(eh, "maxHealth", 1);
            SetFieldInt(eh, "defeatPoints", 100);
            go.AddComponent<EnemyDamageFlash>();
            var vuln = go.AddComponent<EnemyVulnerability>();
            SetFieldBool(vuln, "bread", false);
            SetFieldBool(vuln, "slide", true);
            SetFieldBool(vuln, "rafaga", false);
            SetFieldBool(vuln, "breadOnlyInAir", false);
            var acon = BuildAnimator("Enemy_Xolo",
                new AnimSpec("run", GetSprites($"{EnemiesArtDir}/xolo/run.png"), 14, true),
                new AnimSpec("hit_flash", GetSprites($"{EnemiesArtDir}/xolo/hit_flash.png"), 18, false),
                new AnimSpec("defeat", GetSprites($"{EnemiesArtDir}/xolo/defeat.png"), 10, false));
            if (acon != null)
            {
                var a = go.AddComponent<Animator>();
                a.runtimeAnimatorController = acon;
            }
            go.transform.localScale = Vector3.one * 4.8f;
            var patitas = GetAudio($"{AudioSFXDir}/xolo_patitas.wav");
            SetFieldObject(go.GetComponent<XoloRunner>(), "pawsSfx", patitas);
            var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return saved;
        }

        private static GameObject BuildPanBombaPickup()
        {
            string path = $"{PickupsPrefabDir}/Pickup_PanBomba.prefab";
            var baseBoost = AssetDatabase.LoadAssetAtPath<GameObject>($"{PickupsPrefabDir}/Pickup_BreadBoost.prefab");
            GameObject instance;
            if (baseBoost != null)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(baseBoost);
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                instance.name = "Pickup_PanBomba";
                var boost = instance.GetComponent<Kikiri9.World.BreadBoostPickup>();
                if (boost != null) Object.DestroyImmediate(boost);
                var aura = instance.GetComponent<Kikiri9.World.BreadBoostAura>();
                if (aura != null) Object.DestroyImmediate(aura);
            }
            else
            {
                instance = new GameObject("Pickup_PanBomba");
                var sr = instance.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.78f, 0.42f, 1f, 1f);
                sr.sortingOrder = 12;
                var rb = instance.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                var col = instance.AddComponent<CircleCollider2D>();
                col.radius = 0.5f;
                col.isTrigger = true;
            }
            foreach (var sr in instance.GetComponentsInChildren<SpriteRenderer>())
                sr.color = new Color(0.78f, 0.42f, 1f, 1f);
            var pbPickup = instance.GetComponent<PanBombaPickup>();
            if (pbPickup == null) pbPickup = instance.AddComponent<PanBombaPickup>();
            var pbSfx = GetAudio($"{AudioSFXDir}/pan_dorado.wav");
            SetFieldObject(pbPickup, "pickupSfx", pbSfx);
            instance.tag = "Pickup";
            var saved = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return saved;
        }

        // ---------- WAVE ----------

        private static WaveDefinition BuildWaveDefinition(string name, float meters, string label, float interval,
            List<WaveEntry> entries, List<WaveReward> rewards, bool endsLevel, bool groupSpawn = false, bool freeMovementMode = false)
        {
            string path = $"{WavesDir}/{name}.asset";
            var def = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<WaveDefinition>();
                AssetDatabase.CreateAsset(def, path);
            }
            def.triggerMeters = meters;
            def.bannerLabel = label;
            def.spawnInterval = interval;
            def.waveTimeLimit = 60f;
            def.endsLevelOnClear = endsLevel;
            def.groupSpawn = groupSpawn;
            def.freeMovementMode = freeMovementMode;
            def.entries = entries.ToArray();
            def.rewards = rewards.ToArray();
            EditorUtility.SetDirty(def);
            return def;
        }

        // ---------- ESCENA ----------

        private static void SetupLevel3Scene(WaveDefinition[] waves)
        {
            var scene = EditorSceneManager.OpenScene(Level3Scene, OpenSceneMode.Single);
            var wmGo = GameObject.Find("WaveManager");
            if (wmGo == null)
            {
                wmGo = new GameObject("WaveManager");
                wmGo.AddComponent<AudioSource>();
                wmGo.AddComponent<WaveManager>();
            }
            var wm = wmGo.GetComponent<WaveManager>();
            SetFieldObjectArray(wm, "waves", waves);
            SetFieldObject(wm, "bannerStartSprite", GetSprite($"{UIWavesDir}/oleada_inicio.png"));
            SetFieldObject(wm, "bannerCompleteSprite", GetSprite($"{UIWavesDir}/oleada_completa.png"));
            SetFieldObject(wm, "waveStartSfx", GetAudio($"{AudioSFXDir}/oleada_inicio.wav"));
            SetFieldObject(wm, "waveCompleteSfx", GetAudio($"{AudioSFXDir}/oleada_completa.wav"));
            SetFieldFloat(wm, "spawnY", -3.9f);

            var mbs = GameObject.Find("MiniBossSpawner");
            if (mbs != null) mbs.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, Level3Scene);
        }

        private static void AddEnemiesToLevel3Definition(GameObject[] newEnemies)
        {
            var path = "Assets/_Project/ScriptableObjects/Levels/Level03_Guanajuato.asset";
            var def = AssetDatabase.LoadAssetAtPath<Kikiri9.Levels.LevelDefinition>(path);
            if (def == null) return;
            var list = new List<GameObject>(def.enemyPrefabs);

            // REMOVER xolo del pool de L3 (queda disponible como prefab para futuro, no spawn)
            var xoloPrefab = LoadPrefab($"{EnemiesPrefabDir}/Enemy_Xolo.prefab");
            if (xoloPrefab != null) list.RemoveAll(e => e == xoloPrefab);
            // Tambien por nombre, por si fuera otra referencia
            list.RemoveAll(e => e != null && e.name == "Enemy_Xolo");

            foreach (var e in newEnemies)
                if (e != null && !list.Contains(e)) list.Add(e);
            def.enemyPrefabs = list.ToArray();
            EditorUtility.SetDirty(def);
        }

        private static void AssignPanBombaSprites(GameObject pickup)
        {
            Sprite panSprite = null;
            if (pickup != null)
            {
                var sr = pickup.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) panSprite = sr.sprite;
            }
            Sprite explosion = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Draft_2_Kikiri9/Art/Effects/efectos_a_02.png");

            foreach (var scenePath in new[] { Level1Scene, Level2Scene, Level3Scene })
            {
                if (!File.Exists(scenePath)) continue;
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var launcher = player.GetComponent<Kikiri9.Combat.BreadLauncher>();
                    if (launcher != null)
                    {
                        SetFieldObject(launcher, "panBombaSprite", panSprite);
                        SetFieldObject(launcher, "panBombaExplosionSprite", explosion);
                    }
                    var sfx = player.GetComponent<Kikiri9.Player.PlayerSfx>();
                    if (sfx != null)
                    {
                        var rafagaClip = GetAudio($"{AudioSFXDir}/rafaga.wav");
                        var impactoClip = GetAudio($"{AudioSFXDir}/impacto.wav");
                        if (rafagaClip != null) SetFieldObject(sfx, "slideClip", rafagaClip);
                        if (impactoClip != null) SetFieldObject(sfx, "damageClip", impactoClip);
                    }
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, scenePath);
            }
        }

        private static void AssignLevelMusic()
        {
            var dolores = GetAudio($"{AudioMusicDir}/dolores_hidalgo_loop.wav");
            var guana = GetAudio($"{AudioMusicDir}/guanajuato_loop.wav");
            var l1 = AssetDatabase.LoadAssetAtPath<Kikiri9.Levels.LevelDefinition>("Assets/_Project/ScriptableObjects/Levels/Level01_Dolores.asset");
            var l2 = AssetDatabase.LoadAssetAtPath<Kikiri9.Levels.LevelDefinition>("Assets/_Project/ScriptableObjects/Levels/Level02_Dolores.asset");
            var l3 = AssetDatabase.LoadAssetAtPath<Kikiri9.Levels.LevelDefinition>("Assets/_Project/ScriptableObjects/Levels/Level03_Guanajuato.asset");
            if (l1 != null && dolores != null) { l1.musicClip = dolores; EditorUtility.SetDirty(l1); }
            if (l2 != null && dolores != null) { l2.musicClip = dolores; EditorUtility.SetDirty(l2); }
            if (l3 != null && guana != null) { l3.musicClip = guana; EditorUtility.SetDirty(l3); }
        }

        // ---------- SET FIELDS (typed overloads) ----------

        private static void SetFieldInt(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p != null) { p.intValue = value; so.ApplyModifiedProperties(); }
        }
        private static void SetFieldFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p != null) { p.floatValue = value; so.ApplyModifiedProperties(); }
        }
        private static void SetFieldBool(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p != null) { p.boolValue = value; so.ApplyModifiedProperties(); }
        }
        private static void SetFieldObject(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p != null) { p.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        }
        private static void SetFieldObjectArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) return;
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedProperties();
        }

        private static Sprite GetSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
        private static AudioClip GetAudio(string path) => AssetDatabase.LoadAssetAtPath<AudioClip>(path);

        private static Sprite[] GetSprites(string path)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            var list = new List<Sprite>();
            foreach (var a in all) if (a is Sprite s) list.Add(s);
            list.Sort((x, y) => string.Compare(x.name, y.name, System.StringComparison.Ordinal));
            return list.ToArray();
        }

        // ---------- ANIMATOR ----------

        private struct AnimSpec
        {
            public string name;
            public Sprite[] frames;
            public int fps;
            public bool loop;
            public AnimSpec(string n, Sprite[] f, int fps, bool loop) { name = n; frames = f; this.fps = fps; this.loop = loop; }
        }

        private static AnimatorController BuildAnimator(string name, params AnimSpec[] clips)
        {
            string path = $"{AnimDir}/{name}.controller";
            var ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (ac == null) ac = AnimatorController.CreateAnimatorControllerAtPath(path);

            bool hasState = false, hasDefeatTrigger = false;
            foreach (var par in ac.parameters)
            {
                if (par.name == "State") hasState = true;
                if (par.name == "Defeat") hasDefeatTrigger = true;
            }
            if (!hasState) ac.AddParameter("State", AnimatorControllerParameterType.Int);
            if (!hasDefeatTrigger) ac.AddParameter("Defeat", AnimatorControllerParameterType.Trigger);

            var sm = ac.layers[0].stateMachine;
            var existing = new List<ChildAnimatorState>(sm.states);
            foreach (var st in existing) sm.RemoveState(st.state);

            AnimatorState defeatState = null;
            bool first = true;
            foreach (var spec in clips)
            {
                if (spec.frames == null || spec.frames.Length == 0) continue;
                var clip = CreateAnimationClip(spec.name, spec.frames, spec.fps, spec.loop);
                string clipPath = $"{AnimDir}/{name}_{spec.name}.anim";
                AssetDatabase.CreateAsset(clip, clipPath);
                var state = sm.AddState(spec.name);
                state.motion = clip;
                if (first) { sm.defaultState = state; first = false; }
                if (spec.name == "defeat" || spec.name == "Defeat") defeatState = state;
            }

            // Transicion Any State -> defeat con trigger Defeat
            if (defeatState != null)
            {
                var anyTrans = sm.AddAnyStateTransition(defeatState);
                anyTrans.AddCondition(AnimatorConditionMode.If, 0f, "Defeat");
                anyTrans.duration = 0f;
                anyTrans.hasExitTime = false;
                anyTrans.canTransitionToSelf = false;
            }

            EditorUtility.SetDirty(ac);
            return ac;
        }

        private static AnimationClip CreateAnimationClip(string name, Sprite[] frames, int fps, bool loop)
        {
            var clip = new AnimationClip { name = name, frameRate = fps };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            var binding = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
            var keys = new ObjectReferenceKeyframe[frames.Length];
            for (int i = 0; i < frames.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i / (float)fps, value = frames[i] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            return clip;
        }
    }
}
