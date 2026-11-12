// Dispara oleadas segun MetersAdvanced. Durante una oleada:
//   - Pausa LevelSpawner
//   - Muestra banner inicio con TMP sobre sprite oleada_inicio
//   - Spawnea enemigos de WaveDefinition en intervalos
//   - Espera a que todos los Enemy con tag queden a 0
//   - Dropea rewards + muestra banner "OLEADA COMPLETA"
//   - Si endsLevelOnClear: dispara LevelController.Finish() / LevelOutro

using System.Collections;
using System.Collections.Generic;
using Kikiri9.Session;
using Kikiri9.World;
using UnityEngine;

namespace Kikiri9.Levels.Waves
{
    public class WaveManager : MonoBehaviour
    {
        public static bool WaveActive { get; private set; }

        [SerializeField] private LevelController level;
        [SerializeField] private WaveDefinition[] waves;
        [SerializeField] private float spawnAheadDistance = 10f;
        [SerializeField] private float spawnY = -3.9f;
        [SerializeField] private float dropLandingY = -3.5f;
        [SerializeField] private float dropSpawnHeight = 3f;
        [SerializeField] private AudioClip waveStartSfx;
        [SerializeField] private AudioClip waveCompleteSfx;
        [SerializeField] private Sprite bannerStartSprite;
        [SerializeField] private Sprite bannerCompleteSprite;

        private int nextIndex;
        private bool inWave;
        private readonly List<GameObject> currentEnemies = new List<GameObject>();
        private AudioSource audioSrc;

        private void Awake()
        {
            audioSrc = GetComponent<AudioSource>();
            if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
        }

        private void Start()
        {
            if (level == null) level = FindFirstObjectByType<LevelController>();
            var ph = Object.FindFirstObjectByType<Kikiri9.Session.PlayerHealth>();
            if (ph != null) ph.OnDeath += HandlePlayerDeath;
        }

        private void HandlePlayerDeath()
        {
            StopAllCoroutines();
            WaveActive = false;
            inWave = false;
            if (WaveBanner.Instance != null) WaveBanner.Instance.ForceHide();
        }

        private void OnDestroy()
        {
            var ph = Object.FindFirstObjectByType<Kikiri9.Session.PlayerHealth>();
            if (ph != null) ph.OnDeath -= HandlePlayerDeath;
            WaveActive = false;
        }

        private void Update()
        {
            if (inWave || waves == null || nextIndex >= waves.Length) return;
            if (WorldScroller.Instance == null) return;
            if (WorldScroller.Instance.MetersAdvanced >= waves[nextIndex].triggerMeters)
            {
                StartCoroutine(RunWave(waves[nextIndex]));
                nextIndex++;
            }
        }

        private IEnumerator RunWave(WaveDefinition wave)
        {
            inWave = true;
            WaveActive = wave.freeMovementMode; // Solo frena mundo si la oleada lo pide
            var spawner = FindFirstObjectByType<LevelSpawner>();
            if (spawner != null) spawner.enabled = false;

            if (wave.freeMovementMode)
            {
                // LIMPIAR arena del jefe para que no bloqueen al pollo
                if (BossArenaManager.IsActive)
                {
                    var bam = FindFirstObjectByType<BossArenaManager>();
                    if (bam != null) bam.SetActive(false);
                }
                var clamp = FindFirstObjectByType<Kikiri9.World.ClampPlayerToArena>();
                if (clamp != null) clamp.enabled = false;
                var camLock = FindFirstObjectByType<Kikiri9.World.ArenaCameraLock>();
                if (camLock != null) camLock.enabled = false;
            }

            ShowBanner(wave.bannerLabel, bannerStartSprite, 1.8f);
            if (waveStartSfx != null) audioSrc.PlayOneShot(waveStartSfx);

            yield return new WaitForSeconds(1.2f);

            // Indicador persistente de ARENA (estilo Metal Slug) para freeMovementMode
            if (wave.freeMovementMode)
            {
                ShowArenaIndicator(true);
            }

            // Spawnea el pool de enemigos distribuido en el tiempo
            var pool = BuildPool(wave);
            foreach (var prefab in pool)
            {
                var cam = Camera.main;
                float camX = cam != null ? cam.transform.position.x : 0f;
                // Separacion grande: 0-12u en X + vertical offset chico para variar
                float offsetX = Random.Range(0f, 12f);
                float offsetY = Random.Range(-0.3f, 0.3f);
                var pos = new Vector3(camX + spawnAheadDistance + offsetX, spawnY + offsetY, 0f);
                var e = Instantiate(prefab, pos, Quaternion.identity);
                currentEnemies.Add(e);
                // Spawn interval real: no menor a 1.2s para dar separacion temporal
                float gap = Mathf.Max(wave.spawnInterval, 1.2f);
                yield return new WaitForSeconds(gap);
            }

            // Espera hasta que todos esten muertos o se acabe el tiempo
            float startTime = Time.time;
            while (AnyAlive() && Time.time - startTime < wave.waveTimeLimit)
                yield return null;
            // Si quedaron vivos al expirar, forzar su derrota
            foreach (var e in currentEnemies)
            {
                if (e == null) continue;
                var h = e.GetComponent<Kikiri9.Enemies.EnemyHealth>();
                if (h != null) h.TakeDamage(999);
            }
            currentEnemies.Clear();

            // Banner completa + rewards
            if (wave.freeMovementMode) ShowArenaIndicator(false);
            ShowBanner("OLEADA COMPLETA", bannerCompleteSprite, 1.5f);
            if (waveCompleteSfx != null) audioSrc.PlayOneShot(waveCompleteSfx);

            DropRewards(wave);

            yield return new WaitForSeconds(1.5f);

            if (wave.endsLevelOnClear)
            {
                EndLevel();
            }
            else
            {
                if (spawner != null) spawner.enabled = true;
            }
            inWave = false;
            WaveActive = false;
        }

        private List<GameObject> BuildPool(WaveDefinition wave)
        {
            var pool = new List<GameObject>();
            if (wave.entries == null) return pool;

            if (wave.groupSpawn)
            {
                // Grupos: todos los de un tipo juntos, luego los del siguiente
                foreach (var e in wave.entries)
                {
                    if (e.enemyPrefab == null) continue;
                    for (int i = 0; i < e.count; i++) pool.Add(e.enemyPrefab);
                }
                return pool;
            }

            // Default: round-robin intercalado (1 de cada tipo rotando)
            var buckets = new List<List<GameObject>>();
            foreach (var e in wave.entries)
            {
                if (e.enemyPrefab == null) continue;
                var bucket = new List<GameObject>();
                for (int i = 0; i < e.count; i++) bucket.Add(e.enemyPrefab);
                buckets.Add(bucket);
            }
            while (true)
            {
                bool any = false;
                foreach (var b in buckets)
                {
                    if (b.Count > 0)
                    {
                        pool.Add(b[0]);
                        b.RemoveAt(0);
                        any = true;
                    }
                }
                if (!any) break;
            }
            return pool;
        }

        private bool AnyAlive()
        {
            currentEnemies.RemoveAll(e => e == null);
            return currentEnemies.Count > 0;
        }

        private void DropRewards(WaveDefinition wave)
        {
            if (wave.rewards == null) return;
            var cam = Camera.main;
            float camX = cam != null ? cam.transform.position.x : 0f;

            int total = 0;
            foreach (var r in wave.rewards) if (r.prefab != null) total += r.count;
            if (total == 0) return;

            // Spread estandar: 1u antes de camara hasta 10u despues, jitter chico
            float startX = camX - 1f;
            float endX = camX + 10f;
            float stepX = total > 1 ? (endX - startX) / (total - 1) : 0f;

            int idx = 0;
            foreach (var r in wave.rewards)
            {
                if (r.prefab == null) continue;
                for (int i = 0; i < r.count; i++)
                {
                    float x = startX + stepX * idx;
                    float jitter = Random.Range(-0.25f, 0.25f);
                    float h = dropSpawnHeight + (idx % 2 == 0 ? 0f : 0.6f);
                    var pos = new Vector3(x + jitter, h, 0f);
                    var drop = Instantiate(r.prefab, pos, Quaternion.identity);
                    AddGravityToDrop(drop);
                    idx++;
                }
            }
        }

        private void AddGravityToDrop(GameObject drop)
        {
            var rb = drop.GetComponent<Rigidbody2D>();
            if (rb == null) rb = drop.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = new Vector2(0f, -1f);
            var floater = drop.GetComponent<Kikiri9.World.PickupFloater>();
            if (floater != null) floater.enabled = false;
            // Agregar GroundDrop para que aterrice y se quede recogible
            var gd = drop.GetComponent<Kikiri9.World.GroundDrop>();
            if (gd == null) gd = drop.AddComponent<Kikiri9.World.GroundDrop>();
            gd.Configure(dropLandingY);
        }

        private void ShowBanner(string label, Sprite sprite, float visibleTime)
        {
            if (WaveBanner.Instance == null)
            {
                var go = new GameObject("WaveBanner");
                go.AddComponent<WaveBanner>();
            }
            WaveBanner.Instance?.Show(label, sprite, visibleTime);
        }

        private void ShowArenaIndicator(bool show)
        {
            if (WaveBanner.Instance == null) return;
            if (show) WaveBanner.Instance.ShowPersistent("ARENA - MUEVETE CON A/D", bannerStartSprite);
            else WaveBanner.Instance.ForceHide();
        }

        private void EndLevel()
        {
            // Al acabar wave 3: reanudar el mundo. Pollo retoma auto-run y avanza
            // naturalmente hacia la meta. El LevelController dispara LevelComplete
            // cuando se alcance targetDistance.
            StartCoroutine(FinishRoutine());
        }

        private IEnumerator FinishRoutine()
        {
            // Esperar a que el jugador recoja los drops
            yield return new WaitForSeconds(3f);
            // Reanudar spawn de pickups suaves hacia la meta (no enemigos pesados)
            var spawner = FindFirstObjectByType<LevelSpawner>();
            if (spawner != null) spawner.enabled = true;
            // Flag Wave queda apagado -> WorldScroller y PlayerController reanudan
            WaveActive = false;
        }
    }
}
