// Estado global del arena del jefe. Mientras IsActive:
//   - WorldScroller se congela (CurrentSpeed = 0).
//   - PlayerController entra en modo libre (A/D controlan X, runSpeed ignorado).
//   - LevelSpawner no spawnea nada nuevo.
// Al desactivarse todo vuelve al flujo normal.

using System;
using UnityEngine;

namespace Kikiri9.Session
{
    public class BossArenaManager : MonoBehaviour
    {
        public static BossArenaManager Instance { get; private set; }
        public static bool IsActive => Instance != null && Instance.active;
        public static event Action<bool> OnActiveChanged;

        private bool active;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetActive(bool value)
        {
            if (active == value) return;
            active = value;
            OnActiveChanged?.Invoke(active);
        }
    }
}
