// Mini-boss momia v4: FSM con estados Idle / Alert / Lunge / Recover.
//  - Tiempos de salto y ataque aleatorios dentro de rangos.
//  - Durante el telegraph (Alert) no inflige contact damage: el jugador puede ejecutar
//    su ráfaga sin castigo.
//  - Ground check real con Physics2D.OverlapCircle.
//  - HP 5 y drops ajustados al rango del arena.
//  - Durante el Recover queda vulnerable (contact damage apagado, puede pegarle el pan).

using System.Collections;
using Kikiri9.Player;
using Kikiri9.Session;
using Kikiri9.World;
using UnityEngine;

namespace Kikiri9.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyHealth))]
    public class MiniBossMomia : MonoBehaviour
    {
        private enum BossState { Idle, Alert, Lunge, Recover }

        [Header("Movimiento")]
        [SerializeField] private float horizontalWanderSpeed = 3f;
        [SerializeField] private float wanderRange = 3f;
        [SerializeField] private Vector2 jumpIntervalRange = new Vector2(1.8f, 2.8f);
        [SerializeField] private float jumpVelocity = 11f;

        [Header("Ataque")]
        [SerializeField] private Vector2 attackCooldownRange = new Vector2(2.2f, 3.5f);
        [SerializeField] private float telegraphDuration = 0.55f;
        [SerializeField] private float lungeSpeed = 9f;
        [SerializeField] private float lungeDuration = 0.4f;
        [SerializeField] private float recoverDuration = 0.9f;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private float contactCooldown = 1f;

        [Header("Ground check")]
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -1.1f);
        [SerializeField] private LayerMask groundLayer = ~0;

        [Header("Drops")]
        [SerializeField] private GameObject breadDropPrefab;
        [SerializeField] private GameObject bottleDropPrefab;
        [SerializeField] private GameObject breadBoostPrefab;
        [SerializeField] private float dropInterval = 2.5f;
        [SerializeField] private int defeatBreadBurst = 6;
        [SerializeField] private float breadSpawnY = 3.5f;
        [SerializeField] private float arenaHalfWidth = 6f;

        [Header("Musica suspenso")]
        [SerializeField] private float suspenseMusicPitch = 0.75f;
        [SerializeField] private float suspenseMusicVolume = 0.7f;

        private Rigidbody2D rb;
        private Transform player;
        private EnemyHealth health;
        private BossArenaManager arena;
        private SpriteRenderer sr;

        private BossState state = BossState.Idle;
        private float stateEndTime;
        private float nextJumpTime;
        private float nextDropTime;
        private float nextAttackTime;
        private float nextContactTime;
        private float arenaCenterX;
        private int wanderDir = 1;
        private bool defeated;
        private bool invulnerabilityToPlayerActive;

        private AudioSource musicSource;
        private float originalPitch = 1f;
        private float originalVolume = 0.5f;
        private Color originalTint = Color.white;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<EnemyHealth>();
            sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) originalTint = sr.color;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            foreach (var col in GetComponents<Collider2D>())
                col.isTrigger = false;
        }

        private void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;

            arena = FindFirstObjectByType<BossArenaManager>();
            if (arena == null)
            {
                var go = new GameObject("BossArenaManager");
                arena = go.AddComponent<BossArenaManager>();
            }
            arena.SetActive(true);

            arenaCenterX = transform.position.x;

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

            nextJumpTime = Time.time + Random.Range(jumpIntervalRange.x, jumpIntervalRange.y);
            nextDropTime = Time.time + 1.5f;
            nextAttackTime = Time.time + Random.Range(attackCooldownRange.x, attackCooldownRange.y);

            var spawner = FindFirstObjectByType<Kikiri9.Levels.LevelSpawner>();
            if (spawner != null) spawner.enabled = false;

            ApplySuspenseMusic();
            EnterIdle();
        }

        private void ApplySuspenseMusic()
        {
            var session = GameObject.Find("Session");
            if (session != null) musicSource = session.GetComponent<AudioSource>();
            if (musicSource == null) return;
            originalPitch = musicSource.pitch;
            originalVolume = musicSource.volume;
            musicSource.pitch = suspenseMusicPitch;
            musicSource.volume = suspenseMusicVolume;
        }

        private void RestoreMusic()
        {
            if (musicSource == null) return;
            musicSource.pitch = originalPitch;
            musicSource.volume = originalVolume;
        }

        private void Update()
        {
            if (defeated || health == null) return;
            if (IsDefeatedViaReflection()) { TriggerDefeatSequence(); return; }

            if (state == BossState.Idle)
            {
                TryScheduleJump();
                TryScheduleDrop();
                if (Time.time >= nextAttackTime) EnterAlert();
            }
        }

        private void TryScheduleJump()
        {
            if (Time.time < nextJumpTime) return;
            if (!IsGrounded()) return;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            nextJumpTime = Time.time + Random.Range(jumpIntervalRange.x, jumpIntervalRange.y);
        }

        private void TryScheduleDrop()
        {
            if (Time.time < nextDropTime) return;
            DropItem();
            nextDropTime = Time.time + dropInterval;
        }

        private void EnterIdle()
        {
            state = BossState.Idle;
            invulnerabilityToPlayerActive = false;
            nextAttackTime = Time.time + Random.Range(attackCooldownRange.x, attackCooldownRange.y);
        }

        private void EnterAlert()
        {
            state = BossState.Alert;
            invulnerabilityToPlayerActive = true; // durante telegraph no lastima
            stateEndTime = Time.time + telegraphDuration;
            StartCoroutine(TelegraphFlash(telegraphDuration));
            StartCoroutine(AlertToLunge());
        }

        private IEnumerator AlertToLunge()
        {
            yield return new WaitForSeconds(telegraphDuration);
            if (defeated) yield break;
            EnterLunge();
        }

        private void EnterLunge()
        {
            state = BossState.Lunge;
            invulnerabilityToPlayerActive = false;
            stateEndTime = Time.time + lungeDuration;
            StartCoroutine(LungeRoutine());
        }

        private IEnumerator LungeRoutine()
        {
            while (Time.time < stateEndTime && player != null && !defeated)
            {
                float dir = Mathf.Sign(player.position.x - transform.position.x);
                if (Mathf.Approximately(dir, 0f)) dir = wanderDir;
                rb.linearVelocity = new Vector2(dir * lungeSpeed, rb.linearVelocity.y);
                yield return null;
            }
            if (!defeated) EnterRecover();
        }

        private void EnterRecover()
        {
            state = BossState.Recover;
            invulnerabilityToPlayerActive = true; // el jugador puede pegarle sin recibir dano
            stateEndTime = Time.time + recoverDuration;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            StartCoroutine(RecoverTint());
            StartCoroutine(RecoverToIdle());
        }

        private IEnumerator RecoverTint()
        {
            if (sr == null) yield break;
            var old = sr.color;
            sr.color = new Color(0.6f, 1f, 0.6f, 1f); // leve verdoso = vulnerable
            yield return new WaitForSeconds(recoverDuration);
            if (sr != null) sr.color = old;
        }

        private IEnumerator RecoverToIdle()
        {
            yield return new WaitForSeconds(recoverDuration);
            if (defeated) yield break;
            EnterIdle();
        }

        private IEnumerator TelegraphFlash(float dur)
        {
            if (sr == null) yield break;
            float t = 0f;
            while (t < dur)
            {
                sr.color = new Color(1f, 0.3f, 0.3f, 1f);
                yield return new WaitForSeconds(0.08f);
                sr.color = originalTint;
                yield return new WaitForSeconds(0.08f);
                t += 0.16f;
            }
            if (sr != null) sr.color = originalTint;
        }

        private void FixedUpdate()
        {
            if (defeated) return;
            if (state != BossState.Idle) return;

            float dx = transform.position.x - arenaCenterX;
            if (dx > wanderRange) wanderDir = -1;
            else if (dx < -wanderRange) wanderDir = 1;
            // Pequena aleatoriedad para romper el patron robotico
            if (Random.value < 0.005f) wanderDir = -wanderDir;

            rb.linearVelocity = new Vector2(wanderDir * horizontalWanderSpeed, rb.linearVelocity.y);
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (defeated) return;
            if (invulnerabilityToPlayerActive) return;
            if (!other.collider.CompareTag("Player")) return;
            if (Time.time < nextContactTime) return;
            nextContactTime = Time.time + contactCooldown;
            var ph = other.collider.GetComponent<Kikiri9.Session.PlayerHealth>();
            if (ph != null) ph.TakeDamage(contactDamage);
        }

        private bool IsGrounded()
        {
            Vector2 pos = (Vector2)transform.position + groundCheckOffset;
            var hits = Physics2D.OverlapCircleAll(pos, groundCheckRadius, groundLayer);
            foreach (var h in hits)
            {
                if (h == null) continue;
                if (h.isTrigger) continue;
                if (h.transform == transform) continue;
                if (h.transform.IsChildOf(transform)) continue;
                if (h.CompareTag("Player")) continue;
                if (h.CompareTag("Enemy")) continue;
                if (h.CompareTag("Pickup")) continue;
                if (h.CompareTag("Bread")) continue;
                return true;
            }
            return false;
        }

        private void DropItem()
        {
            float roll = Random.value;
            GameObject prefab;
            if (roll < 0.55f) prefab = breadDropPrefab;
            else if (roll < 0.9f) prefab = bottleDropPrefab;
            else prefab = breadBoostPrefab;

            if (prefab == null) prefab = breadDropPrefab;
            if (prefab == null) return;

            // Clamp al rango del arena para que el drop caiga donde el jugador alcanza.
            float offset = Random.Range(-arenaHalfWidth * 0.8f, arenaHalfWidth * 0.8f);
            var pos = new Vector3(arenaCenterX + offset, breadSpawnY, 0f);
            var drop = Instantiate(prefab, pos, Quaternion.identity);
            AddGravityToDrop(drop);
        }

        private static void AddGravityToDrop(GameObject drop)
        {
            if (drop == null) return;
            var rb = drop.GetComponent<Rigidbody2D>();
            if (rb == null) rb = drop.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = new Vector2(0f, -1f);
            var floater = drop.GetComponent<Kikiri9.World.PickupFloater>();
            if (floater != null) floater.enabled = false;
        }

        private void TriggerDefeatSequence()
        {
            if (defeated) return;
            defeated = true;

            for (int i = 0; i < defeatBreadBurst; i++)
            {
                if (breadDropPrefab == null) break;
                float offset = Random.Range(-arenaHalfWidth * 0.8f, arenaHalfWidth * 0.8f);
                var pos = new Vector3(arenaCenterX + offset, breadSpawnY, 0f);
                var drop = Instantiate(breadDropPrefab, pos, Quaternion.identity);
                AddGravityToDrop(drop);
            }
            if (breadBoostPrefab != null)
            {
                var pos = new Vector3(arenaCenterX, breadSpawnY + 0.5f, 0f);
                var drop = Instantiate(breadBoostPrefab, pos, Quaternion.identity);
                AddGravityToDrop(drop);
            }

            RestoreMusic();
            if (arena != null) arena.SetActive(false);
            var spawner = FindFirstObjectByType<Kikiri9.Levels.LevelSpawner>();
            if (spawner != null) spawner.enabled = true;
        }

        private bool IsDefeatedViaReflection()
        {
            var f = typeof(EnemyHealth).GetField("isDefeated",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return f != null && (bool)f.GetValue(health);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);
        }
    }
}
