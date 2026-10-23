// Bread ammo for the pollo. The launcher must spend one per shot; pickup prefabs
// refill via AddBread. Emits OnBreadChanged for the HUD.

using System;
using UnityEngine;

namespace Kikiri9.Player
{
    public class PlayerAmmo : MonoBehaviour
    {
        [SerializeField] private int startingBread = 5;
        [SerializeField] private int maxBread = 20;

        public event Action<int> OnBreadChanged;
        public int CurrentBread { get; private set; }

        private void Awake()
        {
            CurrentBread = startingBread;
        }

        private void Start()
        {
            OnBreadChanged?.Invoke(CurrentBread);
        }

        public bool TryConsume(int amount = 1)
        {
            if (CurrentBread < amount) return false;
            CurrentBread -= amount;
            OnBreadChanged?.Invoke(CurrentBread);
            return true;
        }

        public void Add(int amount = 1)
        {
            CurrentBread = Mathf.Min(maxBread, CurrentBread + amount);
            OnBreadChanged?.Invoke(CurrentBread);
        }
    }
}
