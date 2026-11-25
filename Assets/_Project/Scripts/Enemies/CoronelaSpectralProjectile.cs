// Proyectil espectral de la Coronela (jefe final, #14). Vuela en linea recta hacia la
// posicion del jugador capturada al momento del disparo (no es homing) y le quita 1
// corazon al tocarlo. Si no golpea nada, se autodestruye despues de un tiempo.

using Kikiri9.Session;
using UnityEngine;

namespace Kikiri9.Enemies
{
    public class CoronelaSpectralProjectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private int damage = 1;

        private Vector2 velocity;
        private bool hit;

        public void Launch(Vector2 vel)
        {
            velocity = vel;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hit) return;
            if (!other.CompareTag("Player")) return;
            hit = true;

            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
