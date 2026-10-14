// Bread projectile. Flies forward with gentle gravity, lands on first collision, blinks
// 3 times, spawns a splash effect sprite over the landing point, then self-destroys. The
// collider is disabled on landing so the pollo can walk straight through the resting pan.

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

        public void SetSplashSprite(Sprite s) => splashSprite = s;

        public void Launch(Vector2 velocity)
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();
            rb.linearVelocity = velocity;
            spawnTime = Time.time;
        }

        private float spawnTime;
        [SerializeField] private float grace = 0.08f;

        private void OnCollisionEnter2D(Collision2D _)
        {
            if (landed) return;
            if (Time.time - spawnTime < grace) return; // ignore muzzle contact
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
