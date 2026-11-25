// Mini-boss momia v3: fixes de encuadre, ataque y rango.
//  - Escala bajada (ahora se ajusta en el prefab, no en el componente)
//  - Patron de ataque: cada attackCooldown se abalanza hacia el pollo + flash rojo
//  - Contact damage: tocarla quita 1 vida al pollo via PlayerHealth
//  - La camara se bloquea al centro del arena (ArenaCameraLock) para que el pollo
//    no deje atras al jefe
//  - Pollo clampeado a ±clampRange del centro del arena por ClampPlayerToArena

using Kikiri9.Player;
using Kikiri9.Session;
using Kikiri9.World;
using UnityEngine;

namespace Kikiri9.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyHealth))]
    public class MiniBossMomia : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float horizontalWanderSpeed = 3f;
        [SerializeField] private float wanderRange = 3f;
        [SerializeField] private float jumpInterval = 2.2f;
        [SerializeField] private float jumpVelocity = 11f;

        [Header("Ataque")]
        [SerializeField] private float attackCooldown = 3.5f;
        [SerializeField] private float lungeSpeed = 9f;
        [SerializeField] private float lungeDuration = 0.4f;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private float contactCooldown = 1f;

        [Header("Drops")]
        [SerializeField] private GameObject breadDropPrefab;
        [SerializeField] private GameObject bottleDropPrefab;
        [SerializeField] private GameObject breadBoostPrefab;
        [SerializeField] private float dropInterval = 2.5f;
        [SerializeField] private int defeatBreadBurst = 6;
        [SerializeField] private float breadSpawnY = 4f;

        [Header("Musica suspenso")]
        [SerializeField] private float suspenseMusicPitch = 0.75f;
        [SerializeField] private float suspenseMusicVolume = 0.7f;

        private Rigidbody2D rb;
        private Transform player;
        private EnemyHealth health;
        private BossArenaManager arena;
        private SpriteRenderer sr;

        private float nextJumpTime;
        private float nextDropTime;
        private float nextAttackTime;
        private float lungeEndTime;
        private float nextContactTime;
        private float arenaCenterX;
        private int wanderDir = 1;
        private bool lunging;
        private bool defeated;

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

            // Arena camera lock + player clamp
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

            nextJumpTime = Time.time + 1f;
            nextDropTime = Time.time + 1.5f;
            nextAttackTime = Time.time + 2f;

            var spawner = FindFirstObjectByType<Kikiri9.Levels.LevelSpawner>();
            if (spawner != null) spawner.enabled = false;

            ApplySuspenseMusic();
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

            if (Time.time >= nextJumpTime && IsGroundedApprox())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                nextJumpTime = Time.time + jumpInterval;
            }

            if (Time.time >= nextDropTime)
            {
                DropItem();
                nextDropTime = Time.time + dropInterval;
            }

            if (Time.time >= nextAttackTime && !lunging && player != null)
            {
                StartCoroutine(LungeAttack());
                nextAttackTime = Time.time + attackCooldown;
            }
        }

        private System.Collections.IEnumerator LungeAttack()
        {
            lunging = true;
            // Telegraph: flash rojo 0.4s
            if (sr != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    sr.color = new Color(1f, 0.3f, 0.3f, 1f);
                    yield return new WaitForSeconds(0.05f);
                    sr.color = originalTint;
                    yield return new WaitForSeconds(0.05f);
                }
                sr.color = originalTint;
            }

            // Lunge: dash hacia el pollo durante lungeDuration
            lungeEndTime = Time.time + lungeDuration;
            while (Time.time < lungeEndTime && player != null)
            {
                float dir = Mathf.Sign(player.position.x - transform.position.x);
                rb.linearVelocity = new Vector2(dir * lungeSpeed, rb.linearVelocity.y);
                yield return null;
            }
            lunging = false;
        }

        private void FixedUpdate()
        {
            if (defeated || lunging) return;

            float dx = transform.position.x - arenaCenterX;
            if (dx > wanderRange) wanderDir = -1;
            else if (dx < -wanderRange) wanderDir = 1;

            rb.linearVelocity = new Vector2(wanderDir * horizontalWanderSpeed, rb.linearVelocity.y);
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (defeated) return;
            if (!other.collider.CompareTag("Player")) return;
            if (Time.time < nextContactTime) return;
            nextContactTime = Time.time + contactCooldown;
            var ph = other.collider.GetComponent<Kikiri9.Session.PlayerHealth>();
            if (ph != null) ph.TakeDamage(contactDamage);
        }

        private bool IsGroundedApprox()
        {
            return Mathf.Abs(rb.linearVelocity.y) < 0.2f;
        }

        private void DropItem()
        {
            if (player == null) return;
            float roll = Random.value;
            GameObject prefab;
            if (roll < 0.55f) prefab = breadDropPrefab;
            else if (roll < 0.9f) prefab = bottleDropPrefab;
            else prefab = breadBoostPrefab;

            if (prefab == null) prefab = breadDropPrefab;
            if (prefab == null) return;

            float offset = Random.Range(-2.5f, 2.5f);
            var pos = new Vector3(player.position.x + offset, breadSpawnY, 0f);
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
                if (breadDropPrefab == null || player == null) break;
                float offset = Random.Range(-3.5f, 3.5f);
                var pos = new Vector3(player.position.x + offset, breadSpawnY, 0f);
                var drop = Instantiate(breadDropPrefab, pos, Quaternion.identity);
                AddGravityToDrop(drop);
            }
            if (breadBoostPrefab != null && player != null)
            {
                var pos = new Vector3(player.position.x, breadSpawnY + 0.5f, 0f);
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
    }
}
