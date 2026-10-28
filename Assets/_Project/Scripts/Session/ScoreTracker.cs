// Scene-level score accumulator. Subscribes to EnemyHealth.OnAnyDefeated and exposes an
// AddPoints method for pickups. Fires OnScoreChanged for the HUD. Local to the run only.

using System;
using Kikiri9.Enemies;
using UnityEngine;

namespace Kikiri9.Session
{
    public class ScoreTracker : MonoBehaviour
    {
        public static ScoreTracker Instance { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action<int> OnBottlesChanged;
        public int CurrentScore { get; private set; }
        public int BottlesCollected { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            EnemyHealth.OnAnyDefeated += AddPoints;
        }

        private void OnDisable()
        {
            EnemyHealth.OnAnyDefeated -= AddPoints;
            if (Instance == this) Instance = null;
        }

        public void AddPoints(int amount)
        {
            if (amount <= 0) return;
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void AddBottle()
        {
            BottlesCollected++;
            OnBottlesChanged?.Invoke(BottlesCollected);
        }
    }
}
