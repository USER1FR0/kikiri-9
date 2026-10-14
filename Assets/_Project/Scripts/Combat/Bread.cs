// Straight-line bread projectile. No gravity, no spin. Lives for a short time then dies.

using UnityEngine;

namespace Kikiri9.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bread : MonoBehaviour
    {
        [SerializeField] private float lifetime = 2.0f;

        private float dieAt;

        public void Launch(Vector2 velocity)
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.linearVelocity = velocity;
            rb.gravityScale = 0f;
            dieAt = Time.time + lifetime;
        }

        private void Update()
        {
            if (Time.time >= dieAt) Destroy(gameObject);
        }
    }
}
