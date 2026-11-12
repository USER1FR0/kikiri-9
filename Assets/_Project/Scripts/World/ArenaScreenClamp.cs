// Mientras WaveManager.WaveActive = true (oleada con free movement), clampa al pollo
// al area visible de camara. Fuera de ese modo no hace nada.

using Kikiri9.Levels.Waves;
using UnityEngine;

namespace Kikiri9.World
{
    public class ArenaScreenClamp : MonoBehaviour
    {
        [SerializeField] private float edgePadding = 1.0f;
        private Transform player;
        private Camera cam;

        private void Start()
        {
            cam = Camera.main;
        }

        private void LateUpdate()
        {
            if (!WaveManager.WaveActive) return;
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }
            if (player == null || cam == null) return;
            float halfW = cam.orthographicSize * cam.aspect;
            float minX = cam.transform.position.x - halfW + edgePadding;
            float maxX = cam.transform.position.x + halfW - edgePadding;
            var pos = player.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            player.position = pos;
        }
    }
}
