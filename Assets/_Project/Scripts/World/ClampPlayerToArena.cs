// Mientras BossArenaManager.IsActive, clampea la X del pollo dentro de ±range
// alrededor del centro del arena. Evita que el jugador se escape del combate.

using Kikiri9.Session;
using UnityEngine;

namespace Kikiri9.World
{
    public class ClampPlayerToArena : MonoBehaviour
    {
        [SerializeField] private float range = 6.5f;
        private float centerX;
        private Transform player;

        public void SetCenter(float x) { centerX = x; }

        private void LateUpdate()
        {
            if (!BossArenaManager.IsActive) return;
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }
            if (player == null) return;

            var pos = player.position;
            pos.x = Mathf.Clamp(pos.x, centerX - range, centerX + range);
            player.position = pos;
        }
    }
}
