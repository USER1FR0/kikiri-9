// Spawns a mixed pool (enemies + obstacles) ahead of the camera at random intervals, using
// the active LevelController's LevelDefinition. Independent from Manuel's EnemySpawner so
// the Sandbox scene keeps its original single-prefab spawner untouched.

using System.Collections.Generic;
using UnityEngine;

namespace Kikiri9.Levels
{
    public class LevelSpawner : MonoBehaviour
    {
        [SerializeField] private LevelController level;
        [SerializeField] private Transform spawnReference;
        [SerializeField] private float spawnAheadDistance = 15f;
        [SerializeField] private float groundY = -2.75f;

        private readonly List<GameObject> active = new List<GameObject>();
        private float nextSpawnTime;

        private void Start()
        {
            if (level == null) level = FindFirstObjectByType<LevelController>();
            if (spawnReference == null && Camera.main != null) spawnReference = Camera.main.transform;
            ScheduleNext();
        }

        [SerializeField] private float minSeparation = 7f;

        private void Update()
        {
            if (level == null || level.Definition == null || spawnReference == null) return;
            if (Time.time < nextSpawnTime) return;

            active.RemoveAll(e => e == null);
            if (active.Count < level.Definition.maxActiveEntities)
            {
                var pos = new Vector3(spawnReference.position.x + spawnAheadDistance, groundY, 0f);

                // Evitar sobreponer con otro ya spawneado cerca
                bool clear = true;
                foreach (var e in active)
                {
                    if (e == null) continue;
                    if (Mathf.Abs(e.transform.position.x - pos.x) < minSeparation) { clear = false; break; }
                }

                if (clear)
                {
                    var prefab = PickRandomPrefab(level.Definition);
                    if (prefab != null) active.Add(Instantiate(prefab, pos, Quaternion.identity));
                }
            }
            ScheduleNext();
        }

        private enum SpawnCategory { Enemy, Obstacle, Pickup }
        private SpawnCategory lastCategory = SpawnCategory.Enemy;
        private int consecutive = 0;
        private const int MaxConsecutive = 2;

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

        private GameObject PickRandomPrefab(LevelDefinition def)
        {
            int eC = def.enemyPrefabs != null ? def.enemyPrefabs.Length : 0;
            int oC = def.obstaclePrefabs != null ? def.obstaclePrefabs.Length : 0;
            int pC = def.pickupPrefabs != null ? def.pickupPrefabs.Length : 0;
            if (eC + oC + pC == 0) return null;

            var avail = new System.Collections.Generic.List<SpawnCategory>(3);
            if (eC > 0) avail.Add(SpawnCategory.Enemy);
            if (oC > 0) avail.Add(SpawnCategory.Obstacle);
            if (pC > 0) avail.Add(SpawnCategory.Pickup);

            // Regla: no spawnear enemigos si el pollo no tiene suficiente pan para matarlos
            // (CurrentBread debe ser > enemigos activos para permitir el spawn).
            var ammo = Object.FindFirstObjectByType<Kikiri9.Player.PlayerAmmo>();
            int bread = ammo != null ? ammo.CurrentBread : int.MaxValue;
            int activeEnemies = CountActiveEnemies();
            if (bread <= activeEnemies && avail.Count > 1) avail.Remove(SpawnCategory.Enemy);

            if (consecutive >= MaxConsecutive && avail.Count > 1)
                avail.Remove(lastCategory);

            if (avail.Count == 0) return null; // no hay nada seguro de spawnear este tick

            var chosen = avail[Random.Range(0, avail.Count)];
            if (chosen == lastCategory) consecutive++;
            else { lastCategory = chosen; consecutive = 1; }

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
            nextSpawnTime = Time.time + Random.Range(range.x, range.y);
        }
    }
}
