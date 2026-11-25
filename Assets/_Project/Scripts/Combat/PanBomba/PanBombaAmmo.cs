// Stock separado de panes bomba. Se consume antes que el pan normal cuando hay al menos 1.

using System;
using UnityEngine;

namespace Kikiri9.Combat.Bomba
{
    public class PanBombaAmmo : MonoBehaviour
    {
        public static event Action<int> OnCountChanged;
        [SerializeField] private int count;

        public int Count => count;

        public void Add(int n)
        {
            count += n;
            OnCountChanged?.Invoke(count);
        }

        public bool TryConsume()
        {
            if (count <= 0) return false;
            count--;
            OnCountChanged?.Invoke(count);
            return true;
        }
    }
}
