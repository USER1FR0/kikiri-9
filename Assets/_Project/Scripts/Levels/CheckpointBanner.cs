// Dispara un banner una sola vez al cruzar un metro objetivo. Reutiliza WaveBanner.

using Kikiri9.Levels.Waves;
using Kikiri9.World;
using UnityEngine;

namespace Kikiri9.Levels
{
    public class CheckpointBanner : MonoBehaviour
    {
        [SerializeField] private float triggerMeters = 1000f;
        [SerializeField, TextArea] private string message = "EL CENTRO ESTA INUNDADO\nSOBREVIVE";
        [SerializeField] private float visibleSeconds = 2.5f;
        [SerializeField] private Sprite bannerSprite;

        private bool shown;

        private void Update()
        {
            if (shown) return;
            if (WorldScroller.Instance == null) return;
            if (WorldScroller.Instance.MetersAdvanced < triggerMeters) return;
            shown = true;

            if (WaveBanner.Instance == null)
            {
                var go = new GameObject("WaveBanner");
                go.AddComponent<WaveBanner>();
            }
            WaveBanner.Instance?.Show(message, bannerSprite, visibleSeconds);
        }
    }
}
