// Marker runtime-nulo que recuerda la Y original de un objeto antes de que el builder
// de fondos lo bajara al piso actual. Permite correr "Apply Adaptive Backgrounds V3"
// multiples veces (cambiando GroundY) sin que decoraciones/enemigos se acumulen abajo.

using UnityEngine;

namespace Kikiri9.World
{
    [DisallowMultipleComponent]
    public class FloorShiftMarker : MonoBehaviour
    {
        public float originalY;
        public bool hasOriginal;
    }
}
