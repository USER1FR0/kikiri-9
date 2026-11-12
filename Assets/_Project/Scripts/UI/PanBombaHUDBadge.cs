// Contador del pan bomba en el HUD. NO MUEVE ni altera nada del HUD existente.
// En Awake busca el objeto "Bread" del UI_Root (icon + count del pan normal) y crea
// un sibling pequenito a su derecha con icono de pan bomba + contador TMP.
// Si no encuentra el Bread, no hace nada (silencioso).

using Kikiri9.Combat.Bomba;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    public class PanBombaHUDBadge : MonoBehaviour
    {
        [SerializeField] private Sprite panBombaIcon;
        [SerializeField] private float xOffsetFromBread = 220f;
        [SerializeField] private float iconSize = 40f;
        [SerializeField] private int fontSize = 32;

        private TextMeshProUGUI countText;
        private PanBombaAmmo ammo;

        private void Start()
        {
            TryBuild();
            PanBombaAmmo.OnCountChanged += HandleCountChanged;
        }

        private void OnDestroy()
        {
            PanBombaAmmo.OnCountChanged -= HandleCountChanged;
        }

        private void TryBuild()
        {
            // Busca el GameObject "Bread" dentro del UI_Root (contador de pan normal)
            var breadNode = GameObject.Find("Bread");
            if (breadNode == null) return;
            var breadRt = breadNode.GetComponent<RectTransform>();
            if (breadRt == null) return;

            // Crear nuestro contenedor como hijo del MISMO padre (BottomBar) para heredar anchor
            var container = new GameObject("PanBomba", typeof(RectTransform));
            container.transform.SetParent(breadRt.parent, false);
            var rt = container.GetComponent<RectTransform>();
            rt.anchorMin = breadRt.anchorMin;
            rt.anchorMax = breadRt.anchorMax;
            rt.pivot = breadRt.pivot;
            rt.sizeDelta = new Vector2(80f, breadRt.sizeDelta.y);
            // Posicionarlo a la derecha del bread sin modificar el bread
            rt.anchoredPosition = breadRt.anchoredPosition + new Vector2(xOffsetFromBread, 0f);

            // Icono
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(container.transform, false);
            var iconRt = (RectTransform)iconGo.transform;
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(iconSize, iconSize);
            var img = iconGo.GetComponent<Image>();
            img.sprite = panBombaIcon;
            img.color = new Color(0.8f, 0.4f, 1f, 1f);
            img.preserveAspect = true;
            img.raycastTarget = false;

            // Count text
            var txtGo = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(container.transform, false);
            var txtRt = (RectTransform)txtGo.transform;
            txtRt.anchorMin = new Vector2(0f, 0.5f);
            txtRt.anchorMax = new Vector2(0f, 0.5f);
            txtRt.pivot = new Vector2(0f, 0.5f);
            txtRt.anchoredPosition = new Vector2(iconSize + 8f, 0f);
            txtRt.sizeDelta = new Vector2(40f, iconSize);
            countText = txtGo.GetComponent<TextMeshProUGUI>();
            countText.alignment = TextAlignmentOptions.Left;
            countText.fontSize = fontSize;
            countText.fontStyle = FontStyles.Bold;
            countText.color = Color.white;
            countText.text = "0";
            countText.raycastTarget = false;

            // Oculto hasta tener al menos 1
            container.SetActive(false);
            // Si ya hay stock inicial, mostrar
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                ammo = player.GetComponent<PanBombaAmmo>();
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
            if (newCount <= 0)
            {
                container.SetActive(false);
            }
            else
            {
                container.SetActive(true);
                countText.text = newCount.ToString();
            }
        }
    }
}
