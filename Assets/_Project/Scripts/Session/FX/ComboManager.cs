// Combo = kills sin recibir dano. Se rompe solo con danio. Muestra:
//  - texto flotante sobre el pollo (x2, x3, x5, x8, MAX) con pop
//  - sprite destello (combo_destello.png, 5 frames @ 20fps) anima junto al texto
// El sprite se carga desde Resources/VFX/combo_destello

using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Kikiri9.Session.FX
{
    public class ComboManager : MonoBehaviour
    {
        public static ComboManager Instance { get; private set; }
        public static event Action<int> OnComboChanged;

        [SerializeField] private int maxCombo = 10;
        [SerializeField] private AudioClip comboSfx;
        [SerializeField] private Sprite[] destelloFrames;
        [SerializeField, Range(1, 60)] private int destelloFps = 20;

        private int kills;
        private float displayScale = 1f;
        private AudioSource audio;

        private TextMeshPro label;
        private SpriteRenderer destelloSr;
        private Transform anchor;
        private Coroutine destelloRoutine;

        public int Combo => Multiplier(kills);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            audio = GetComponent<AudioSource>();
            if (audio == null) audio = gameObject.AddComponent<AudioSource>();
            audio.pitch = 0.75f;
            audio.playOnAwake = false;
            TryLoadDestelloFromResources();
            BuildVisual();
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void TryLoadDestelloFromResources()
        {
            if (destelloFrames != null && destelloFrames.Length > 0) return;
            var all = Resources.LoadAll<Sprite>("VFX/combo_destello");
            if (all != null && all.Length > 0)
            {
                System.Array.Sort(all, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
                destelloFrames = all;
            }
        }

        private void BuildVisual()
        {
            var destelloGo = new GameObject("ComboDestello");
            destelloGo.transform.SetParent(transform, false);
            destelloSr = destelloGo.AddComponent<SpriteRenderer>();
            destelloSr.sortingOrder = 250;
            destelloSr.enabled = false;
            destelloGo.transform.localScale = Vector3.one * 4f;

            var textGo = new GameObject("ComboLabel");
            textGo.transform.SetParent(transform, false);
            textGo.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            label = textGo.AddComponent<TextMeshPro>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 5f;
            label.fontStyle = FontStyles.Bold;
            label.color = new Color(1f, 0.85f, 0.3f, 1f);
            label.outlineColor = new Color(0.5f, 0.15f, 0.05f, 1f);
            label.outlineWidth = 0.25f;
            label.sortingOrder = 200;
            label.text = "";
        }

        private int Multiplier(int k)
        {
            if (k <= 0) return 0;
            return Mathf.Min(k, maxCombo);
        }

        public static void NotifyKill()
        {
            if (Instance == null) return;
            Instance.kills++;
            Instance.PulseVisual();
            if (Instance.comboSfx != null) Instance.audio.PlayOneShot(Instance.comboSfx, 0.7f);
            OnComboChanged?.Invoke(Instance.kills);

            if (Instance.kills >= Instance.maxCombo)
            {
                Instance.GrantMaxReward();
                Instance.kills = 0;
                OnComboChanged?.Invoke(0);
                if (Instance.label != null) Instance.label.text = "";
            }
        }

        private void GrantMaxReward()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            // MAX combo = +1 Pan Barredor acumulable (uso con L)
            var ammo = player.GetComponent<Kikiri9.Combat.Barredor.PanBarredorAmmo>();
            if (ammo == null) ammo = player.AddComponent<Kikiri9.Combat.Barredor.PanBarredorAmmo>();
            ammo.Add(1);
            if (comboSfx != null) audio.PlayOneShot(comboSfx, 1f);
        }

        public static void NotifyDamage()
        {
            if (Instance == null) return;
            if (Instance.kills == 0) return;
            Instance.kills = 0;
            if (Instance.label != null) Instance.label.text = "";
            OnComboChanged?.Invoke(0);
        }

        public static int CurrentMultiplier() => Instance != null ? Instance.Combo : 1;

        private void PulseVisual()
        {
            if (label == null) return;
            int m = Multiplier(kills);
            if (m < 2) { label.text = ""; return; }
            label.text = m >= maxCombo ? "MAX" : $"x{m}";
            displayScale = 1.8f;

            if (destelloFrames != null && destelloFrames.Length > 0)
            {
                if (destelloRoutine != null) StopCoroutine(destelloRoutine);
                destelloRoutine = StartCoroutine(PlayDestello());
            }
        }

        private IEnumerator PlayDestello()
        {
            destelloSr.enabled = true;
            float frameTime = 1f / Mathf.Max(1, destelloFps);
            foreach (var frame in destelloFrames)
            {
                destelloSr.sprite = frame;
                yield return new WaitForSecondsRealtime(frameTime);
            }
            destelloSr.enabled = false;
            destelloRoutine = null;
        }

        private float playerLookupTimer;
        private void LateUpdate()
        {
            if (anchor == null)
            {
                playerLookupTimer -= Time.unscaledDeltaTime;
                if (playerLookupTimer <= 0f)
                {
                    playerLookupTimer = 0.5f;
                    var p = GameObject.FindGameObjectWithTag("Player");
                    if (p != null) anchor = p.transform;
                }
            }
            if (anchor == null || label == null) return;
            // Solo actualiza si hay texto visible (ahorra tiempo cuando no hay combo)
            if (string.IsNullOrEmpty(label.text) && displayScale < 1.01f) return;
            transform.position = new Vector3(anchor.position.x, anchor.position.y + 2.0f, 0f);
            displayScale = Mathf.Lerp(displayScale, 1f, Time.unscaledDeltaTime * 8f);
            label.transform.localScale = Vector3.one * displayScale;
        }
    }
}
