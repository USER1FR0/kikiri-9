// Scene-level singleton. Owns the current GameState, controls Time.timeScale for pause/end
// states and broadcasts changes to any listener (HUD, panels, controllers).

using System;
using UnityEngine;

namespace Kikiri9.Session
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public event Action<GameState> OnStateChanged;
        public GameState CurrentState { get; private set; } = GameState.Playing;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        public void SetState(GameState next)
        {
            if (CurrentState == next) return;
            CurrentState = next;
            Time.timeScale = (next == GameState.Playing || next == GameState.LevelOutro) ? 1f : 0f; // LevelOutro NO pausa
            OnStateChanged?.Invoke(next);
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing) SetState(GameState.Paused);
            else if (CurrentState == GameState.Paused) SetState(GameState.Playing);
        }
    }
}
