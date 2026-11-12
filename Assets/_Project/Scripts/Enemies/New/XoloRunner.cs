// Xolo: corre rapido pegado al piso. Vulnerable solo al slide. SIN SONIDO.

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
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            if (Camera.main != null) camT = Camera.main.transform;
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = new Vector2(speedX, rb.linearVelocity.y);
        }

        private void Update()
        {
            if (camT == null) return;
            if (transform.position.x < camT.position.x - despawnDistanceBehind)
                Destroy(gameObject);
        }
    }
}
