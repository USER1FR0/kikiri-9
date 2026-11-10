// Mientras BossArenaManager.IsActive, bloquea la camara en el centro del arena.
// Al terminar, restaura el CameraFollow para que siga al pollo normal.

using Kikiri9.CameraSystem;
using Kikiri9.Session;
using UnityEngine;

namespace Kikiri9.World
{
    public class ArenaCameraLock : MonoBehaviour
    {
        public static ArenaCameraLock Instance { get; private set; }

        private float lockedX;
        private bool lockActive;
        private CameraFollow follow;
        private Transform camT;
        private Transform savedTarget;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            BossArenaManager.OnActiveChanged += HandleArenaChanged;
        }

        private void OnDisable()
        {
            BossArenaManager.OnActiveChanged -= HandleArenaChanged;
        }

        public void SetLockCenter(float x)
        {
            lockedX = x;
        }

        private void HandleArenaChanged(bool active)
        {
            if (Camera.main == null) return;
            camT = Camera.main.transform;
            follow = Camera.main.GetComponent<CameraFollow>();

            if (active)
            {
                lockActive = true;
                if (follow != null)
                {
                    var f = typeof(CameraFollow).GetField("target",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (f != null) savedTarget = (Transform)f.GetValue(follow);
                    follow.SetTarget(null);
                }
                lockedX = camT.position.x + 6f; // centro del arena ligeramente al frente
            }
            else
            {
                lockActive = false;
                if (follow != null && savedTarget != null) follow.SetTarget(savedTarget);
            }
        }

        private void LateUpdate()
        {
            if (!lockActive || camT == null) return;
            var p = camT.position;
            p.x = Mathf.Lerp(p.x, lockedX, 0.15f);
            camT.position = p;
        }
    }
}
