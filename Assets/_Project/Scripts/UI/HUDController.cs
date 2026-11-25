// Scene HUD. Shows hearts, score, time mm:ss, meters/target, progress bar, bread count and
// coin count. Reacts to PlayerHealth, ScoreTracker, WorldScroller and LevelController; all
// optional so the HUD degrades gracefully if any system is missing.

using Kikiri9.Enemies;
using Kikiri9.Levels;
using Kikiri9.Player;
using Kikiri9.Session;
using Kikiri9.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Hearts")]
        [SerializeField] private Image[] heartImages;
        [SerializeField] private Sprite heartFullSprite;
        [SerializeField] private Sprite heartEmptySprite;

        [Header("Counters")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text metersText;
        [SerializeField] private TMP_Text breadText;
        [SerializeField] private TMP_Text coinsText;

        [Header("Progress")]
        [SerializeField] private Slider progressSlider;

        // Cache para evitar rebuilds de Canvas cada frame cuando los valores cambian solo
        // una fraccion. Antes esto disparaba SetVerticesDirty 60 veces/seg con 700+ batches.
        private int lastMetersInt = int.MinValue;
        private float lastProgress = -1f;
        private int lastBossHp = int.MinValue;
        private float lastBossProgress = -1f;

        private PlayerHealth health;
        private PlayerAmmo ammo;
        private ScoreTracker score;
        private WorldScroller world;
        private LevelController level;
        private float runStart;

        private void Start()
        {
            health = FindFirstObjectByType<PlayerHealth>();
            ammo = FindFirstObjectByType<PlayerAmmo>();
            if (ammo != null)
            {
                ammo.OnBreadChanged += SetBread;
                SetBread(ammo.CurrentBread);
            }
            score = ScoreTracker.Instance;
            world = WorldScroller.Instance;
            level = FindFirstObjectByType<LevelController>();

            if (health != null)
            {
                health.OnHealthChanged += UpdateHearts;
                UpdateHearts(health.CurrentHealth, health.MaxHealth);
            }

            if (score != null)
            {
                score.OnScoreChanged += UpdateScore;
                score.OnBottlesChanged += SetCoins;
                UpdateScore(0);
                SetCoins(0);
            }

            runStart = Time.time;
            UpdateMetersAndProgress();
        }

        private void OnDestroy()
        {
            if (health != null) health.OnHealthChanged -= UpdateHearts;
            if (ammo != null) ammo.OnBreadChanged -= SetBread;
            if (score != null)
            {
                score.OnScoreChanged -= UpdateScore;
                score.OnBottlesChanged -= SetCoins;
            }
        }

        private void Update()
        {
            if (timeText != null)
            {
                float t = Time.time - runStart;
                int mm = Mathf.FloorToInt(t / 60f);
                int ss = Mathf.FloorToInt(t % 60f);
                timeText.text = $"{mm:00}:{ss:00}";
            }
            UpdateMetersAndProgress();
        }

        private void UpdateMetersAndProgress()
        {
            // Jefe activo: barra de progreso muestra HP del jefe (ya cambia en pasos de 1,
            // pero cacheamos igual para evitar setear el mismo valor dos veces).
            if (BossArenaManager.IsActive && BossArenaManager.ActiveBossHealth != null)
            {
                var boss = BossArenaManager.ActiveBossHealth;
                if (metersText != null && boss.CurrentHealth != lastBossHp)
                {
                    metersText.text = $"JEFE {boss.CurrentHealth} / {boss.MaxHealth}";
                    lastBossHp = boss.CurrentHealth;
                }
                if (progressSlider != null)
                {
                    float p = boss.MaxHealth > 0 ? Mathf.Clamp01((float)boss.CurrentHealth / boss.MaxHealth) : 0f;
                    if (!Mathf.Approximately(p, lastBossProgress))
                    {
                        progressSlider.value = p;
                        lastBossProgress = p;
                    }
                }
                return;
            }

            if (world == null) return;
            float meters = world.MetersAdvanced;
            float target = level != null && level.Definition != null ? level.Definition.targetDistance : 0f;
            float displayMeters = target > 0f ? Mathf.Min(meters, target) : meters;

            // Texto solo cuando cambia el entero (antes se reasignaba el mismo string 60/seg).
            int metersInt = Mathf.FloorToInt(displayMeters);
            if (metersText != null && metersInt != lastMetersInt)
            {
                metersText.text = target > 0f ? $"{metersInt} / {Mathf.FloorToInt(target)} m" : $"{metersInt} m";
                lastMetersInt = metersInt;
            }

            // Slider solo cuando el valor cambia en >= 0.5% (ancho tipico de 1 pixel en barra
            // de 200px). Evita rebuilds del Canvas por fracciones imperceptibles.
            if (progressSlider != null && target > 0f)
            {
                float newProgress = Mathf.Clamp01(meters / target);
                if (Mathf.Abs(newProgress - lastProgress) > 0.005f || (newProgress >= 1f && lastProgress < 1f))
                {
                    progressSlider.value = newProgress;
                    lastProgress = newProgress;
                }
            }
        }

        private void UpdateHearts(int current, int max)
        {
            if (heartImages == null) return;
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] == null) continue;
                heartImages[i].enabled = i < max;
                var target = i < current ? heartFullSprite : heartEmptySprite;
                if (target != null) heartImages[i].sprite = target;
                // Color fallback when sprites are missing: full = red, empty = faded
                heartImages[i].color = i < current ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            }
        }

        private void UpdateScore(int value)
        {
            if (scoreText != null) scoreText.text = value.ToString("D5");
        }

        public void SetBread(int count) { if (breadText != null) breadText.text = count.ToString(); }
        public void SetCoins(int count) { if (coinsText != null) coinsText.text = count.ToString(); }
    }
}
