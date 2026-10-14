// Makes a tiled ground follow the camera horizontally so the world looks infinite.
// The ground must be wide enough that it always covers the view (default 80 units works
// for an ortho size of 4.5). Kinematic Rigidbody2D is recommended to avoid broadphase
// rebuild warnings when moved every frame.

using UnityEngine;

namespace Kikiri9.World
{
    [DisallowMultipleComponent]
    public class GroundFollower : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float lockedY = -3.5f;

        public void SetCamera(Transform t) => cameraTransform = t;

        private void LateUpdate()
        {
            if (cameraTransform == null) return;
            transform.position = new Vector3(cameraTransform.position.x, lockedY, 0f);
        }
    }
}
