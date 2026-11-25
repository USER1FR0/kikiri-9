using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Kikiri9.Visuales
{
    // Solo presentación: recibe la recarga real de tu controlador de habilidad.
    [DisallowMultipleComponent]
    public sealed class KikiriRafagaVisual : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private Button boton;
        [SerializeField] private Image imagenBoton;
        [SerializeField] private Image mascaraRecarga;
        [SerializeField] private Sprite listo;
        [SerializeField] private Sprite pulsado;
        private bool disponible = true;
        private bool presionado;

        private void Awake()
        {
            if (mascaraRecarga != null)
            {
                mascaraRecarga.type = Image.Type.Filled;
                mascaraRecarga.fillMethod = Image.FillMethod.Radial360;
                mascaraRecarga.fillOrigin = 2; // Arriba.
                mascaraRecarga.fillClockwise = true;
                mascaraRecarga.raycastTarget = false;
                mascaraRecarga.color = new Color(0.078f, 0.047f, 0.149f, 0.85f);
            }
            ActualizarRecarga(0f, 1f);
        }

        public void ActualizarRecarga(float segundosRestantes, float segundosTotales)
        {
            float restante = float.IsNaN(segundosRestantes) || float.IsInfinity(segundosRestantes)
                ? 0f : Mathf.Max(0f, segundosRestantes);
            float total = float.IsNaN(segundosTotales) || float.IsInfinity(segundosTotales)
                ? 0f : Mathf.Max(0f, segundosTotales);
            disponible = restante <= 0f;
            if (!disponible) presionado = false;
            if (mascaraRecarga != null)
                mascaraRecarga.fillAmount = disponible ? 0f : total > 0f ? Mathf.Clamp01(restante / total) : 1f;
            if (boton != null) boton.interactable = disponible;
            Dibujar();
        }

        // También se puede llamar desde los eventos existentes de la tecla K.
        public void MostrarPulsado(bool valor)
        {
            presionado = disponible && valor;
            Dibujar();
        }

        private void Dibujar()
        {
            if (imagenBoton == null) return;
            imagenBoton.sprite = presionado && pulsado != null ? pulsado : listo;
            imagenBoton.color = disponible ? Color.white : new Color(0.68f, 0.68f, 0.68f, 1f);
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left) MostrarPulsado(true);
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left) MostrarPulsado(false);
        }

        public void OnPointerExit(PointerEventData data) { MostrarPulsado(false); }
        private void OnDisable() { presionado = false; Dibujar(); }
    }
}
