// Contador del Pan Barredor en el HUD. Igual que PanBombaHUDBadge:
// no mueve nada; solo agrega un sibling al contador de Bread en BottomBar.

using Kikiri9.Combat.Barredor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    public class PanBarredorHUDBadge : MonoBehaviour
    {
        [SerializeField] private Sprite barredorIcon;
        [SerializeField] private float xOffsetFromBread = 320f;
        [SerializeField] private float iconSize = 40f;
        [SerializeField] private int fontSize = 32;

        private TextMeshProUGUI countText;
        private PanBarredorAmmo ammo;

        private void Start()
        {
            TryBuild();
            PanBarredorAmmo.OnCountChanged += HandleCountChanged;
        }

        private void OnDestroy()
        {
            PanBarredorAmmo.OnCountChanged -= HandleCountChanged;
        }

        private void TryBuild()
        {
            var breadNode = GameObject.Find("Bread");
            if (breadNode == null) return;
            var breadRt = breadNode.GetComponent<RectTransform>();
            if (breadRt == null) return;

            var container = new GameObject("PanBarredor", typeof(RectTransform));
            container.transform.SetParent(breadRt.parent, false);
            var rt = container.GetComponent<RectTransform>();
            rt.anchorMin = breadRt.anchorMin;
            rt.anchorMax = breadRt.anchorMax;
            rt.pivot = breadRt.pivot;
            rt.sizeDelta = new Vector2(80f, breadRt.sizeDelta.y);
            rt.anchoredPosition = breadRt.anchoredPosition + new Vector2(xOffsetFromBread, 0f);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(container.transform, false);
            var iconRt = (RectTransform)iconGo.transform;
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(iconSize * 1.3f, iconSize);
            var img = iconGo.GetComponent<Image>();
            img.sprite = barredorIcon;
            img.color = new Color(1f, 0.85f, 0.45f, 0.75f); // semi-transparente para que se vea "grande"
            img.preserveAspect = true;
            img.raycastTarget = false;

            var txtGo = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(container.transform, false);
            var txtRt = (RectTransform)txtGo.transform;
            txtRt.anchorMin = new Vector2(0f, 0.5f);
            txtRt.anchorMax = new Vector2(0f, 0.5f);
            txtRt.pivot = new Vector2(0f, 0.5f);
            txtRt.anchoredPosition = new Vector2(iconSize * 1.3f + 8f, 0f);
            txtRt.sizeDelta = new Vector2(40f, iconSize);
            countText = txtGo.GetComponent<TextMeshProUGUI>();
            countText.alignment = TextAlignmentOptions.Left;
            countText.fontSize = fontSize;
            countText.fontStyle = FontStyles.Bold;
            countText.color = Color.white;
            countText.text = "0";
            countText.raycastTarget = false;

            container.SetActive(false);

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                ammo = player.GetComponent<PanBarredorAmmo>();
                if (ammo != null && ammo.Count > 0)
                {
                    container.SetActive(true);
                    countText.text = ammo.Count.ToString();
                }
            }
        }

        private void HandleCountChanged(int newCount)
        {
            if (countText == null) return;
            var container = countText.transform.parent.gameObject;
            if (newCount <= 0) container.SetActive(false);
            else
            {
                container.SetActive(true);
                countText.text = newCount.ToString();
            }
        }
    }
}
