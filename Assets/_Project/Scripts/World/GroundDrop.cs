// Cae con gravedad, rebota 2-3 veces al tocar landingY, luego se queda recogible.

using UnityEngine;

namespace Kikiri9.World
{
    public class GroundDrop : MonoBehaviour
    {
        [SerializeField] private float landingY = -3.5f;
        [SerializeField] private float despawnDistanceBehind = 25f;
        [SerializeField] private float maxLifetimeAfterLand = 20f;
        [SerializeField] private int maxBounces = 3;
        [SerializeField] private float bounceDamping = 0.5f;

        private Rigidbody2D rb;
        private bool landed;
        private int bouncesLeft;
        private Transform camT;
        private float destroyAfterLandTime = -1f;

        public void Configure(float landY)
        {
            landingY = landY;
            bouncesLeft = maxBounces;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (Camera.main != null) camT = Camera.main.transform;
            bouncesLeft = maxBounces;
        }

        private void FixedUpdate()
        {
            if (landed || rb == null) return;
            if (transform.position.y <= landingY && rb.linearVelocity.y <= 0f)
            {
                if (bouncesLeft > 0)
                {
                    // Rebote: invertir velocidad vertical con damping
                    var v = rb.linearVelocity;
                    v.y = Mathf.Abs(v.y) * bounceDamping;
                    if (v.y < 0.8f) { landed = true; v = Vector2.zero; }
                    rb.linearVelocity = v;
                    var p = transform.position;
                    p.y = landingY;
                    transform.position = p;
                    bouncesLeft--;
                }
                else
                {
                    landed = true;
                }

                if (landed)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.gravityScale = 0f;
                    foreach (var c in GetComponents<Collider2D>()) c.isTrigger = true;
                    destroyAfterLandTime = Time.time + maxLifetimeAfterLand;
                }
            }
        }

        private void Update()
        {
            if (destroyAfterLandTime > 0f && Time.time > destroyAfterLandTime) { Destroy(gameObject); return; }
            if (camT == null) return;
            if (transform.position.x < camT.position.x - despawnDistanceBehind) Destroy(gameObject);
        }
    }
}
