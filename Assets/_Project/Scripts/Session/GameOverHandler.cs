// Put on the pollo. Touching anything tagged "Enemy" ends the run: player input stops and
// the active scene reloads after a short delay. The scene must be listed in Build Settings.

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
        private bool isGameOver;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(EnemyTag)) GameOver();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(EnemyTag)) GameOver();
        }

        public void GameOver()
        {
            if (isGameOver) return;
            isGameOver = true;
            if (player != null) player.enabled = false;
            Invoke(nameof(Reload), restartDelay);
        }

        private void Reload()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Optional simple black overlay while waiting for the reload.
        private void OnGUI()
        {
            if (!fadeToBlack || !isGameOver) return;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        }
    }
}
