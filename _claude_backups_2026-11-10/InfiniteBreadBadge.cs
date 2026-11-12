// Indicador world-space sobre el pollo durante el buff de pan infinito.
// El sprite del pan y de las partículas se inyecta desde fuera para no depender de
// UnityEditor en runtime.

using Kikiri9.Player;
using TMPro;
using UnityEngine;

namespace Kikiri9.UI
{
    public class InfiniteBreadBadge : MonoBehaviour
    {
        private static InfiniteBreadBadge current;
        private static Sprite pendingIcon;

        private InfiniteBreadBuff buff;
        private SpriteRenderer iconSr;
        private TextMeshPro timerText;
        private Transform anchor;

        private void Awake()
        {
            InfiniteBreadBuff.OnActivated += HandleActivated;
            InfiniteBreadBuff.OnExpired += HandleExpired;
        }

        private void OnDestroy()
        {
            InfiniteBreadBuff.OnActivated -= HandleActivated;
            InfiniteBreadBuff.OnExpired -= HandleExpired;
            if (current == this) current = null;
        }

        public static void Install(Sprite icon)
        {
            pendingIcon = icon;
            EnsureInstance();
        }

        private static void EnsureInstance()
        {
            if (current != null) return;
            var go = new GameObject("InfiniteBreadBadge");
            var badge = go.AddComponent<InfiniteBreadBadge>();
            current = badge;
            badge.BuildVisual();
            badge.Hide();
        }

        private void BuildVisual()
        {
            iconSr = new GameObject("Icon").AddComponent<SpriteRenderer>();
            iconSr.transform.SetParent(transform, false);
            iconSr.sprite = pendingIcon;
            iconSr.color = new Color(1f, 0.85f, 0.25f, 1f);
            iconSr.sortingOrder = 100;
            iconSr.transform.localScale = Vector3.one * 0.6f;

            timerText = new GameObject("Timer").AddComponent<TextMeshPro>();
            timerText.transform.SetParent(transform, false);
            timerText.transform.localPosition = new Vector3(0f, -0.65f, 0f);
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.fontSize = 3f;
            timerText.fontStyle = FontStyles.Bold;
            timerText.color = new Color(1f, 0.85f, 0.25f, 1f);
            timerText.sortingOrder = 100;
            timerText.text = "10.0s";
        }

        private void FindAnchor()
        {
            if (anchor != null) return;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) anchor = player.transform;
        }

        private void LateUpdate()
        {
            FindAnchor();
            if (anchor == null) { Hide(); return; }
            if (buff == null) buff = anchor.GetComponent<InfiniteBreadBuff>();

            transform.position = new Vector3(anchor.position.x, anchor.position.y + 2.3f, 0f);

            if (buff != null && buff.IsActive)
            {
                Show();
                timerText.text = $"{buff.RemainingSeconds:F1}s";
                float s = 0.6f + Mathf.Sin(Time.time * 6f) * 0.06f;
                iconSr.transform.localScale = new Vector3(s, s, 1f);
            }
            else
            {
                Hide();
            }
        }

        private void Show()
        {
            if (iconSr != null) iconSr.enabled = true;
            if (timerText != null) timerText.enabled = true;
        }

        private void Hide()
        {
            if (iconSr != null) iconSr.enabled = false;
            if (timerText != null) timerText.enabled = false;
        }

        private void HandleActivated(float _) { EnsureInstance(); }
        private void HandleExpired() { if (current != null) current.Hide(); }
    }
}
