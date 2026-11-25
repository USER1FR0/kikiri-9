using UnityEngine;

namespace Kikiri9.Visuales
{
    // Para el halo como objeto hijo. No modifica el padre ni los colliders.
    [DisallowMultipleComponent]
    public sealed class KikiriPulsoSuave : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer imagen;
        [SerializeField, Range(0f, 0.15f)] private float amplitud = 0.04f;
        [SerializeField, Min(0f)] private float ciclosPorSegundo = 0.65f;
        [SerializeField, Range(0f, 1f)] private float alfaMinima = 0.65f;
        private Vector3 escalaBase;
        private Color colorBase;
        private float inicio;

        private void OnEnable()
        {
            escalaBase = transform.localScale;
            if (imagen != null) colorBase = imagen.color;
            inicio = Time.time;
        }

        private void Update()
        {
            float onda = Mathf.Sin((Time.time - inicio) * ciclosPorSegundo * 2f * Mathf.PI);
            transform.localScale = escalaBase * (1f + amplitud * onda);
            if (imagen != null)
            {
                Color c = colorBase;
                c.a *= Mathf.Lerp(alfaMinima, 1f, 0.5f + 0.5f * onda);
                imagen.color = c;
            }
        }

        private void OnDisable()
        {
            transform.localScale = escalaBase;
            if (imagen != null) imagen.color = colorBase;
        }
    }
}
