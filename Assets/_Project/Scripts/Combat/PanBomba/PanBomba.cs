// Pan bomba: trayectoria normal; al impactar explota en radio y mata multiples enemigos.

using System.Collections;
using Kikiri9.Enemies;
using Kikiri9.Enemies.New;
using UnityEngine;

namespace Kikiri9.Combat.Bomba
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PanBomba : MonoBehaviour
    {
        [SerializeField] private float explosionRadius = 2.5f;
        [SerializeField] private int damagePerEnemy = 3;
        [SerializeField] private float grace = 0.08f;
        [SerializeField] private Sprite explosionSprite;
        [SerializeField] private float explosionLife = 0.5f;

        private float spawnTime;
        private bool detonated;
        private Rigidbody2D rb;
        private Collider2D col;

        public void SetExplosionSprite(Sprite s) => explosionSprite = s;

        public void Launch(Vector2 vel)
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            rb.linearVelocity = vel;
            spawnTime = Time.time;
            Destroy(gameObject, 6f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (detonated) return;
            if (Time.time - spawnTime < grace) return;
            if (other.CompareTag("Player") || other.CompareTag("Pickup") || other.CompareTag("Bread")) return;
            Detonate();
        }

        private void Detonate()
        {
            detonated = true;
            var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (var h in hits)
            {
                if (h == null) continue;
                if (!h.CompareTag("Enemy")) continue;
                var eh = h.GetComponent<EnemyHealth>();
                if (eh == null) continue;
                eh.TakeDamage(damagePerEnemy, DamageSource.Rafaga, transform.position);
            }
            Kikiri9.Session.FX.CameraShake.Shake(0.5f, 0.3f);
            SpawnExplosionVfx();
            if (col != null) col.enabled = false;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            Destroy(gameObject, 0.1f);
        }

        private void SpawnExplosionVfx()
        {
            var go = new GameObject("Bomba_Explosion");
            go.transform.position = transform.position;
            go.transform.localScale = Vector3.one * (explosionRadius * 0.5f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = explosionSprite;
            sr.color = new Color(1f, 0.6f, 0.2f, 1f);
            sr.sortingOrder = 20;
            Destroy(go, explosionLife);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
