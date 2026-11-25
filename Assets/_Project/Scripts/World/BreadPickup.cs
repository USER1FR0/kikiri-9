// Bread pickup. On trigger with the pollo, adds one bread to PlayerAmmo, plays a
// pickup sound and destroys itself. Same lifecycle as BottlePickup.

using Kikiri9.Player;
using UnityEngine;

namespace Kikiri9.World
{
    [RequireComponent(typeof(Collider2D))]
    public class BreadPickup : MonoBehaviour
    {
        [SerializeField] private int amount = 1;
        [SerializeField] private Sprite pickupVfxSprite;
        [SerializeField] private AudioClip pickupClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

        private bool collected;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected) return;
            if (!other.CompareTag("Player")) return;
            collected = true;

            var ammo = other.GetComponent<PlayerAmmo>();
            if (ammo != null) ammo.Add(amount);

            if (pickupVfxSprite != null)
                VfxSpawner.Spawn(pickupVfxSprite, transform.position, 0.4f, 0.6f, 20);
            if (pickupClip != null && Camera.main != null)
                AudioSource.PlayClipAtPoint(pickupClip, Camera.main.transform.position, volume);

            Destroy(gameObject);
        }
    }
}
