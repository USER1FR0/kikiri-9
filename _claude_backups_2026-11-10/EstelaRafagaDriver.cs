// Enciende la estela de rafaga como hijo del pollo mientras dura la animacion de
// ataque, apagandola despues. Lee el estado directamente del PlayerPunch.

using System.Collections;
using UnityEngine;

namespace Kikiri9.Player
{
    public class EstelaRafagaDriver : MonoBehaviour
    {
        [SerializeField] private GameObject estela;
        [SerializeField] private float duracion = 0.45f;

        private void Start()
        {
            if (estela != null) estela.SetActive(false);
        }

        public void ShowForDuration()
        {
            StartCoroutine(Show());
        }

        private IEnumerator Show()
        {
            if (estela == null) yield break;
            estela.SetActive(true);
            yield return new WaitForSeconds(duracion);
            estela.SetActive(false);
        }
    }
}
