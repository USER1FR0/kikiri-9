// Spawns a mixed pool (enemies + obstacles) ahead of the camera at random intervals, using
// the active LevelController's LevelDefinition. Pausa durante BossArenaManager.IsActive.
// Respeta safeStartMeters (primeros metros solo pickups) para dar un onramp calido al nivel.

using System.Collections.Generic;
using Kikiri9.Session;
using UnityEngine;

namespace Kikiri9.Levels
{
    public class LevelSpawner : MonoBehaviour
    {
        [SerializeField] private LevelController level;
        [SerializeField] private Transform spawnReference;
        [SerializeField] private float spawnAheadDistance = 15f;
        [SerializeField] private float groundY = -2.75f;
        [SerializeField] private float pickupExtraY = 0.5f; // eleva pickups respecto a groundY
        [SerializeField] private float intenseFromMeters = 1000f;
        [SerializeField] private float intenseSpawnMultiplier = 0.55f; // intervalo *0.55 = ~2x spawn
        [SerializeField] private float minSeparation = 7f;

        private readonly List<GameObject> active = new List<GameObject>();
        private float nextSpawnTime;

        private void Start()
        {
            if (level == null) level = FindFirstObjectByType<LevelController>();
            if (spawnReference == null && Camera.main != null) spawnReference = Camera.main.transform;
            ScheduleNext();
        }

        private float ActiveMinSeparation
        {
            get
            {
                var def = level != null ? level.Definition : null;
                return (def != null && def.minSpawnSeparation > 0f) ? def.minSpawnSeparation : minSeparation;
            }
        }

        private void Update()
        {
            if (BossArenaManager.IsActive) return;
            if (level == null || level.Definition == null || spawnReference == null) return;
            if (Time.time < nextSpawnTime) return;

            active.RemoveAll(e => e == null);
            if (active.Count < level.Definition.maxActiveEntities)
            {
                var pos = new Vector3(spawnReference.position.x + spawnAheadDistance, groundY, 0f);

                bool clear = true;
                foreach (var e in active)
                {
                    if (e == null) continue;
                    if (Mathf.Abs(e.transform.position.x - pos.x) < ActiveMinSeparation) { clear = false; break; }
                }

                if (clear)
                {
                    var prefab = PickRandomPrefab(level.Definition);
                    if (prefab != null)
                    {
                        var p = pos;
                        if (lastCategory == SpawnCategory.Pickup)
                        {
                            p.y += pickupExtraY;
                            if (level.Definition.pickupHighChance > 0f && Random.value < level.Definition.pickupHighChance)
                                p.y += level.Definition.pickupHighYBonus;
                        }
                        active.Add(Instantiate(prefab, p, Quaternion.identity));
                    }
                }
            }
            ScheduleNext();
        }

        private enum SpawnCategory { Enemy, Obstacle, Pickup }
        private SpawnCategory lastCategory = SpawnCategory.Enemy;
        private int consecutive = 0;
        private const int MaxConsecutive = 2;
        private const int MaxConsecutiveObstacles = 1;
        private float lastObstacleMeters = -999f;
        [SerializeField] private float enemyGapAfterObstacle = 10f;

        private static SpawnCategory PickWeighted(List<SpawnCategory> avail, LevelDefinition def)
        {
            float sum = 0f;
            foreach (var c in avail) sum += CategoryWeight(def, c);
            if (sum <= 0f) return avail[Random.Range(0, avail.Count)];
            float roll = Random.value * sum;
            float cum = 0f;
            foreach (var c in avail)
            {
                cum += CategoryWeight(def, c);
                if (roll <= cum) return c;
            }
            return avail[avail.Count - 1];
        }

        private static float CategoryWeight(LevelDefinition def, SpawnCategory c)
        {
            switch (c)
            {
                case SpawnCategory.Enemy: return Mathf.Max(0f, def.enemyWeight);
                case SpawnCategory.Obstacle: return Mathf.Max(0f, def.obstacleWeight);
                default: return Mathf.Max(0f, def.pickupWeight);
            }
        }

        private static GameObject FindBreadPickup(LevelDefinition def)
        {
            foreach (var p in def.pickupPrefabs)
                if (p != null && p.name.Contains("Pickup_Bread") && !p.name.Contains("Boost"))
                    return p;
            return def.pickupPrefabs.Length > 0 ? def.pickupPrefabs[0] : null;
        }

        private int CountActiveEnemies()
        {
            int n = 0;
            foreach (var e in active)
            {
                if (e == null) continue;
                if (e.CompareTag("Enemy") && e.GetComponent<Kikiri9.Enemies.EnemyHealth>() != null) n++;
            }
            return n;
        }

        private bool InSafeZone(LevelDefinition def)
        {
            if (def.safeStartMeters <= 0f || Kikiri9.World.WorldScroller.Instance == null) return false;
            return Kikiri9.World.WorldScroller.Instance.MetersAdvanced < def.safeStartMeters;
        }

        private GameObject PickRandomPrefab(LevelDefinition def)
        {
            int eC = def.enemyPrefabs != null ? def.enemyPrefabs.Length : 0;
            int oC = def.obstaclePrefabs != null ? def.obstaclePrefabs.Length : 0;
            int pC = def.pickupPrefabs != null ? def.pickupPrefabs.Length : 0;
            if (eC + oC + pC == 0) return null;

            bool inSafeZone = InSafeZone(def);

            if (def.breadLowRatio > 0f && pC > 0 && !inSafeZone)
            {
                var lowAmmo = Object.FindFirstObjectByType<Kikiri9.Player.PlayerAmmo>();
                int lowBread = lowAmmo != null ? lowAmmo.CurrentBread : 999;
                int lowEnemies = CountActiveEnemies();
                if (lowEnemies > 0 && lowBread < Mathf.CeilToInt(lowEnemies * def.breadLowRatio))
                {
                    var breadPickup = FindBreadPickup(def);
                    if (breadPickup != null)
                    {
                        lastCategory = SpawnCategory.Pickup;
                        consecutive = 1;
                        return breadPickup;
                    }
                }
            }

            float wE = def.enemyWeight, wO = def.obstacleWeight, wP = def.pickupWeight;
            bool useWeighted = (wE + wO + wP) > 0f;

            var avail = new List<SpawnCategory>(3);
            if (eC > 0) avail.Add(SpawnCategory.Enemy);
            if (oC > 0) avail.Add(SpawnCategory.Obstacle);
            if (pC > 0) avail.Add(SpawnCategory.Pickup);

            // Zona segura inicial: solo pickups
            if (inSafeZone)
            {
                avail.Remove(SpawnCategory.Enemy);
                avail.Remove(SpawnCategory.Obstacle);
                if (avail.Count == 0) return null;
            }
            else
            {
                var ammo = Object.FindFirstObjectByType<Kikiri9.Player.PlayerAmmo>();
                int bread = ammo != null ? ammo.CurrentBread : int.MaxValue;
                int activeEnemies = CountActiveEnemies();
                if (bread <= activeEnemies && avail.Count > 1) avail.Remove(SpawnCategory.Enemy);

                // Tras un obstaculo, esperar enemyGapAfterObstacle metros antes de enemigo
                float metersNow = Kikiri9.World.WorldScroller.Instance != null
                    ? Kikiri9.World.WorldScroller.Instance.MetersAdvanced : 0f;
                if (lastCategory == SpawnCategory.Obstacle
                    && metersNow - lastObstacleMeters < enemyGapAfterObstacle
                    && avail.Count > 1)
                {
                    avail.Remove(SpawnCategory.Enemy);
                }

                if (consecutive >= MaxConsecutive && avail.Count > 1)
                    avail.Remove(lastCategory);
                // Evita que dos obstaculos seguidos se amontonen
                if (lastCategory == SpawnCategory.Obstacle && consecutive >= MaxConsecutiveObstacles && avail.Count > 1)
                    avail.Remove(SpawnCategory.Obstacle);
            }

            if (avail.Count == 0) return null;

            SpawnCategory chosen;
            if (useWeighted)
                chosen = PickWeighted(avail, def);
            else
                chosen = avail[Random.Range(0, avail.Count)];

            if (chosen == lastCategory) consecutive++;
            else { lastCategory = chosen; consecutive = 1; }

            if (chosen == SpawnCategory.Obstacle)
            {
                lastObstacleMeters = Kikiri9.World.WorldScroller.Instance != null
                    ? Kikiri9.World.WorldScroller.Instance.MetersAdvanced : 0f;
            }

            switch (chosen)
            {
                case SpawnCategory.Enemy:    return def.enemyPrefabs[Random.Range(0, eC)];
                case SpawnCategory.Obstacle: return def.obstaclePrefabs[Random.Range(0, oC)];
                default:                     return def.pickupPrefabs[Random.Range(0, pC)];
            }
        }

        private void ScheduleNext()
        {
            var def = level != null ? level.Definition : null;
            var range = def != null ? def.spawnIntervalRange : new Vector2(2f, 3.5f);
            float gap = Random.Range(range.x, range.y);
            // Si estamos en la recta final (ultimos metros), spawn mas intenso
            if (Kikiri9.World.WorldScroller.Instance != null
                && Kikiri9.World.WorldScroller.Instance.MetersAdvanced >= intenseFromMeters)
            {
                gap *= intenseSpawnMultiplier;
            }
            nextSpawnTime = Time.time + gap;
        }
    }
}
