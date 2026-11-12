// Pickup que agrega 1 pan bomba al stock del pollo.

using UnityEngine;

namespace Kikiri9.Combat.Bomba
{
    // Unificado con Pan Barredor: ahora este pickup da 1 Pan Barredor (uso con L).
    [RequireComponent(typeof(Collider2D))]
    public class PanBombaPickup : MonoBehaviour
    {
        [SerializeField] private AudioClip pickupSfx;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            var ammo = other.GetComponent<Kikiri9.Combat.Barredor.PanBarredorAmmo>();
            if (ammo == null) ammo = other.gameObject.AddComponent<Kikiri9.Combat.Barredor.PanBarredorAmmo>();
            ammo.Add(1);
            if (pickupSfx != null) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);
            Destroy(gameObject);
        }
    }
}
