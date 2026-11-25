// Enciende la estela de rafaga como hijo del pollo mientras dura la animacion de
// ataque. Si no se asigno desde inspector, busca un hijo llamado "Estela" al despertar.

using System.Collections;
using UnityEngine;

namespace Kikiri9.Player
{
    public class EstelaRafagaDriver : MonoBehaviour
    {
        [SerializeField] private GameObject estela;
        [SerializeField] private float duracion = 0.45f;

        private void Awake()
        {
            if (estela == null)
            {
                var t = transform.Find("Estela");
                if (t == null) t = transform.Find("estela");
                if (t == null) t = transform.Find("EstelaRafaga");
                if (t != null) estela = t.gameObject;
            }
        }

        private void Start()
        {
            if (estela != null) estela.SetActive(false);
        }

        public void ShowForDuration()
        {
            if (estela == null) return;
            StopAllCoroutines();
            StartCoroutine(Show());
        }

        private IEnumerator Show()
        {
            estela.SetActive(true);
            yield return new WaitForSeconds(duracion);
            if (estela != null) estela.SetActive(false);
        }
    }
}
