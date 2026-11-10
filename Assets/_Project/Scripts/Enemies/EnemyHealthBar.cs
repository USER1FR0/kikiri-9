// Barra de vida flotante sobre enemigos resistentes. KISS: dos SpriteRenderers
// (bg negro + relleno rojo) hechos en runtime desde texturas 1x1, sin Canvas.

using UnityEngine;

namespace Kikiri9.Enemies
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(1.2f, 0.18f);
        [SerializeField] private float yOffset = 2.4f;
        [SerializeField] private Color bgColor = new Color(0f, 0f, 0f, 0.7f);
        [SerializeField] private Color fillColor = new Color(0.95f, 0.25f, 0.25f, 1f);

        private EnemyHealth health;
        private Transform fillTransform;
        private int lastShown = -1;
        private int maxHealth;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            maxHealth = GetMaxHealthViaReflection();
            if (maxHealth <= 1) { enabled = false; return; }
            BuildBar();
        }

        private int GetMaxHealthViaReflection()
        {
            var f = typeof(EnemyHealth).GetField("maxHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return f != null ? (int)f.GetValue(health) : 1;
        }

        private int GetCurrentHealthViaReflection()
        {
            var f = typeof(EnemyHealth).GetField("currentHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return f != null ? (int)f.GetValue(health) : maxHealth;
        }

        private void BuildBar()
        {
            var root = new GameObject("HealthBar");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, yOffset, 0f);

            var bg = new GameObject("BG");
            bg.transform.SetParent(root.transform, false);
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite = SolidSprite();
            bgSr.color = bgColor;
            bgSr.sortingOrder = 50;
            bg.transform.localScale = new Vector3(size.x, size.y, 1f);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(root.transform, false);
            fillGo.transform.localPosition = new Vector3(-size.x / 2f, 0f, 0f);
            var fsr = fillGo.AddComponent<SpriteRenderer>();
            fsr.sprite = SolidSprite();
            fsr.color = fillColor;
            fsr.sortingOrder = 51;
            fillGo.transform.localScale = new Vector3(size.x, size.y * 0.75f, 1f);
            // Pivot izquierdo para que escale desde ahi
            fillTransform = fillGo.transform;
            fillTransform.localPosition = new Vector3(0f, 0f, 0f);
        }

        private static Sprite cachedSolidSprite;
        private static Sprite SolidSprite()
        {
            if (cachedSolidSprite != null) return cachedSolidSprite;
            var t = new Texture2D(2, 2);
            t.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            t.Apply();
            cachedSolidSprite = Sprite.Create(t, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
            return cachedSolidSprite;
        }

        private void LateUpdate()
        {
            if (fillTransform == null) return;
            int current = GetCurrentHealthViaReflection();
            if (current == lastShown) return;
            lastShown = current;
            float ratio = Mathf.Clamp01((float)current / maxHealth);
            var s = fillTransform.localScale;
            fillTransform.localScale = new Vector3(size.x * ratio, s.y, 1f);
        }
    }
}
