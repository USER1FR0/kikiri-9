// Spawnea el mini-boss una vez cuando MetersAdvanced >= triggerMeters.
// Garantiza que haya un BossArenaManager en la escena antes del spawn.

using Kikiri9.Session;
using Kikiri9.World;
using UnityEngine;

namespace Kikiri9.Levels
{
    public class MiniBossSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private float triggerMeters = 1000f;
        [SerializeField] private float spawnAheadDistance = 10f;
        [SerializeField] private float spawnY = -1.5f;

        private bool spawned;

        private void Awake()
        {
            if (FindFirstObjectByType<BossArenaManager>() == null)
            {
                var go = new GameObject("BossArenaManager");
                go.AddComponent<BossArenaManager>();
            }
        }

        private void Update()
        {
            if (spawned || bossPrefab == null || WorldScroller.Instance == null) return;
            if (WorldScroller.Instance.MetersAdvanced < triggerMeters) return;
            spawned = true;
            var cam = Camera.main;
            if (cam == null) return;
            var pos = new Vector3(cam.transform.position.x + spawnAheadDistance, spawnY, 0f);
            Instantiate(bossPrefab, pos, Quaternion.identity);
        }
    }
}
