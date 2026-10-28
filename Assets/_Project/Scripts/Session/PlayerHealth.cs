// Player hit points for levels. When present on the pollo, intercepts enemy collisions and
// consumes one heart with a short invulnerability window (sprite blinks). At zero hearts
// fires OnDeath, which GameOverHandler listens to. The Sandbox scene has no PlayerHealth,
// so GameOverHandler keeps its original one-hit behavior there.

using System;
using System.Collections;
using UnityEngine;

namespace Kikiri9.Session
{
    public class PlayerHealth : MonoBehaviour
    {
        private const string EnemyTag = "Enemy";

        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float invulnerabilityDuration = 1f;
        [SerializeField] private float blinkInterval = 0.1f;
        [SerializeField] private Sprite damageVfxSprite;

        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsInvulnerable { get; private set; }
        public bool IsDead { get; private set; }

        private SpriteRenderer sr;
        private Collider2D ownCollider;
        private readonly System.Collections.Generic.List<Collider2D> ignoredDuringIframes = new System.Collections.Generic.List<Collider2D>();

        private void Awake()
        {
            CurrentHealth = maxHealth;
            sr = GetComponent<SpriteRenderer>();
            ownCollider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag(EnemyTag)) return;
            if (IsInvulnerable) { IgnorePair(collision.collider); return; }
            TakeDamage(1);
            IgnorePair(collision.collider);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!IsInvulnerable) return;
            if (collision.gameObject.CompareTag(EnemyTag)) IgnorePair(collision.collider);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(EnemyTag)) return;
            if (IsInvulnerable) { IgnorePair(other); return; }
            TakeDamage(1);
            IgnorePair(other);
        }

        private void IgnorePair(Collider2D other)
        {
            if (ownCollider == null || other == null) return;
            Physics2D.IgnoreCollision(ownCollider, other, true);
            if (!ignoredDuringIframes.Contains(other)) ignoredDuringIframes.Add(other);
        }

        private void RestoreIgnoredCollisions()
        {
            foreach (var c in ignoredDuringIframes)
                if (c != null && ownCollider != null) Physics2D.IgnoreCollision(ownCollider, c, false);
            ignoredDuringIframes.Clear();
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || IsInvulnerable) return;
            // Durante el outro del nivel el pollo no puede morir
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.LevelOutro) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0)
            {
                IsDead = true;
                OnDeath?.Invoke();
                return;
            }

            var sfx = GetComponent<Kikiri9.Player.PlayerSfx>();
            if (sfx != null) sfx.PlayDamage();
            if (damageVfxSprite != null)
                Kikiri9.World.VfxSpawner.Spawn(damageVfxSprite, transform.position + Vector3.up * 1f, 0.5f, 0.6f, 22);
            StartCoroutine(InvulnerabilityRoutine());
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            IsInvulnerable = true;
            float elapsed = 0f;
            while (elapsed < invulnerabilityDuration)
            {
                if (sr != null) sr.enabled = !sr.enabled;
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }
            if (sr != null) sr.enabled = true;
            IsInvulnerable = false;
            RestoreIgnoredCollisions();
        }
    }
}
