using System.Collections;
using UnityEngine;

namespace Kikiri9.UI
{
    // Controla la escena Boot: muestra el splash/logo un tiempo fijo y despues
    // pide el fundido hacia MainMenu. Si no hay SceneFader en la escena, carga
    // la escena directo sin fundido.
    public class BootSplash : MonoBehaviour
    {
        [SerializeField] private float displayDuration = 1.5f;
        [SerializeField] private string nextSceneName = "MainMenu";

        private void Start()
        {
            StartCoroutine(WaitAndContinue());
        }

        private IEnumerator WaitAndContinue()
        {
            yield return new WaitForSecondsRealtime(displayDuration);

            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.LoadSceneWithFade(nextSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
