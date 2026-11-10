// Scene-level source of world movement. Provides a single speed and tracks accumulated
// meters. Supports a progressive speed curve (lineal baseSpeed -> maxSpeed over the
// target distance), driven by LevelController.
// Si BossArenaManager.IsActive = true, el scroll se congela hasta que el jefe caiga.

using System;
using Kikiri9.Session;
using UnityEngine;

namespace Kikiri9.World
{
    public class WorldScroller : MonoBehaviour
    {
        public static WorldScroller Instance { get; private set; }

        [SerializeField] private float initialSpeed = 5f;

        public event Action<float> OnMetersChanged;

        public float CurrentSpeed { get; private set; }
        public float MetersAdvanced { get; private set; }

        private float baseSpeed, maxSpeed, targetMeters;
        private bool progressionEnabled;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            CurrentSpeed = initialSpeed;
            baseSpeed = initialSpeed;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetSpeed(float speed)
        {
            CurrentSpeed = Mathf.Max(0f, speed);
            baseSpeed = CurrentSpeed;
            progressionEnabled = false;
        }

        public void SetProgression(float baseS, float maxS, float targetM)
        {
            baseSpeed = Mathf.Max(0f, baseS);
            maxSpeed = Mathf.Max(baseSpeed, maxS);
            targetMeters = Mathf.Max(1f, targetM);
            progressionEnabled = true;
            CurrentSpeed = baseSpeed;
        }

        private void Update()
        {
            if (BossArenaManager.IsActive) { return; }

            if (progressionEnabled && targetMeters > 0f)
            {
                float t = Mathf.Clamp01(MetersAdvanced / targetMeters);
                CurrentSpeed = Mathf.Lerp(baseSpeed, maxSpeed, t);
            }
            if (CurrentSpeed <= 0f) return;
            MetersAdvanced += CurrentSpeed * Time.deltaTime;
            OnMetersChanged?.Invoke(MetersAdvanced);
        }
    }
}
