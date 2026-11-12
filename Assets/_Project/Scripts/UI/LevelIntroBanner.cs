// Muestra "NIVEL X - <nombre>" al iniciar la escena. Fade in rapido, 1.5s visible,
// fade out. No bloquea input. Se destruye al terminar.

using System.Collections;
using Kikiri9.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    public class LevelIntroBanner : MonoBehaviour
    {
        [SerializeField] private string levelNumberLabel = "NIVEL 2";
        [SerializeField] private float holdSeconds = 1.5f;
        [SerializeField] private float fadeSeconds = 0.4f;

        public static void SpawnFor(string number, LevelDefinition def)
        {
            var root = new GameObject("LevelIntroBanner", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var bannerGo = new GameObject("Banner", typeof(RectTransform), typeof(CanvasGroup));
            bannerGo.transform.SetParent(root.transform, false);
            var rt = bannerGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.6f);
            rt.anchorMax = new Vector2(1f, 0.85f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var cg = bannerGo.GetComponent<CanvasGroup>();
            cg.alpha = 0f;

            // Fondo semitransparente
            var bgGo = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(bannerGo.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            bgGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            var numberGo = new GameObject("Number", typeof(RectTransform), typeof(TextMeshProUGUI));
            numberGo.transform.SetParent(bannerGo.transform, false);
            var numRt = numberGo.GetComponent<RectTransform>();
            numRt.anchorMin = new Vector2(0f, 0.5f); numRt.anchorMax = new Vector2(1f, 1f);
            numRt.offsetMin = Vector2.zero; numRt.offsetMax = Vector2.zero;
            var numText = numberGo.GetComponent<TextMeshProUGUI>();
            numText.text = number;
            numText.alignment = TextAlignmentOptions.Center;
            numText.fontSize = 90;
            numText.color = new Color(1f, 0.85f, 0.4f);
            numText.fontStyle = FontStyles.Bold;

            var nameGo = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGo.transform.SetParent(bannerGo.transform, false);
            var nRt = nameGo.GetComponent<RectTransform>();
            nRt.anchorMin = new Vector2(0f, 0f); nRt.anchorMax = new Vector2(1f, 0.5f);
            nRt.offsetMin = Vector2.zero; nRt.offsetMax = Vector2.zero;
            var nText = nameGo.GetComponent<TextMeshProUGUI>();
            nText.text = def != null ? def.levelName.ToUpper() : "";
            nText.alignment = TextAlignmentOptions.Center;
            nText.fontSize = 48;
            nText.color = Color.white;

            var runner = root.AddComponent<LevelIntroBanner>();
            runner.levelNumberLabel = number;
            runner.StartCoroutine(runner.Run(cg));
        }

        private IEnumerator Run(CanvasGroup cg)
        {
            float t = 0f;
            while (t < fadeSeconds) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Clamp01(t / fadeSeconds); yield return null; }
            yield return new WaitForSecondsRealtime(holdSeconds);
            t = 0f;
            while (t < fadeSeconds) { t += Time.unscaledDeltaTime; cg.alpha = 1f - Mathf.Clamp01(t / fadeSeconds); yield return null; }
            Destroy(gameObject);
        }
    }
}
