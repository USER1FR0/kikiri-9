// Alimenta el visual de la rafaga (KikiriRafagaVisual) leyendo el cooldown real
// del componente PlayerPunch del pollo.

using Kikiri9.Player;
using Kikiri9.Visuales;
using UnityEngine;

namespace Kikiri9.UI
{
    public class RafagaVisualDriver : MonoBehaviour
    {
        private KikiriRafagaVisual visual;
        private PlayerPunch punch;

        private void Awake()
        {
            visual = GetComponentInChildren<KikiriRafagaVisual>();
        }

        private void Update()
        {
            if (visual == null) return;
            if (punch == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) punch = p.GetComponent<PlayerPunch>();
            }
            if (punch == null) return;
            visual.ActualizarRecarga(punch.Remaining, punch.Cooldown);
        }
    }
}
