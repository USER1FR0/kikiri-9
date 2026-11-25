// Escena de creditos: scroll suave sobre el fondo 08_amanecer_final. Al terminar
// el scroll (cuando el contenido salio del viewport), se detiene y aparece un
// mensaje final "GRACIAS POR JUGAR" para no dejar el panel vacio.

using System.Collections;
using Kikiri9.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kikiri9.UI.Story
{
    public class CreditsSceneController : MonoBehaviour
    {
        [SerializeField] private RectTransform scrollContent;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button backButton;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private TMP_Text endMessage;
        [SerializeField] private string menuSceneName = "MainMenu";
        [SerializeField] private float scrollSpeed = 40f;
        [SerializeField] private float initialDelay = 1.0f;

        private bool scrolling;

        private void Start()
        {
            if (backButton != null) backButton.onClick.AddListener(OnBack);
            if (bodyText != null) bodyText.text = BuildText();
            if (endMessage != null)
            {
                endMessage.text = "GRACIAS POR JUGAR";
                endMessage.gameObject.SetActive(false);
            }
            StartCoroutine(AutoScroll());
        }

        private string BuildText()
        {
            return
                "KIKIRI-9: EL ULTIMO PAN\n\n" +
                "Un juego de Juan Francisco Rodriguez Guerrero\n" +
                "y Manuel Mata Campos.\n\n" +
                "Grupo GIDS6091 - UTNG\n" +
                "Instructor: Rodriguez Guerrero Juan Francisco\n\n" +
                "- MOTOR -\n" +
                "Unity 6.3 LTS\n\n" +
                "- ARTE -\n" +
                "Ilustraciones generadas con OpenAI Image Gen\n" +
                "Pixel art original y composicion por el equipo\n\n" +
                "- AUDIO -\n" +
                "Sintesis y ambiente original\n\n" +
                "- CODIGO -\n" +
                "Jugador, enemigos, niveles 3-4 e integracion PC: JFRG\n" +
                "UI, HUD, menus, audio, guardado, niveles 1-2: Manuel Mata Campos\n\n" +
                "- INSPIRACION -\n" +
                "Dia de Muertos\n" +
                "Dolores Hidalgo y Guanajuato capital";
        }

        private IEnumerator AutoScroll()
        {
            yield return new WaitForSecondsRealtime(initialDelay);
            scrolling = true;
            while (scrolling && scrollContent != null)
            {
                scrollContent.anchoredPosition += new Vector2(0f, scrollSpeed * Time.unscaledDeltaTime);

                // Detectar fin: cuando la base del contenido sale por arriba del viewport
                if (viewport != null)
                {
                    float contentBottomY = scrollContent.anchoredPosition.y; // pivot (0.5,0)
                    float viewportHeight = viewport.rect.height;
                    if (contentBottomY >= viewportHeight + 40f)
                    {
                        scrolling = false;
                        if (endMessage != null) endMessage.gameObject.SetActive(true);
                        yield break;
                    }
                }
                yield return null;
            }
        }

        public void OnBack()
        {
            scrolling = false;
            if (SceneFader.Instance != null && Application.CanStreamedLevelBeLoaded(menuSceneName))
                SceneFader.Instance.LoadSceneWithFade(menuSceneName);
            else if (Application.CanStreamedLevelBeLoaded(menuSceneName))
                SceneManager.LoadScene(menuSceneName);
        }
    }
}
