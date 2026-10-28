// Attached to planted obstacles (tomb, cart). The obstacle stays at a fixed world
// position; it's the pollo that runs forward. This script only despawns the object once
// it is far enough behind the camera, so passed obstacles don't pile up in the scene.

using UnityEngine;

namespace Kikiri9.World
{
    public class ObstacleScroller : MonoBehaviour
    {
        [SerializeField] private float despawnBehindCamera = 15f;

        private Camera cam;

        private void Start()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if (cam == null) return;
            if (transform.position.x < cam.transform.position.x - despawnBehindCamera)
            {
                Destroy(gameObject);
            }
        }
    }
}
