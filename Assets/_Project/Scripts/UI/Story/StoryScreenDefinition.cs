// ScriptableObject que define una pantalla narrativa. Soporta texto paginado:
// - "pages" (string[]) son las paginas de la MISMA imagen de fondo.
// - "textEs" (legacy, string) se usa como pagina unica si "pages" esta vacio.

using UnityEngine;

namespace Kikiri9.UI.Story
{
    [CreateAssetMenu(fileName = "StoryScreen", menuName = "Kikiri-9/Story/Screen")]
    public class StoryScreenDefinition : ScriptableObject
    {
        [Header("Fondo")]
        public Sprite background;

        [Header("Texto narrativo (paginas)")]
        [Tooltip("Cada entrada es una pagina de la MISMA pantalla. Usa varias cuando el texto no cabe en el contenedor.")]
        [TextArea(3, 8)] public string[] pages;

        [Tooltip("Legacy: usada como pagina unica si 'pages' esta vacio.")]
        [TextArea(3, 8)] public string textEs;

        [Header("Tiempo")]
        public float fadeInSeconds = 0.5f;
        public float fadeOutSeconds = 0.35f;

        [Header("Overlays animados (opcional)")]
        public StoryEffect[] effects;

        public string[] GetPages()
        {
            if (pages != null && pages.Length > 0) return pages;
            if (!string.IsNullOrEmpty(textEs)) return new[] { textEs };
            return new[] { "" };
        }
    }

    [System.Serializable]
    public class StoryEffect
    {
        public string id;
        public Sprite[] frames;
        public float frameRate = 6f;
        public bool loop = true;
        public Vector2 anchor = new Vector2(0.5f, 0.5f);
        public Vector2 size = new Vector2(300f, 300f);
        public Color tint = Color.white;
        public float startDelay = 0f;
        [Range(0f, 10f)] public float randomRepeatInterval = 0f;
    }
}
