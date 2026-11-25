// Al disparar OnDeath hace zoom lento sobre el pollo + timeScale lento + fade negro.
// GameOverHandler sigue mostrando el panel; esto solo es la transicion visual.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.Session.FX
{
    public class DeathFeedback : MonoBehaviour
    {
        [SerializeField] private float zoomDuration = 5.0f;
        [SerializeField] private float targetOrthoSize = 2.5f;
        [SerializeField] private float finalTimeScale = 0.4f;

        public void PlayDeath(Transform player, System.Action onComplete)
        {
            StartCoroutine(Run(player, onComplete));
        }

        private IEnumerator Run(Transform player, System.Action onComplete)
        {
            var cam = Camera.main;
            float startSize = cam != null ? cam.orthographicSize : 5f;
            Vector3 startPos = cam != null ? cam.transform.position : Vector3.zero;
            Vector3 targetPos = player != null
                ? new Vector3(player.position.x, player.position.y, startPos.z)
                : startPos;

            var canvasGo = new GameObject("DeathFadeCanvas", typeof(Canvas), typeof(CanvasScaler));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            var img = new GameObject("Fade", typeof(RectTransform), typeof(Image));
            img.transform.SetParent(canvasGo.transform, false);
            var rt = img.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var fade = img.GetComponent<Image>();
            fade.color = new Color(0f, 0f, 0f, 0f);
            fade.raycastTarget = false;

            float t = 0f;
            Time.timeScale = finalTimeScale;
            while (t < zoomDuration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / zoomDuration);
                if (cam != null)
                {
                    cam.orthographicSize = Mathf.Lerp(startSize, targetOrthoSize, u);
                    // Seguir al pollo mientras cae (recalcula targetPos cada frame)
                    Vector3 chasePos = player != null
                        ? new Vector3(player.position.x, player.position.y, startPos.z)
                        : targetPos;
                    cam.transform.position = Vector3.Lerp(startPos, chasePos, u);
                }
                fade.color = new Color(0f, 0f, 0f, u * 0.7f);
                yield return null;
            }
            Time.timeScale = 1f;
            // Quitar fade antes de invocar onComplete, para que el GameOver panel se vea limpio
            Destroy(canvasGo);
            // Restaurar ortho size y pos de la camara
            if (cam != null)
            {
                cam.orthographicSize = startSize;
                cam.transform.position = startPos;
            }
            onComplete?.Invoke();
        }
    }
}
