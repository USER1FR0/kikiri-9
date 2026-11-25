using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.Visuales
{
    // El temporizador pertenece al juego; este componente no concede munición.
    [DisallowMultipleComponent]
    public sealed class KikiriPanDoradoVisual : MonoBehaviour
    {
        [SerializeField] private GameObject indicador;
        [SerializeField] private Image relleno;
        [SerializeField, Min(0f)] private float avisoFinal = 3f;
        private float restante;
        private Color colorBase = new Color(1f, 0.79f, 0.16f, 1f);

        private void Awake()
        {
            if (relleno != null)
            {
                colorBase = relleno.color;
                relleno.type = Image.Type.Filled;
                relleno.fillMethod = Image.FillMethod.Horizontal;
                relleno.fillOrigin = 0; // Izquierda.
                relleno.raycastTarget = false;
            }
            ActualizarDuracion(0f, 10f);
        }

        public void ActualizarDuracion(float segundosRestantes, float segundosTotales)
        {
            float total = float.IsNaN(segundosTotales) || float.IsInfinity(segundosTotales)
                ? 0f : Mathf.Max(0f, segundosTotales);
            restante = float.IsNaN(segundosRestantes) || float.IsInfinity(segundosRestantes)
                ? 0f : Mathf.Max(0f, segundosRestantes);
            bool activo = restante > 0f && total > 0f;
            if (indicador != null && indicador != gameObject && indicador.activeSelf != activo)
                indicador.SetActive(activo);
            if (relleno != null)
            {
                relleno.fillAmount = activo ? Mathf.Clamp01(restante / total) : 0f;
                relleno.color = colorBase;
            }
        }

        private void Update()
        {
            if (relleno == null || restante <= 0f || restante > avisoFinal) return;
            Color c = colorBase;
            // Pulso visual suave. Time.time se detiene si el juego pausa con timeScale = 0.
            c.a *= 0.65f + 0.35f * (0.5f + 0.5f * Mathf.Sin(Time.time * 7f));
            relleno.color = c;
        }

        private void OnDisable()
        {
            if (relleno != null) relleno.color = colorBase;
        }
    }
}
