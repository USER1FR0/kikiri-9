// Stock de panes barredores. Se gana 1 cada vez que el combo llega a MAX.

using System;
using UnityEngine;

namespace Kikiri9.Combat.Barredor
{
    public class PanBarredorAmmo : MonoBehaviour
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
