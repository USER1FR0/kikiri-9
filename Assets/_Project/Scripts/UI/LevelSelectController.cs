// Pantalla "Selector de Niveles" (#15). Se construye con LevelSelectBuilder
// (Tools > Kikiri-9 > Build Level Select). Nivel 1 siempre disponible; los demas
// se desbloquean segun SaveService.highestUnlockedLevel. Un nivel se pinta como
// "completado" en cuanto el siguiente ya esta desbloqueado.

using System;
using Kikiri9.Session;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    public class LevelSelectController : MonoBehaviour
    {
        [Serializable]
        public class LevelCard
        {
            public int levelNumber;
            public string sceneName;
            public Button button;
            public Image cardImage;
            public Image badgeImage;
        }

        [SerializeField] private LevelCard[] cards;
        [SerializeField] private Sprite disponibleSprite;
        [SerializeField] private Sprite bloqueadoSprite;
        [SerializeField] private Sprite completadoSprite;
        [SerializeField] private Sprite candadoBadge;
        [SerializeField] private Sprite completadoBadge;
        [SerializeField] private string menuSceneName = "MainMenu";

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (cards == null) return;
            var data = SaveService.Load();

            foreach (var card in cards)
            {
                bool unlocked = card.levelNumber <= data.highestUnlockedLevel;
                bool completed = card.levelNumber < data.highestUnlockedLevel;
                bool sceneExists = !string.IsNullOrEmpty(card.sceneName) && Application.CanStreamedLevelBeLoaded(card.sceneName);

                if (card.cardImage != null)
                    card.cardImage.sprite = !unlocked ? bloqueadoSprite : (completed ? completadoSprite : disponibleSprite);

                if (card.badgeImage != null)
                {
                    card.badgeImage.enabled = !unlocked || completed;
                    card.badgeImage.sprite = !unlocked ? candadoBadge : completadoBadge;
                }

                if (card.button != null)
                    card.button.interactable = unlocked && sceneExists;
            }
        }

        // Llamado por el Button.onClick de cada tarjeta (AddIntPersistentListener en
        // LevelSelectBuilder), pasando el numero de nivel de esa tarjeta.
        public void OnCardClicked(int levelNumber)
        {
            LevelCard card = null;
            foreach (var c in cards)
            {
                if (c.levelNumber == levelNumber) { card = c; break; }
            }

            if (card == null || string.IsNullOrEmpty(card.sceneName)) return;
            if (!Application.CanStreamedLevelBeLoaded(card.sceneName)) return;

            if (SceneFader.Instance != null) SceneFader.Instance.LoadSceneWithFade(card.sceneName);
            else SceneManager.LoadScene(card.sceneName);
        }

        public void OnBackClicked()
        {
            if (SceneFader.Instance != null && Application.CanStreamedLevelBeLoaded(menuSceneName))
                SceneFader.Instance.LoadSceneWithFade(menuSceneName);
            else if (Application.CanStreamedLevelBeLoaded(menuSceneName))
                SceneManager.LoadScene(menuSceneName);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
