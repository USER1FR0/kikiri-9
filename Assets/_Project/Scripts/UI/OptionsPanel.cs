// Panel de Opciones reutilizable: funciona tanto desde el Menu Principal (un solo
// GameObject que MainMenuController activa/desactiva, OnEnable se encarga de
// refrescar valores) como desde Pausa (patron BuildPanelFrame: contenedor siempre
// activo + "visual" que se muestra/oculta; PausePanel llama a Show() porque ahi
// OnEnable no se dispara). Volumen de musica/SFX via sliders; calidad y "mostrar
// controles" como botones ciclicos, para seguir el mismo lenguaje de botones con
// sprite swap del resto del juego sin introducir dropdowns ni toggles nuevos.

using Kikiri9.Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    public class OptionsPanel : MonoBehaviour
    {
        private static readonly string[] QualityNames = { "Baja", "Media", "Alta" };

        [Header("Solo instancia de Pausa (null en Menu Principal)")]
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject returnToPanel;

        [Header("Controles")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button qualityButton;
        [SerializeField] private TMP_Text qualityLabel;
        [SerializeField] private Button controlsButton;
        [SerializeField] private TMP_Text controlsLabel;

        private bool listenersWired;

        private void Awake()
        {
            if (root != null) root.SetActive(false);
        }

        // Se dispara solo cuando este GameObject en si se activa/desactiva (caso Menu
        // Principal, donde el componente vive en el mismo objeto que se togglea).
        private void OnEnable()
        {
            if (!listenersWired) WireListeners();
            RefreshFromCurrentValues();
        }

        // Caso Pausa: el componente vive en un contenedor siempre activo, asi que
        // PausePanel llama esto explicitamente en vez de depender de OnEnable.
        public void Show()
        {
            if (root != null) root.SetActive(true);
            if (!listenersWired) WireListeners();
            RefreshFromCurrentValues();
        }

        private void WireListeners()
        {
            listenersWired = true;
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(GameOptions.SetMusicVolume);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(GameOptions.SetSfxVolume);
        }

        private void RefreshFromCurrentValues()
        {
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(GameOptions.MusicVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(GameOptions.SfxVolume);
            RefreshQualityLabel();
            RefreshControlsLabel();
        }

        public void OnQualityClicked()
        {
            GameOptions.SetQualityLevel((GameOptions.QualityLevel + 1) % 3);
            RefreshQualityLabel();
        }

        public void OnControlsClicked()
        {
            GameOptions.SetShowControls(!GameOptions.ShowControls);
            RefreshControlsLabel();
        }

        private void RefreshQualityLabel()
        {
            if (qualityLabel != null) qualityLabel.text = $"Calidad: {QualityNames[GameOptions.QualityLevel]}";
        }

        private void RefreshControlsLabel()
        {
            if (controlsLabel != null) controlsLabel.text = GameOptions.ShowControls ? "Controles: Si" : "Controles: No";
        }

        // Solo Pausa (boton "Cerrar" dedicado). El Menu Principal usa su propio boton
        // "Volver" ya existente (MainMenuController.OnClosePanelPressed).
        public void OnCloseClicked()
        {
            if (root != null) root.SetActive(false);
            if (returnToPanel != null) returnToPanel.SetActive(true);
        }
    }

    // _reimport_force: touch para que Unity refresque el cache de ScriptAssemblies
}
