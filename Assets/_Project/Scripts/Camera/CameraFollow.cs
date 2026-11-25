// Follows the target smoothly on X only. The Y position stays wherever the camera was
// placed in the Scene view — so you can compose the view manually without Play undoing
// your framing. Enable followY if you ever want vertical tracking.

using UnityEngine;

namespace Kikiri9.CameraSystem
{
    [DisallowMultipleComponent]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float offsetX = 7f;
        [SerializeField, Min(0f)] private float smoothTime = 0.15f;
        [SerializeField] private bool followY = false;
        [SerializeField] private float offsetY = 1f;
        [SerializeField] private float minY = -2f;

        private Vector3 velocity;

        public void SetTarget(Transform t) => target = t;

        private void LateUpdate()
        {
            if (target == null) return;
            float x = target.position.x + offsetX;
            float y = followY ? Mathf.Max(target.position.y + offsetY, minY) : transform.position.y;
            var desired = new Vector3(x, y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }
    }
}
