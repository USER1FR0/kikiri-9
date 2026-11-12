// Bread projectile v2. Usa trigger para no colisionar fisicamente con pickups u obstaculos.
// Dana a enemigos al tocarlos. Al tocar algo que no sea pickup (enemigo, obstaculo, suelo)
// se queda pegado, parpadea 3 veces, spawnea un splash y se destruye.

using System.Collections;
using UnityEngine;

namespace Kikiri9.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bread : MonoBehaviour
    {
        [SerializeField] private float blinkInterval = 0.12f;
        [SerializeField] private int blinkCount = 3;
        [SerializeField] private Sprite splashSprite;
        [SerializeField] private float splashLifetime = 0.4f;
        [SerializeField] private float splashScale = 0.6f;

        private Rigidbody2D rb;
        private Collider2D col;
        private SpriteRenderer sr;
        private bool landed;
        private float spawnTime;
        [SerializeField] private float grace = 0.08f;

        public void SetSplashSprite(Sprite s) => splashSprite = s;

        public void Launch(Vector2 velocity)
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();
            rb.linearVelocity = velocity;
            spawnTime = Time.time;
            Destroy(gameObject, 5f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (landed) return;
            if (Time.time - spawnTime < grace) return;
            if (other == null) return;

            // Ignorar pickups, el propio pollo y otros panes (atraviesa sin detonar).
            if (other.CompareTag("Player")) return;
            if (other.CompareTag("Pickup")) return;
            if (other.CompareTag("Bread")) return;
            if (other.CompareTag("Enemy"))
            {
                var enemyHealth = other.GetComponent<Kikiri9.Enemies.EnemyHealth>();
                if (enemyHealth == null)
                    enemyHealth = other.GetComponentInParent<Kikiri9.Enemies.EnemyHealth>();
                if (enemyHealth != null)
                {
                    bool inAir = transform.position.y > -2.5f;
                    var src = inAir ? Kikiri9.Enemies.New.DamageSource.BreadInAir : Kikiri9.Enemies.New.DamageSource.Bread;
                    enemyHealth.TakeDamage(1, src, transform.position);
                }
            }

            Land();
        }

        private void Land()
        {
            landed = true;
            if (col != null) col.enabled = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            StartCoroutine(BlinkAndDie());
        }

        private IEnumerator BlinkAndDie()
        {
            if (sr != null)
            {
                for (int i = 0; i < blinkCount; i++)
                {
                    sr.enabled = false;
                    yield return new WaitForSeconds(blinkInterval);
                    sr.enabled = true;
                    yield return new WaitForSeconds(blinkInterval);
                }
                sr.enabled = false;
            }
            SpawnSplash();
            Destroy(gameObject);
        }

        private void SpawnSplash()
        {
            if (splashSprite == null) return;
            var go = new GameObject("Pan_Splash");
            go.transform.position = transform.position;
            go.transform.localScale = Vector3.one * splashScale;
            var s = go.AddComponent<SpriteRenderer>();
            s.sprite = splashSprite;
            s.sortingOrder = 16;
            Destroy(go, splashLifetime);
        }
    }
}
