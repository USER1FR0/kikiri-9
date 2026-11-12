// Buff que hace que PlayerAmmo no consuma pan por una duracion dada.
// Lo activa el BreadBoostPickup. Se auto-apaga al terminar el tiempo.

using System;
using UnityEngine;

namespace Kikiri9.Player
{
    [RequireComponent(typeof(PlayerAmmo))]
    public class InfiniteBreadBuff : MonoBehaviour
    {
        public static event Action<float> OnActivated; // segundos restantes al activar
        public static event Action OnExpired;

        private PlayerAmmo ammo;
        private float endTime;
        private bool active;

        public bool IsActive => active;
        public float RemainingSeconds => active ? Mathf.Max(0f, endTime - Time.time) : 0f;

        private void Awake()
        {
            ammo = GetComponent<PlayerAmmo>();
        }

        public void Activate(float duration)
        {
            if (duration <= 0f) return;
            endTime = Time.time + duration;
            if (!active)
            {
                active = true;
                if (ammo != null) ammo.SetInfinite(true);
                OnActivated?.Invoke(duration);
            }
            else
            {
                // Si ya estaba activo, extiende la duracion
                OnActivated?.Invoke(RemainingSeconds);
            }
        }

        private void Update()
        {
            if (!active) return;
            if (Time.time < endTime) return;
            active = false;
            if (ammo != null) ammo.SetInfinite(false);
            OnExpired?.Invoke();
        }

        private void OnDisable()
        {
            if (active && ammo != null) ammo.SetInfinite(false);
            active = false;
        }
    }
}
