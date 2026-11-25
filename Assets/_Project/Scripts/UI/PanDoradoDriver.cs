// Alimenta el visual del pan dorado (KikiriPanDoradoVisual) con el tiempo restante
// del InfiniteBreadBuff del pollo.

using Kikiri9.Player;
using Kikiri9.Visuales;
using UnityEngine;

namespace Kikiri9.UI
{
    public class PanDoradoDriver : MonoBehaviour
    {
        private KikiriPanDoradoVisual visual;
        private InfiniteBreadBuff buff;
        private const float DuracionTotal = 10f;

        private void Awake()
        {
            visual = GetComponent<KikiriPanDoradoVisual>();
        }

        private void Update()
        {
            if (visual == null) return;
            if (buff == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) buff = p.GetComponent<InfiniteBreadBuff>();
            }
            if (buff == null) { visual.ActualizarDuracion(0f, DuracionTotal); return; }
            float restante = buff.IsActive ? buff.RemainingSeconds : 0f;
            visual.ActualizarDuracion(restante, DuracionTotal);
        }
    }
}
