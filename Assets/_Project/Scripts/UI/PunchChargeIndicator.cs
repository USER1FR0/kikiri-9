// HUD charger para el puñetazo. Barra circular en esquina inferior-izquierda que
// se rellena durante el cooldown. Verde-dorado cuando esta listo, oscura mientras carga.
// Reemplazable facilmente por un sprite de cargador cuando el user mande el asset.

using Kikiri9.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    public class PunchChargeIndicator : MonoBehaviour
    {
        private static PunchChargeIndicator current;
        private PlayerPunch source;
        private Image fill;
        private Image bg;
        private TextMeshProUGUI label;

        private void OnDestroy()
        {
            if (current == this) current = null;
        }

        public static void Install()
        {
            if (current != null) return;
            var go = new GameObject("PunchChargeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Contenedor esquina inferior-izquierda
            var holderGo = new GameObject("PunchHolder", typeof(RectTransform));
            holderGo.transform.SetParent(go.transform, false);
            var holderRt = holderGo.GetComponent<RectTransform>();
            holderRt.anchorMin = new Vector2(0f, 0f);
            holderRt.anchorMax = new Vector2(0f, 0f);
            holderRt.pivot = new Vector2(0f, 0f);
            holderRt.anchoredPosition = new Vector2(140f, 90f);
            holderRt.sizeDelta = new Vector2(130f, 130f);

            // BG oscuro
            var bgGo = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(holderGo.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.color = new Color(0.08f, 0.05f, 0.1f, 0.9f);

            // Fill (circular)
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(holderGo.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(8f, 8f);
            fillRt.offsetMax = new Vector2(-8f, -8f);
            var fillImg = fillGo.GetComponent<Image>();
            fillImg.color = new Color(1f, 0.55f, 0.15f, 1f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Radial360;
            fillImg.fillOrigin = (int)Image.Origin360.Top;
            fillImg.fillClockwise = true;
            fillImg.fillAmount = 0f;

            // Label "K" / "RAFAGA"
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(holderGo.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var labelText = labelGo.GetComponent<TextMeshProUGUI>();
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 28;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = Color.white;
            labelText.text = "K";

            var ind = go.AddComponent<PunchChargeIndicator>();
            ind.bg = bgImg;
            ind.fill = fillImg;
            ind.label = labelText;
            current = ind;
        }

        private void LateUpdate()
        {
            if (source == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) source = player.GetComponent<PlayerPunch>();
            }
            if (source == null) return;

            bool ready = source.Ready;
            fill.fillAmount = source.Progress01;
            fill.color = ready ? new Color(1f, 0.85f, 0.25f, 1f) : new Color(1f, 0.55f, 0.15f, 1f);

            if (ready) label.text = "K";
            else label.text = Mathf.CeilToInt(source.Remaining).ToString();
        }
    }
}
