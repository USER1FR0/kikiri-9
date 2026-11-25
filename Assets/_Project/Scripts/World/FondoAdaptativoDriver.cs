// Alimenta el paneo del FondoAdaptativoKikiri9 con el progreso del WorldScroller
// (distancia recorrida / targetDistance). El resultado: horizonte y fachada se
// deslizan sutilmente conforme el nivel avanza, sin tiling ni duplicados.

using Kikiri9.Levels;
using UnityEngine;

namespace Kikiri9.World
{
    [RequireComponent(typeof(FondoAdaptativoKikiri9))]
    public class FondoAdaptativoDriver : MonoBehaviour
    {
        private FondoAdaptativoKikiri9 fondo;
        private LevelController lc;

        private void Awake()
        {
            fondo = GetComponent<FondoAdaptativoKikiri9>();
        }

        private void Update()
        {
            if (fondo == null) return;
            if (WorldScroller.Instance == null) return;
            if (lc == null) lc = FindFirstObjectByType<LevelController>();
            if (lc == null || lc.Definition == null) return;

            float target = lc.Definition.targetDistance;
            if (target <= 0f) return;
            float p = Mathf.Clamp01(WorldScroller.Instance.MetersAdvanced / target);
            fondo.SetProgress01(p);
        }
    }
}
