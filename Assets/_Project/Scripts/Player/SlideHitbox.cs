// Durante el slide activa un hitbox trigger en los pies del pollo. Dana a enemigos
// con DamageSource.Slide. Se instala automaticamente al slide.

using Kikiri9.Enemies;
using Kikiri9.Enemies.New;
using UnityEngine;

namespace Kikiri9.Player
{
    public class SlideHitbox : MonoBehaviour
    {
        private int damage;
        private float lifetime;
        private float endTime;

        public void Setup(int dmg, float life)
        {
            damage = dmg;
            lifetime = life;
            endTime = Time.time + life;
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.0f, 0.5f);
            col.offset = new Vector2(0.6f, -0.35f);
        }

        private void Update()
        {
            if (Time.time >= endTime) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;
            var h = other.GetComponent<EnemyHealth>();
            if (h != null) h.TakeDamage(damage, DamageSource.Slide, transform.position);
        }
    }
}
