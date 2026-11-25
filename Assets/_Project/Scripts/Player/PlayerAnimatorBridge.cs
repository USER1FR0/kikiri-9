// Bridges PlayerHealth events to the Animator: fires Hit on damage, Dead on death.
// Safely checks each parameter exists in the controller so an older controller does
// not spam warnings.

using Kikiri9.Session;
using UnityEngine;

namespace Kikiri9.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimatorBridge : MonoBehaviour
    {
        private Animator animator;
        private PlayerHealth health;
        private bool hasHit, hasDead;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            health = GetComponent<PlayerHealth>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                foreach (var p in animator.parameters)
                {
                    if (p.name == "Hit") hasHit = true;
                    else if (p.name == "Dead") hasDead = true;
                }
            }
        }

        private void OnEnable()
        {
            if (health == null) return;
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            if (health == null) return;
            health.OnHealthChanged -= HandleHealthChanged;
            health.OnDeath -= HandleDeath;
        }

        private int lastHealth = -1;
        private void HandleHealthChanged(int current, int max)
        {
            if (lastHealth >= 0 && current < lastHealth && current > 0 && hasHit)
                animator.SetTrigger("Hit");
            lastHealth = current;
        }

        private void HandleDeath()
        {
            if (hasDead) animator.SetTrigger("Dead");
        }
    }
}
