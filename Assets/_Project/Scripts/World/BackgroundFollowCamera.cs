// Mueve un objeto raiz (que contiene las 3 capas adaptativas) siguiendo la camara en X.
// El componente FondoAdaptativoKikiri9 lee la camara directamente, pero este follower
// evita que los layers se queden en X=0 cuando la camara avanza.

using UnityEngine;

namespace Kikiri9.World
{
    [DisallowMultipleComponent]
    public class BackgroundFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float lockedY;

        private void LateUpdate()
        {
            if (cameraTransform == null) return;
            transform.position = new Vector3(cameraTransform.position.x, lockedY, transform.position.z);
        }
    }
}
