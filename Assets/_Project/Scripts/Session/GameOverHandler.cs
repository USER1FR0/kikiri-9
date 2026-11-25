// On the pollo. Translates "run ended" into either an immediate scene reload (Sandbox, no
// PlayerHealth sibling) or a GameOver state transition (levels with PlayerHealth and
// GameStateManager). When PlayerHealth exists, this component defers all damage logic to
// it and only reacts to its OnDeath event.

using Kikiri9.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kikiri9.Session
{
    public class GameOverHandler : MonoBehaviour
    {
        private const string EnemyTag = "Enemy";

        [SerializeField] private float restartDelay = 0.3f;
        [SerializeField] private bool fadeToBlack = false;

        private PlayerController player;
        private PlayerHealth health;
        private bool isGameOver;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            health = GetComponent<PlayerHealth>();
        }

        private void OnEnable()
        {
            if (health != null) health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            if (health != null) health.OnDeath -= HandleDeath;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (health != null) return;
            if (collision.gameObject.CompareTag(EnemyTag)) HandleDeath();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (health != null) return;
            if (other.CompareTag(EnemyTag)) HandleDeath();
        }

        public void HandleDeath()
        {
            if (isGameOver) return;
            isGameOver = true;
            Debug.Log("[Kikiri-9] GameOverHandler.HandleDeath");
            if (player != null) player.enabled = false;

            var sfx = GetComponent<Kikiri9.Player.PlayerSfx>();
            if (sfx != null) sfx.PlayGameOver();

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetState(GameState.GameOver);
                return;
            }

            Invoke(nameof(Reload), restartDelay);
        }

        private void Reload()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Debug: F10 fuerza GameOver (confirma que el panel se muestra correctamente).
        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.f10Key.wasPressedThisFrame)
            {
                Debug.Log("[Kikiri-9] F10: force GameOver");
                HandleDeath();
            }
        }

        private void OnGUI()
        {
            if (!fadeToBlack || !isGameOver) return;
            if (GameStateManager.Instance != null) return;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        }
    }
}
