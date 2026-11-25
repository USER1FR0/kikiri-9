// Pausa breve del tiempo del juego cuando pasa un impacto importante.
// Garantiza restaurar timeScale a 1 aunque se encadenen multiples freezes
// o se interrumpa la coroutine por otra llamada.

using System.Collections;
using UnityEngine;

namespace Kikiri9.Session.FX
{
    public class HitStop : MonoBehaviour
    {
        public static HitStop Instance { get; private set; }
        private const float NormalTimeScale = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Time.timeScale = NormalTimeScale;
                Instance = null;
            }
        }

        public static void Freeze(float scale, float duration)
        {
            if (Instance == null) return;
            Instance.StopAllCoroutines();
            // Restaurar a normal antes de empezar el nuevo freeze (no capturar valor actual)
            Time.timeScale = NormalTimeScale;
            Instance.StartCoroutine(Instance.DoFreeze(scale, duration));
        }

        private IEnumerator DoFreeze(float scale, float duration)
        {
            Time.timeScale = scale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = NormalTimeScale;
        }
    }
}
