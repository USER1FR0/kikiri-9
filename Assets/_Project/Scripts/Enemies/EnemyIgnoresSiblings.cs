// Ignora colisiones entre este Enemy y todos los otros con tag Enemy en la escena.
// Evita que zombies y obstaculos (que comparten tag Enemy) se solapen fisicamente.
// El pollo y el pan siguen colisionando con ellos normalmente.

using UnityEngine;

namespace Kikiri9.Enemies
{
    public class EnemyIgnoresSiblings : MonoBehaviour
    {
        private Collider2D ownCol;

        private void Start()
        {
            ownCol = GetComponent<Collider2D>();
            if (ownCol == null) return;

            // Ignorar colision con todos los otros Enemy activos
            foreach (var go in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                if (go == gameObject) continue;
                var other = go.GetComponent<Collider2D>();
                if (other != null) Physics2D.IgnoreCollision(ownCol, other, true);
            }
        }
    }
}
