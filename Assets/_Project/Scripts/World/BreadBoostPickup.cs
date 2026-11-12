// Pickup que activa el buff de pan infinito por 'duration' segundos.
// Mismo lifecycle que BreadPickup: trigger con Player, VFX + sfx + destroy.

using Kikiri9.Player;
using UnityEngine;

namespace Kikiri9.World
{
    [RequireComponent(typeof(Collider2D))]
    public class BreadBoostPickup : MonoBehaviour
    {
        [SerializeField] private AudioClip pickupSfx;
        [SerializeField] private float duration = 10f;
        [SerializeField] private Sprite pickupVfxSprite;
        [SerializeField] private AudioClip pickupClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.9f;

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

            var buff = other.GetComponent<InfiniteBreadBuff>();
            if (buff == null) buff = other.gameObject.AddComponent<InfiniteBreadBuff>();
            buff.Activate(duration);

            if (pickupVfxSprite != null)
                VfxSpawner.Spawn(pickupVfxSprite, transform.position, 0.5f, 0.8f, 20);
            if (pickupClip != null && Camera.main != null)
                AudioSource.PlayClipAtPoint(pickupClip, Camera.main.transform.position, volume);

            if (pickupSfx != null) AudioSource.PlayClipAtPoint(pickupSfx, transform.position, 0.8f);
            Destroy(gameObject);
        }
    }
}
