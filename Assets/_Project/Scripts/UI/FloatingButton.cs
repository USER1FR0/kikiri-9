using UnityEngine;

namespace Kikiri9.UI
{
    // Hace flotar suavemente un elemento de UI (los botones del menu principal)
    // con un movimiento senoidal en Y, para dar sensacion de menu "vivo". No
    // reemplaza la posicion configurada en el editor: la captura en Awake como
    // base y solo le suma un desplazamiento temporal cada frame.
    [RequireComponent(typeof(RectTransform))]
    public class FloatingButton : MonoBehaviour
    {
        [SerializeField] private float amplitude = 6f;
        [SerializeField] private float speed = 1.2f;
        [SerializeField] private float phaseOffset = 0f;

        private RectTransform rectTransform;
        private Vector2 basePosition;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            basePosition = rectTransform.anchoredPosition;
        }

        private void Update()
        {
            float offsetY = Mathf.Sin((Time.unscaledTime * speed) + phaseOffset) * amplitude;
            rectTransform.anchoredPosition = basePosition + new Vector2(0f, offsetY);
        }
    }
}
