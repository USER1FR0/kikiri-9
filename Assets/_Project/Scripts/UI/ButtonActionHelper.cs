// Button helper: dispatches a method call to a target GameObject on click. Auto-wires
// its handler to the Button component at Awake. Method is named TriggerAction (not
// Invoke) so it does not collide with MonoBehaviour.Invoke(string, float).

using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonActionHelper : MonoBehaviour
    {
        public GameObject target;
        public string action;

        private void Awake()
        {
            var btn = GetComponent<Button>();
            btn.onClick.RemoveListener(TriggerAction);
            btn.onClick.AddListener(TriggerAction);
        }

        public void TriggerAction()
        {
            if (target == null || string.IsNullOrEmpty(action)) return;
            target.SendMessage(action, SendMessageOptions.DontRequireReceiver);
        }
    }
}
