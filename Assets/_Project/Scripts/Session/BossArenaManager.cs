// Estado global del arena del jefe. Mientras IsActive:
//   - WorldScroller se congela (CurrentSpeed = 0).
//   - PlayerController entra en modo libre (A/D controlan X, runSpeed ignorado).
//   - LevelSpawner no spawnea nada nuevo.
// Al desactivarse todo vuelve al flujo normal.
//
// ActiveBossHealth es opcional: quien active el arena (MiniBossMomia, y luego la
// Coronela en el nivel 4) puede registrar su EnemyHealth aqui para que el HUD
// sustituya la barra de recorrido por la vida del jefe mientras dura la pelea.

using System;
using Kikiri9.Enemies;
using UnityEngine;

namespace Kikiri9.Session
{
    public class BossArenaManager : MonoBehaviour
    {
        public static BossArenaManager Instance { get; private set; }
        public static bool IsActive => Instance != null && Instance.active;
        public static EnemyHealth ActiveBossHealth { get; private set; }
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
            if (!active) ActiveBossHealth = null;
            OnActiveChanged?.Invoke(active);
        }

        // El jefe se registra al activar el arena (y se limpia solo al desactivarla).
        public void SetBossHealth(EnemyHealth health)
        {
            ActiveBossHealth = health;
        }
    }
}
