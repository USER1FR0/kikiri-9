// Componente adaptativo del PDF kikiri9_fondos_adaptativos.
// Escala y centra las 3 capas (cielo, horizonte, fachada) para que llenen siempre el
// viewport de la camara sin mostrar bordes. Una sola instancia por capa, cero wrap.

using UnityEngine;

namespace Kikiri9.World
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1000)]
    public sealed class FondoAdaptativoKikiri9 : MonoBehaviour
    {
        [Header("Una copia de cada capa. Sprites Single, Full Rect, Pivot Center.")]
        [SerializeField] private Camera camara;
        [SerializeField] private Transform lineaSuelo;
        [SerializeField] private SpriteRenderer cielo;
        [SerializeField] private SpriteRenderer horizonte;
        [SerializeField] private SpriteRenderer fachada;

        [Header("Encuadre")]
        [Tooltip("0 toma como altura base la altura visible de la camara al iniciar.")]
        [SerializeField, Min(0f)] private float alturaArte = 0f;
        [Tooltip("Altura de la linea de suelo dentro del lienzo, medida desde abajo.")]
        [SerializeField, Range(0f, 1f)] private float sueloDesdeAbajo = 0.125f;

        [Header("Desplazamiento opcional a lo largo del nivel")]
        [SerializeField, Range(0f, 0.15f)] private float paneoHorizonte = 0.025f;
        [SerializeField, Range(0f, 0.15f)] private float paneoFachada = 0.055f;

        private float alturaInicial;
        private float progreso = 0.5f;

        private void OnEnable() { alturaInicial = 0f; }

        public void SetProgress01(float avance)
        {
            if (!float.IsNaN(avance))
                progreso = Mathf.Clamp01(avance);
        }

        private void LateUpdate()
        {
            if (camara == null || !camara.orthographic || lineaSuelo == null ||
                !Valido(cielo) || !Valido(horizonte) || !Valido(fachada) ||
                cielo == horizonte || cielo == fachada || horizonte == fachada)
                return;

            float alto = 2f * camara.orthographicSize;
            float ancho = alto * camara.aspect;
            if (alto <= 0f || ancho <= 0f) return;
            if (alturaInicial <= 0f) alturaInicial = alto;

            Vector3 centro = camara.transform.position;
            float altoArteBase = alturaArte > 0f ? alturaArte : alturaInicial;
            float formatoArte = Mathf.Min(Relacion(horizonte), Relacion(fachada));
            float altoArquitectura = Mathf.Max(altoArteBase, ancho * 1.002f / formatoArte);
            float altoCielo = Mathf.Max(alto, ancho / Relacion(cielo)) * 1.002f;

            Colocar(cielo, altoCielo, centro.x, centro.y);
            float yArquitectura = lineaSuelo.position.y + (0.5f - sueloDesdeAbajo) * altoArquitectura;
            float direccion = 1f - 2f * progreso;
            ColocarArquitectura(horizonte, altoArquitectura, centro.x, yArquitectura, ancho, paneoHorizonte, direccion);
            ColocarArquitectura(fachada, altoArquitectura, centro.x, yArquitectura, ancho, paneoFachada, direccion);
        }

        private static bool Valido(SpriteRenderer capa)
        {
            return capa != null && capa.sprite != null &&
                capa.sprite.bounds.size.x > 0f && capa.sprite.bounds.size.y > 0f;
        }

        private static float Relacion(SpriteRenderer capa)
        {
            Vector3 medida = capa.sprite.bounds.size;
            return medida.x / medida.y;
        }

        private static void ColocarArquitectura(SpriteRenderer capa, float alto, float x, float y,
            float anchoVista, float paneo, float direccion)
        {
            float anchoArte = alto * Relacion(capa);
            float margen = Mathf.Max(0f, (anchoArte - anchoVista * 1.002f) * 0.5f);
            float desplazamiento = Mathf.Min(anchoVista * paneo, margen) * direccion;
            Colocar(capa, alto, x + desplazamiento, y);
        }

        private static void Colocar(SpriteRenderer capa, float alto, float x, float y)
        {
            Transform t = capa.transform;
            float escala = alto / capa.sprite.bounds.size.y;
            t.localScale = new Vector3(escala, escala, 1f);
            t.position = new Vector3(x, y, t.position.z);
        }
    }
}
