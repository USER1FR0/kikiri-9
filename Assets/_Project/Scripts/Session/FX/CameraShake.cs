// Shake de camara sin drift acumulativo:
//  - Cada frame deshacemos el offset aplicado el frame anterior y aplicamos uno nuevo.
//  - Al terminar el shake, la camara queda EXACTAMENTE donde CameraFollow la posiciona
//    (sin desplazamiento residual).

using UnityEngine;

namespace Kikiri9.Session.FX
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private float timeLeft;
        private float amplitude;
        private Vector3 appliedOffset;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Shake(float amp, float dur)
        {
            if (Instance == null) return;
            Instance.amplitude = Mathf.Max(Instance.amplitude, amp);
            Instance.timeLeft = Mathf.Max(Instance.timeLeft, dur);
        }

        private void LateUpdate()
        {
            // 1) Deshacer offset del frame anterior
            if (appliedOffset != Vector3.zero)
            {
                transform.position -= appliedOffset;
                appliedOffset = Vector3.zero;
            }

            // 2) Si queda shake, aplicar un nuevo offset limpio
            if (timeLeft <= 0f) { amplitude = 0f; return; }
            float t = Time.unscaledTime;
            float x = (Mathf.PerlinNoise(t * 20f, 0f) - 0.5f) * amplitude;
            float y = (Mathf.PerlinNoise(0f, t * 20f) - 0.5f) * amplitude;
            appliedOffset = new Vector3(x, y, 0f);
            transform.position += appliedOffset;
            timeLeft -= Time.unscaledDeltaTime;
        }
    }
}
