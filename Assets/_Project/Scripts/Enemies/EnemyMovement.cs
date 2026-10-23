// Moves an enemy to the left at a constant speed. The enemy destroys itself once it is far
// behind the camera, so enemies the pollo already passed do not pile up in the scene.

using Kikiri9.World;
using UnityEngine;

namespace Kikiri9.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private float speedX = -2f;
        [SerializeField] private float despawnDistanceBehind = 20f;
        [SerializeField] private bool useWorldScrollerIfPresent = false;

        private Rigidbody2D rb;
        private Transform cameraTransform;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            if (Camera.main != null) cameraTransform = Camera.main.transform;
        }

        private void FixedUpdate()
        {
            float sx = speedX;
            if (useWorldScrollerIfPresent && WorldScroller.Instance != null)
            {
                sx = -WorldScroller.Instance.CurrentSpeed;
            }
            // Keep Y velocity in case gravity is added later.
            rb.linearVelocity = new Vector2(sx, rb.linearVelocity.y);
        }

        private void Update()
        {
            if (cameraTransform == null) return;
            if (transform.position.x < cameraTransform.position.x - despawnDistanceBehind)
                Destroy(gameObject);
        }
    }
}
