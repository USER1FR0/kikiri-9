// Banner de oleada: un canvas overlay con marco sprite + texto TMP.
// No toca el HUD del juego ni sus canvas. Se auto-crea desde WaveManager.

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.Levels.Waves
{
    public class WaveBanner : MonoBehaviour
    {
        public static WaveBanner Instance { get; private set; }

        private Canvas canvas;
        private Image marco;
        private TextMeshProUGUI label;
        private Coroutine currentRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Build();
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Build()
        {
            var cvGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            cvGo.transform.SetParent(transform, false);
            canvas = cvGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 450;
            var scaler = cvGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var marcoGo = new GameObject("Marco", typeof(RectTransform), typeof(Image));
            marcoGo.transform.SetParent(cvGo.transform, false);
            marco = marcoGo.GetComponent<Image>();
            marco.preserveAspect = true;
            marco.raycastTarget = false;
            var rt = (RectTransform)marcoGo.transform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -180f);
            rt.sizeDelta = new Vector2(600f, 140f);

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(marcoGo.transform, false);
            label = textGo.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 56f;
            label.fontStyle = FontStyles.Bold;
            label.color = new Color(1f, 0.85f, 0.25f, 1f);
            var trt = (RectTransform)textGo.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(60f, 25f);
            trt.offsetMax = new Vector2(-60f, -25f);

            canvas.enabled = false;
        }

        public void ShowPersistent(string text, Sprite marcoSprite)
        {
            if (currentRoutine != null) { StopCoroutine(currentRoutine); currentRoutine = null; }
            canvas.enabled = true;
            if (marcoSprite != null) marco.sprite = marcoSprite;
            label.text = text;
        }

        public void ForceHide()
        {
            if (currentRoutine != null) { StopCoroutine(currentRoutine); currentRoutine = null; }
            if (canvas != null) canvas.enabled = false;
        }

        public void Show(string text, Sprite marcoSprite, float visibleTime)
        {
            if (currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(ShowRoutine(text, marcoSprite, visibleTime));
        }

        private IEnumerator ShowRoutine(string text, Sprite marcoSprite, float visibleTime)
        {
            canvas.enabled = true;
            if (marcoSprite != null) marco.sprite = marcoSprite;
            label.text = text;
            yield return new WaitForSeconds(visibleTime);
            canvas.enabled = false;
            currentRoutine = null;
        }
    }
}
