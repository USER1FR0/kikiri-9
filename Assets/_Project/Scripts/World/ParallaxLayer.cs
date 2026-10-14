// Infinite parallax layer. The GameObject sits near the camera (so the tiled sprite is
// always on screen) but visually "slips" relative to the camera. The slip wraps by the
// tile width so the content cycles without a visible seam.

using UnityEngine;

namespace Kikiri9.World
{
    [DisallowMultipleComponent]
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField, Range(0f, 1f)] private float parallaxFactor = 0.4f;
        [SerializeField] private float lockedY;
        [SerializeField, Min(0.1f)] private float tileWidth = 10f;

        private void LateUpdate()
        {
            if (cameraTransform == null) return;

            float cameraX = cameraTransform.position.x;
            float slip = cameraX * parallaxFactor;
            float wrapped = slip - Mathf.Floor(slip / tileWidth) * tileWidth;
            float x = cameraX - wrapped;
            transform.position = new Vector3(x, lockedY, transform.position.z);
        }
    }
}
