// Enemy hit points. When health reaches zero the enemy plays its defeat animation, stops
// moving, stops colliding (so the pollo can pass through) and is destroyed after a delay.

using System;
using UnityEngine;

namespace Kikiri9.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 1;
        [SerializeField] private int defeatPoints = 100;
        [SerializeField] private Sprite defeatVfxSprite;
        [SerializeField] private string defeatAnimTrigger = "Defeat";
        [SerializeField] private float destroyAfterDefeat = 1.5f;

        // Fired when any enemy is defeated, with its point value. ScoreTracker subscribes.
        public static event Action<int> OnAnyDefeated;

        public static AudioClip ImpactoClip;

        private int currentHealth;
        private bool isDefeated;
        private Animator animator;
        private Collider2D col;
        private Rigidbody2D rb;
        private EnemyMovement movement;

        private void Awake()
        {
            currentHealth = maxHealth;
            animator = GetComponent<Animator>();
            col = GetComponent<Collider2D>();
            rb = GetComponent<Rigidbody2D>();
            movement = GetComponent<EnemyMovement>();
        }

        public void TakeDamage(int amount)
        {
            TakeDamage(amount, Kikiri9.Enemies.New.DamageSource.Bread, transform.position);
        }

        public void TakeDamage(int amount, Kikiri9.Enemies.New.DamageSource src, Vector3 impactPos)
        {
            if (isDefeated) return;
            var vuln = GetComponent<Kikiri9.Enemies.New.EnemyVulnerability>();
            if (vuln != null && !vuln.AcceptsDamage(src, impactPos))
            {
                // Rechazo visual sutil
                var flashRej = GetComponent<EnemyDamageFlash>();
                if (flashRej != null) flashRej.Flash();
                return;
            }
            currentHealth -= amount;
            var flash = GetComponent<EnemyDamageFlash>();
            if (flash != null) flash.Flash();
            if (currentHealth <= 0) Defeat();
        }

        private void Defeat()
        {
            isDefeated = true;
            int mult = Kikiri9.Session.FX.ComboManager.CurrentMultiplier();
            OnAnyDefeated?.Invoke(defeatPoints * mult);
            Kikiri9.Session.FX.ComboManager.NotifyKill();
            Kikiri9.Session.FX.HitStop.Freeze(0.1f, 0.03f);
            Kikiri9.Session.FX.CameraShake.Shake(0.15f, 0.1f);
            if (ImpactoClip != null)
                AudioSource.PlayClipAtPoint(ImpactoClip, transform.position, 0.8f);
            if (defeatVfxSprite != null)
                Kikiri9.World.VfxSpawner.Spawn(defeatVfxSprite, transform.position + Vector3.up * 0.8f, 0.5f, 0.5f, 18);
            if (animator != null)
            {
                // SetTrigger solo si el parametro existe
                bool hasDefeatTrigger = false, hasStateParam = false;
                foreach (var par in animator.parameters)
                {
                    if (par.name == defeatAnimTrigger && par.type == AnimatorControllerParameterType.Trigger) hasDefeatTrigger = true;
                    if (par.name == "State" && par.type == AnimatorControllerParameterType.Int) hasStateParam = true;
                }
                if (hasDefeatTrigger) animator.SetTrigger(defeatAnimTrigger);
                if (hasStateParam) animator.SetInteger("State", 99);

                // Play(defeat) solo si el estado existe
                var ac = animator.runtimeAnimatorController;
                if (ac != null)
                {
                    foreach (var c in ac.animationClips)
                    {
                        if (c == null) continue;
                        string cn = c.name;
                        if (cn.ToLower().Contains("defeat"))
                        {
                            int hash = Animator.StringToHash(cn);
                            if (animator.HasState(0, hash)) animator.Play(hash, 0, 0f);
                            break;
                        }
                    }
                }
            }
            if (col != null) col.enabled = false;
            if (movement != null) movement.enabled = false;
            // A kinematic body keeps its velocity after the movement script is disabled.
            if (rb != null) rb.linearVelocity = Vector2.zero;
            Destroy(gameObject, destroyAfterDefeat);
        }

        // Right click the component in the Inspector during Play mode to test the defeat.
        [ContextMenu("Debug Take Damage")]
        private void DebugTakeDamage()
        {
            if (!Application.isPlaying) return;
            TakeDamage(1);
        }
    }
}
