// InfiniteBreadBadge v2: deshabilitado. El indicador del buff "pan infinito" vive
// ahora solo en el canvas de HUD (PanDoradoHUD + PanDoradoDriver), no encima del pollo.
// Dejamos la API publica (Install) como no-op para no romper el instalador del power-up.

using UnityEngine;

namespace Kikiri9.UI
{
    public class InfiniteBreadBadge : MonoBehaviour
    {
        public static void Install(Sprite icon)
        {
            // No-op: el indicador world-space fue retirado. El HUD inferior lo reemplaza.
            _ = icon;
        }
    }
}
