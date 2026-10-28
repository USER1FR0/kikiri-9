using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Kikiri9.Session;

namespace Kikiri9.UI
{
    // Logica de los 5 botones del menu principal (Jugar, Continuar, Opciones,
    // Creditos, Salir). Los paneles de Opciones y Creditos son simples
    // GameObjects que se activan/desactivan, sin logica propia.
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject continueButton;
        [SerializeField] private GameObject exitMessagePanel;
        [SerializeField] private string defaultLevelSceneName = "Level01_Dolores";
        [SerializeField] private GameObject firstSelectedButton;

        private void Start()
        {
            CloseAllPanels();
            RefreshContinueButton();
        }

        private void Update()
        {
            // Si por cualquier motivo no hay ningun boton seleccionado (al iniciar,
            // o despues de cerrar un panel y que su boton "Volver" se desactive junto
            // con el), volvemos a seleccionar JUGAR para que el teclado/gamepad
            // siempre tengan desde donde navegar.
            if (EventSystem.current != null
                && EventSystem.current.currentSelectedGameObject == null
                && firstSelectedButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            }
        }

        private void RefreshContinueButton()
        {
            if (continueButton == null)
            {
                return;
            }

            bool hasSavedProgress = GameSession.Instance != null && GameSession.Instance.HasSavedProgress;

            // Lo dejamos activo (no SetActive(false)) para que siga formando parte de
            // la cadena de navegacion por teclado/gamepad; solo cambia su estado visual
            // a "desactivado" (sprite botones_04) y deja de responder a clic/submit.
            var continueButtonComponent = continueButton.GetComponent<Button>();
            if (continueButtonComponent != null)
            {
                continueButtonComponent.interactable = hasSavedProgress;
            }
        }

        public void OnPlayPressed()
        {
            string sceneToLoad = defaultLevelSceneName;

            if (GameSession.Instance != null)
            {
                GameSession.Instance.ResetSession();
                GameSession.Instance.SetCurrentLevel(sceneToLoad);
            }

            LoadScene(sceneToLoad);
        }

        public void OnContinuePressed()
        {
            string sceneToLoad = defaultLevelSceneName;

            if (GameSession.Instance != null && GameSession.Instance.HasSavedProgress)
            {
                sceneToLoad = GameSession.Instance.CurrentLevelSceneName;
            }

            LoadScene(sceneToLoad);
        }

        public void OnOptionsPressed()
        {
            CloseAllPanels();

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(true);
            }
        }

        public void OnCreditsPressed()
        {
            CloseAllPanels();

            if (creditsPanel != null)
            {
                creditsPanel.SetActive(true);
            }
        }

        public void OnClosePanelPressed()
        {
            CloseAllPanels();
        }

        public void OnExitPressed()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            CloseAllPanels();

            if (exitMessagePanel != null)
            {
                exitMessagePanel.SetActive(true);
            }
#elif UNITY_EDITOR
            // En el editor Application.Quit() no hace nada; detenemos el modo Play
            // para poder probar el boton sin tener que hacer un build.
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void LoadScene(string sceneName)
        {
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.LoadSceneWithFade(sceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }

        private void CloseAllPanels()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            if (creditsPanel != null)
            {
                creditsPanel.SetActive(false);
            }

            if (exitMessagePanel != null)
            {
                exitMessagePanel.SetActive(false);
            }
        }
    }
}
