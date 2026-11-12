// Carga los SFX globales desde Resources/Audio/ y los asigna a los singletons que los usan.
// Permite que EnemyHealth.ImpactoClip, ComboManager.comboSfx, etc. esten listos sin tener
// que arrastrarlos en cada escena.

using Kikiri9.Enemies;
using UnityEngine;

namespace Kikiri9.Session.FX
{
    public class AudioClipLoader : MonoBehaviour
    {
        private void Awake()
        {
            // Los paths usan el AudioClip name sin extension, buscados en Resources/.
            var impacto = Resources.Load<AudioClip>("Audio/SFX/impacto");
            if (impacto != null) EnemyHealth.ImpactoClip = impacto;

            var combo = Resources.Load<AudioClip>("Audio/SFX/combo");
            if (combo != null && ComboManager.Instance != null)
            {
                // Inyectar el clip en ComboManager por reflexion (es private serializable)
                var f = typeof(ComboManager).GetField("comboSfx",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (f != null) f.SetValue(ComboManager.Instance, combo);
            }
        }
    }
}
