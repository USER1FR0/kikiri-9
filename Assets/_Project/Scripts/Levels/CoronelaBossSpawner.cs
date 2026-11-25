// Spawnea a la Coronela (jefe final, #14) una vez cuando MetersAdvanced >= triggerMeters.
// Muestra un aviso breve (0.7s) antes de instanciarla, igual que pide el issue. Garantiza
// que haya un BossArenaManager en la escena antes del spawn (igual que MiniBossSpawner).

using System.Collections;
using Kikiri9.Session;
using Kikiri9.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.Levels
{
    public class CoronelaBossSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private float triggerMeters = 2000f;
        [SerializeField] private float spawnAheadDistance = 10f;
        [SerializeField] private float spawnY = 0f;
        [SerializeField] private float warningDuration = 0.7f;
        [SerializeField] private string warningText = "LA CORONELA SE ACERCA...";

        private bool triggered;

        private void Awake()
        {
            if (FindFirstObjectByType<BossArenaManager>() == null)
            {
                var go = new GameObject("BossArenaManager");
                go.AddComponent<BossArenaManager>();
            }
        }

        private void Update()
        {
            if (triggered || bossPrefab == null || WorldScroller.Instance == null) return;
            if (WorldScroller.Instance.MetersAdvanced < triggerMeters) return;
            triggered = true;
            StartCoroutine(WarningThenSpawn());
        }

        private IEnumerator WarningThenSpawn()
        {
            var banner = ShowWarning();
            yield return new WaitForSeconds(warningDuration);
            if (banner != null) Destroy(banner);

            var cam = Camera.main;
            if (cam == null) yield break;
            var pos = new Vector3(cam.transform.position.x + spawnAheadDistance, spawnY, 0f);
            Instantiate(bossPrefab, pos, Quaternion.identity);
        }

        private GameObject ShowWarning()
        {
            var root = new GameObject("CoronelaWarning", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(root.transform, false);
            var rect = (RectTransform)textGo.transform;
            rect.anchorMin = new Vector2(0f, 0.65f);
            rect.anchorMax = new Vector2(1f, 0.8f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.text = warningText;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 64;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color(1f, 0.3f, 0.3f);

            return root;
        }
    }
}
