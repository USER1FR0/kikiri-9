// Mantiene un fondo pegado a la camara (follow 100% + lockedY). Para la capa del cielo
// que no debe tener slip/wrap, solo seguir con la camara.

using UnityEngine;

namespace Kikiri9.World
{
    [DisallowMultipleComponent]
    public class CameraFollowBg : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float lockedY;
        [SerializeField] private float lockedZ = 10f;

        private void LateUpdate()
        {
            if (cameraTransform == null) return;
            transform.position = new Vector3(cameraTransform.position.x, lockedY, lockedZ);
        }
    }
}
