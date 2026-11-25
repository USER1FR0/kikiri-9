// Conecta el Button de rafaga al OnPunch del PlayerPunch via un pseudo-click publico.
// Asi el usuario puede activarla con touch/click aparte de la tecla K.

using Kikiri9.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    [RequireComponent(typeof(Button))]
    public class RafagaButtonHook : MonoBehaviour
    {
        private PlayerPunch punch;

        private void Start()
        {
            var btn = GetComponent<Button>();
            btn.onClick.AddListener(TryFireRafaga);
        }

        public void TryFireRafaga()
        {
            if (punch == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) punch = p.GetComponent<PlayerPunch>();
            }
            if (punch != null) punch.FireFromExternal();
        }
    }
}
