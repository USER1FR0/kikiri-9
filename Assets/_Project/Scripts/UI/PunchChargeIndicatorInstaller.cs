// Instala el PunchChargeIndicator al iniciar la escena.

using UnityEngine;

namespace Kikiri9.UI
{
    public class PunchChargeIndicatorInstaller : MonoBehaviour
    {
        private void Start()
        {
            PunchChargeIndicator.Install();
        }
    }
}
