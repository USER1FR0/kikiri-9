// Pan barredor: pan GRANDE, semi-transparente, atraviesa enemigos danandolos.
// No se destruye al tocar al primero. Vive 3s. Ideal para oleadas.

using Kikiri9.Enemies;
using Kikiri9.Enemies.New;
using UnityEngine;

namespace Kikiri9.Combat.Barredor
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PanBarredor : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private int damagePerEnemy = 1;

        private System.Collections.Generic.HashSet<int> alreadyHit = new System.Collections.Generic.HashSet<int>();

        public void Launch(Vector2 vel)
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.linearVelocity = vel;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null) return;
            if (!other.CompareTag("Enemy")) return;
            var h = other.GetComponent<EnemyHealth>();
            if (h == null) h = other.GetComponentInParent<EnemyHealth>();
            if (h == null) return;
            int id = h.GetInstanceID();
            if (alreadyHit.Contains(id)) return;
            alreadyHit.Add(id);
            h.TakeDamage(damagePerEnemy, DamageSource.Rafaga, transform.position);
        }
    }
}
