// Follows the target smoothly on the X axis (and loosely on Y). Clamp above Y limit so
// the camera does not dip below the ground when the pollo falls.

using UnityEngine;

namespace Kikiri9.CameraSystem
{
    [DisallowMultipleComponent]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset = new Vector2(5f, 1f);
        [SerializeField, Min(0f)] private float smoothTime = 0.15f;
        [SerializeField] private float minY = -2f;

        private Vector3 velocity;

        public void SetTarget(Transform t) => target = t;

        private void LateUpdate()
        {
            if (target == null) return;
            var desired = new Vector3(
                target.position.x + offset.x,
                Mathf.Max(target.position.y + offset.y, minY),
                transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }
    }
}
