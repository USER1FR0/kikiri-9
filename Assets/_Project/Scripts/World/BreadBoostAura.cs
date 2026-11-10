// Visual del pan dorado: destellos girando alrededor del pan normal usando sprites
// de "cempasuchil" (del sheet Effects/electricidad_activacion_cempasuchil.png), tematica
// Dia de Muertos. El sprite base del pan queda sin tint.

using UnityEngine;

namespace Kikiri9.World
{
    public class BreadBoostAura : MonoBehaviour
    {
        [SerializeField] private Sprite sparkleSprite;
        [SerializeField] private int sparkleCount = 3;
        [SerializeField] private float orbitRadius = 0.6f;
        [SerializeField] private float orbitSpeed = 140f;
        [SerializeField] private Color sparkleColor = new Color(1f, 0.6f, 0.2f, 1f);

        private Transform[] sparkles;

        private void Start()
        {
            BuildSparkles();
        }

        public void SetSparkleSprite(Sprite s) { sparkleSprite = s; }

        private void BuildSparkles()
        {
            if (sparkleSprite == null) return;
            sparkles = new Transform[sparkleCount];
            int baseOrder = GetComponent<SpriteRenderer>()?.sortingOrder ?? 0;
            for (int i = 0; i < sparkleCount; i++)
            {
                var go = new GameObject($"Sparkle_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sparkleSprite;
                sr.color = sparkleColor;
                sr.sortingOrder = baseOrder + 1;
                go.transform.localScale = Vector3.one * 0.3f;
                sparkles[i] = go.transform;
            }
        }

        private void Update()
        {
            if (sparkles == null) return;
            float t = Time.time * orbitSpeed * Mathf.Deg2Rad;
            for (int i = 0; i < sparkles.Length; i++)
            {
                if (sparkles[i] == null) continue;
                float angle = t + (Mathf.PI * 2f / sparkles.Length) * i;
                float x = Mathf.Cos(angle) * orbitRadius;
                float y = Mathf.Sin(angle) * orbitRadius;
                sparkles[i].localPosition = new Vector3(x, y, 0f);
                float scale = 0.3f + Mathf.Sin(Time.time * 4f + i) * 0.07f;
                sparkles[i].localScale = new Vector3(scale, scale, 1f);
            }
        }
    }
}
