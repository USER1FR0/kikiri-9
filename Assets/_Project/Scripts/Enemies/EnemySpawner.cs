// Spawns enemies ahead of the camera at random intervals, keeping at most maxActive alive.

using System.Collections.Generic;
using UnityEngine;

namespace Kikiri9.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform spawnReference;
        [SerializeField] private float spawnAheadDistance = 15f;
        [SerializeField] private float groundY = -2.75f;
        [SerializeField] private Vector2 intervalRange = new Vector2(2f, 3.5f);
        [SerializeField] private int maxActive = 5;

        private readonly List<GameObject> activeEnemies = new List<GameObject>();
        private float nextSpawnTime;

        private void Start()
        {
            if (spawnReference == null && Camera.main != null) spawnReference = Camera.main.transform;
            ScheduleNext();
        }

        private void Update()
        {
            if (enemyPrefab == null || spawnReference == null) return;
            if (Time.time < nextSpawnTime) return;

            // Destroyed enemies turn into null references; drop them before counting.
            activeEnemies.RemoveAll(e => e == null);
            if (activeEnemies.Count < maxActive)
            {
                var position = new Vector3(spawnReference.position.x + spawnAheadDistance, groundY, 0f);
                activeEnemies.Add(Instantiate(enemyPrefab, position, Quaternion.identity));
            }
            ScheduleNext();
        }

        private void ScheduleNext()
        {
            nextSpawnTime = Time.time + Random.Range(intervalRange.x, intervalRange.y);
        }
    }
}
