// Xolo: corre rapido pegado al piso. Vulnerable al slide y al pan (ver
// EnemyVulnerability). SIN SONIDO.
//
// Kinematic + sin gravedad, igual que el resto de los enemigos (EnemyMovement): un
// cuerpo Dynamic con gravedad chocaba fisicamente contra los obstaculos solidos y se
// quedaba atorado contra ellos, generando contactos inestables con el jugador que
// disparaban el daño dos veces.

using UnityEngine;

namespace Kikiri9.Enemies.New
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyHealth))]
    public class XoloRunner : MonoBehaviour
    {
        [SerializeField] private float speedX = -3.2f;
        [SerializeField] private float despawnDistanceBehind = 20f;

        private Rigidbody2D rb;
        private Transform camT;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            if (Camera.main != null) camT = Camera.main.transform;
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = new Vector2(speedX, 0f);
        }

        private void Update()
        {
            if (camT == null) return;
            if (transform.position.x < camT.position.x - despawnDistanceBehind)
                Destroy(gameObject);
        }
    }
}
