// Reads a LevelDefinition, applies world speed, watches progress. On reaching the
// target distance, enters LevelOutro: spawner disabled, pollo keeps running on its own,
// camera freezes. When the pollo leaves the right edge of the screen, dispatches
// LevelComplete and the panel shows up.

using System.Collections;
using Kikiri9.CameraSystem;
using Kikiri9.Session;
using Kikiri9.World;
using UnityEngine;

namespace Kikiri9.Levels
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private LevelDefinition definition;
        [SerializeField] private float outroExitOffset = 10f; // cuanto pasa el pollo del borde derecho antes del panel

        public LevelDefinition Definition => definition;
        public float Progress01
        {
            get
            {
                if (definition == null || definition.targetDistance <= 0f) return 0f;
                if (WorldScroller.Instance == null) return 0f;
                return Mathf.Clamp01(WorldScroller.Instance.MetersAdvanced / definition.targetDistance);
            }
        }

        private bool goalReached;
        private bool subscribed;

        private void Start()
        {
            if (definition == null) { Debug.LogError("[Level] definition missing"); return; }
            StartCoroutine(BindWorldScroller());
        }

        private IEnumerator BindWorldScroller()
        {
            while (WorldScroller.Instance == null) yield return null;
            WorldScroller.Instance.SetProgression(definition.worldSpeed, definition.maxSpeed, definition.targetDistance);
            WorldScroller.Instance.OnMetersChanged -= CheckProgress;
            WorldScroller.Instance.OnMetersChanged += CheckProgress;
            subscribed = true;
            CheckProgress(WorldScroller.Instance.MetersAdvanced);
        }

        private void OnDestroy()
        {
            if (subscribed && WorldScroller.Instance != null)
                WorldScroller.Instance.OnMetersChanged -= CheckProgress;
        }

        private void CheckProgress(float meters)
        {
            if (goalReached || definition == null) return;
            if (meters < definition.targetDistance) return;
            goalReached = true;
            Debug.Log($"[Kikiri-9] Goal reached at {meters:F1}m, starting outro");
            StartCoroutine(OutroSequence());
        }

        private IEnumerator OutroSequence()
        {
            // 1. Entrar en estado LevelOutro: tiempo sigue corriendo, inputs del pollo bloqueados
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.SetState(GameState.LevelOutro);

            // 2. Desactivar el spawner (no mas enemigos ni botellas)
            var spawner = FindFirstObjectByType<LevelSpawner>();
            if (spawner != null) spawner.enabled = false;

            // 3. Congelar la camara donde esta
            var cam = Camera.main;
            var follow = cam != null ? cam.GetComponent<CameraFollow>() : null;
            if (follow != null) follow.SetTarget(null);

            // 4. Esperar a que el pollo salga por la derecha de la pantalla
            var pollo = GameObject.FindGameObjectWithTag("Player");
            if (pollo == null || cam == null)
            {
                yield return new WaitForSeconds(1.5f);
                if (GameStateManager.Instance != null)
                    GameStateManager.Instance.SetState(GameState.LevelComplete);
                yield break;
            }

            float rightEdge = cam.transform.position.x + cam.orthographicSize * cam.aspect + outroExitOffset;
            while (pollo != null && pollo.transform.position.x < rightEdge)
                yield return null;

            // 5. Panel LevelComplete
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.SetState(GameState.LevelComplete);
        }
    }
}
