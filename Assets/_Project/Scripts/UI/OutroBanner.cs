// Shows a center screen banner during LevelOutro ("META"). Fades in and out around the
// state change. Attached to the UI_Root as a child; shown via GameStateManager.

using System.Collections;
using Kikiri9.Session;
using TMPro;
using UnityEngine;

namespace Kikiri9.UI
{
    public class OutroBanner : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text label;

        private bool subscribed;

        private void Start()
        {
            if (root != null) root.SetActive(false);
            StartCoroutine(SubscribeWhenReady());
        }

        private IEnumerator SubscribeWhenReady()
        {
            while (GameStateManager.Instance == null) yield return null;
            GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
            GameStateManager.Instance.OnStateChanged += HandleStateChanged;
            subscribed = true;
        }

        private void OnDestroy()
        {
            if (subscribed && GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState s)
        {
            if (root == null) return;
            if (s == GameState.LevelOutro)
            {
                root.SetActive(true);
                StopAllCoroutines();
                StartCoroutine(FadeInOut());
            }
            else
            {
                root.SetActive(false);
            }
        }

        private IEnumerator FadeInOut()
        {
            if (label == null) yield break;
            label.alpha = 0f;
            // fade in
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.4f) { label.alpha = t; yield return null; }
            label.alpha = 1f;
            // hold
            yield return new WaitForSeconds(1.6f);
            // fade out
            for (float t = 1f; t > 0f; t -= Time.deltaTime / 0.4f) { label.alpha = t; yield return null; }
            label.alpha = 0f;
            root.SetActive(false);
        }
    }
}
