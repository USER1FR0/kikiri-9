// Enemy hit points. When health reaches zero the enemy plays its defeat animation, stops
// moving, stops colliding (so the pollo can pass through) and is destroyed after a delay.

using UnityEngine;

namespace Kikiri9.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 1;
        [SerializeField] private string defeatAnimTrigger = "Defeat";
        [SerializeField] private float destroyAfterDefeat = 0.5f;

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
            if (isDefeated) return;
            currentHealth -= amount;
            if (currentHealth <= 0) Defeat();
        }

        private void Defeat()
        {
            isDefeated = true;
            if (animator != null) animator.SetTrigger(defeatAnimTrigger);
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
