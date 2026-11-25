// Coronela: jefe final del juego (#14). Flota en el arena (sin gravedad, sin FSM de
// salto como la Momia #12), dispara un proyectil espectral cada ~2.5s hacia el jugador
// y deja aparecer un pan alcanzable cada 4s (el spawner normal esta apagado durante la
// pelea, igual que con la Momia). Al bajarle sus 12 HP, hace su animacion de derrota y
// el juego pasa directo a la pantalla de victoria: a diferencia de la Momia, aqui NO se
// reanuda la carrera, porque este es el final del juego.

using System.Collections;
using Kikiri9.Session;
using Kikiri9.World;
using UnityEngine;

namespace Kikiri9.Enemies
{
    [RequireComponent(typeof(EnemyHealth), typeof(Collider2D))]
    public class CoronelaBoss : MonoBehaviour
    {
        [Header("Flotacion")]
        [SerializeField] private float floatAmplitude = 0.4f;
        [SerializeField] private float floatSpeed = 1.2f;

        [Header("Ataque")]
        [SerializeField] private float attackIntervalMin = 2.2f;
        [SerializeField] private float attackIntervalMax = 2.8f;
        [SerializeField] private float telegraphDuration = 0.5f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 7f;
        [SerializeField] private Vector3 muzzleOffset = new Vector3(-1f, 0.3f, 0f);

        [Header("Suministro de pan")]
        [SerializeField] private GameObject breadDropPrefab;
        [SerializeField] private float breadInterval = 3f;
        [SerializeField] private float breadSpawnY = -1.5f;
        [SerializeField] private float arenaHalfWidth = 5f;

        [Header("Animacion (sprites)")]
        [SerializeField] private Sprite[] caminarFrames;
        [SerializeField] private Sprite[] ataqueFrames;
        [SerializeField] private Sprite[] derrotaFrames;
        [SerializeField] private float frameInterval = 0.1f;

        [Header("Final")]
        [SerializeField] private float endDelay = 1.2f;

        private EnemyHealth health;
        private SpriteRenderer sr;
        private BossArenaManager arena;
        private Transform player;

        private float baseY;
        private float arenaCenterX;
        private float nextAttackTime;
        private float nextBreadTime;
        private bool defeated;
        private bool attacking;
        private Coroutine idleAnimRoutine;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            sr = GetComponentInChildren<SpriteRenderer>();

            foreach (var col in GetComponents<Collider2D>())
                col.isTrigger = false;
        }

        private void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;

            baseY = transform.position.y;
            arenaCenterX = transform.position.x;

            arena = FindFirstObjectByType<BossArenaManager>();
            if (arena == null)
            {
                var go = new GameObject("BossArenaManager");
                arena = go.AddComponent<BossArenaManager>();
            }
            arena.SetActive(true);
            arena.SetBossHealth(health);

            var cl = FindFirstObjectByType<ArenaCameraLock>();
            if (cl == null)
            {
                var go = new GameObject("ArenaCameraLock");
                cl = go.AddComponent<ArenaCameraLock>();
            }
            cl.SetLockCenter(arenaCenterX);

            var cp = FindFirstObjectByType<ClampPlayerToArena>();
            if (cp == null)
            {
                var go = new GameObject("ClampPlayerToArena");
                cp = go.AddComponent<ClampPlayerToArena>();
            }
            cp.SetCenter(arenaCenterX);

            var spawner = FindFirstObjectByType<Kikiri9.Levels.LevelSpawner>();
            if (spawner != null) spawner.enabled = false;

            nextAttackTime = Time.time + Random.Range(attackIntervalMin, attackIntervalMax) * 0.5f;
            nextBreadTime = Time.time + 1.5f;

            idleAnimRoutine = StartCoroutine(PlayLoop(caminarFrames));
        }

        private void Update()
        {
            if (defeated) return;
            if (IsDefeatedViaReflection())
            {
                if (idleAnimRoutine != null) StopCoroutine(idleAnimRoutine);
                defeated = true;
                StartCoroutine(DefeatSequence());
                return;
            }

            // Flota con un vaiven suave sobre su posicion base (no flota mientras ataca,
            // para que el telegraph/disparo se vea estable).
            if (!attacking)
            {
                var pos = transform.position;
                pos.y = baseY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
                transform.position = pos;
            }

            if (!attacking && Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackRoutine());
            }

            if (Time.time >= nextBreadTime)
            {
                SpawnReachableBread();
                nextBreadTime = Time.time + breadInterval;
            }
        }

        private IEnumerator AttackRoutine()
        {
            attacking = true;
            nextAttackTime = Time.time + Random.Range(attackIntervalMin, attackIntervalMax);

            if (idleAnimRoutine != null) StopCoroutine(idleAnimRoutine);
            yield return PlayOnce(ataqueFrames, telegraphDuration);

            if (!defeated) FireProjectile();

            if (!defeated) idleAnimRoutine = StartCoroutine(PlayLoop(caminarFrames));
            attacking = false;
        }

        private void FireProjectile()
        {
            if (projectilePrefab == null || player == null) return;

            var origin = transform.position + muzzleOffset;
            var go = Instantiate(projectilePrefab, origin, Quaternion.identity);
            var proj = go.GetComponent<CoronelaSpectralProjectile>();
            if (proj == null) return;

            Vector2 dir = ((Vector2)player.position - (Vector2)origin).normalized;
            if (dir == Vector2.zero) dir = Vector2.left;
            proj.Launch(dir * projectileSpeed);
        }

        private void SpawnReachableBread()
        {
            if (breadDropPrefab == null) return;
            float offset = Random.Range(-arenaHalfWidth * 0.8f, arenaHalfWidth * 0.8f);
            var pos = new Vector3(arenaCenterX + offset, breadSpawnY, 0f);
            Instantiate(breadDropPrefab, pos, Quaternion.identity);
        }

        private IEnumerator PlayLoop(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0 || sr == null) yield break;
            int i = 0;
            while (true)
            {
                sr.sprite = frames[i % frames.Length];
                i++;
                yield return new WaitForSeconds(frameInterval);
            }
        }

        private IEnumerator PlayOnce(Sprite[] frames, float minDuration)
        {
            if (frames == null || frames.Length == 0 || sr == null)
            {
                yield return new WaitForSeconds(minDuration);
                yield break;
            }
            float elapsed = 0f;
            int i = 0;
            while (elapsed < minDuration || i < frames.Length)
            {
                sr.sprite = frames[Mathf.Min(i, frames.Length - 1)];
                i++;
                yield return new WaitForSeconds(frameInterval);
                elapsed += frameInterval;
            }
        }

        private IEnumerator DefeatSequence()
        {
            yield return PlayOnce(derrotaFrames, derrotaFrames != null ? derrotaFrames.Length * frameInterval : 0.6f);

            if (arena != null) arena.SetActive(false);

            yield return new WaitForSeconds(endDelay);

            if (GameStateManager.Instance != null)
                GameStateManager.Instance.SetState(GameState.LevelComplete);
        }

        private bool IsDefeatedViaReflection()
        {
            var f = typeof(EnemyHealth).GetField("isDefeated",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return f != null && (bool)f.GetValue(health);
        }
    }
}
