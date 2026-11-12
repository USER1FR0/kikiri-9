// Player hit points for levels. Añadido: SetTemporaryInvulnerability() publico para
// que PlayerPunch pueda proteger al pollo durante la rafaga.

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
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.LevelOutro) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            Kikiri9.Session.FX.ComboManager.NotifyDamage();
            Kikiri9.Session.FX.CameraShake.Shake(0.4f, 0.2f);
            Kikiri9.Session.FX.HitStop.Freeze(0.05f, 0.05f);

            if (CurrentHealth <= 0)
            {
                IsDead = true;
                TriggerDeathFeedbackThenGameOver();
                return;
            }

            var sfx = GetComponent<Kikiri9.Player.PlayerSfx>();
            if (sfx != null) sfx.PlayDamage();
            if (damageVfxSprite != null)
                Kikiri9.World.VfxSpawner.Spawn(damageVfxSprite, transform.position + Vector3.up * 1f, 0.5f, 0.6f, 22);
            StartCoroutine(InvulnerabilityRoutine());
        }

        public void SetTemporaryInvulnerability(float duration)
        {
            StartCoroutine(TempInvulnRoutine(duration));
        }

        private IEnumerator TempInvulnRoutine(float duration)
        {
            IsInvulnerable = true;
            yield return new WaitForSeconds(duration);
            IsInvulnerable = false;
            RestoreIgnoredCollisions();
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

        private void TriggerDeathFeedbackThenGameOver()
        {
            // Parar el auto-run para que la animacion Death se vea sin desplazamiento
            var pc = GetComponent<Kikiri9.Player.PlayerController>();
            if (pc != null) pc.enabled = false;

            // Congelar fisica: pollo se queda en su sitio y toca el piso
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            // Desactivar colisiones con enemigos durante la muerte
            foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;

            // Disparar la animacion Death del pollito (trigger "Dead")
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                bool hasDead = false;
                foreach (var par in animator.parameters)
                    if (par.name == "Dead" && (par.type == AnimatorControllerParameterType.Trigger
                        || par.type == AnimatorControllerParameterType.Bool)) { hasDead = true; break; }
                if (hasDead) animator.SetTrigger("Dead");
                // Fallback: Play directo al estado "Death" si existe
                if (animator.HasState(0, Animator.StringToHash("Death")))
                    animator.Play("Death", 0, 0f);
            }

            var fx = FindFirstObjectByType<Kikiri9.Session.FX.DeathFeedback>();
            if (fx == null)
            {
                var go = new GameObject("DeathFeedback");
                fx = go.AddComponent<Kikiri9.Session.FX.DeathFeedback>();
            }
            fx.PlayDeath(transform, () => {
                OnDeath?.Invoke();
            });
        }
    }
}
