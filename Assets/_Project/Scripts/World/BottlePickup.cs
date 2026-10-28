// Botella coleccionable. Al tocarla el pollo, suma puntaje, spawna un destello y
// se destruye. Sin inventario persistente de momento; solo visual + puntaje.

using Kikiri9.Session;
using UnityEngine;

namespace Kikiri9.World
{
    [RequireComponent(typeof(Collider2D))]
    public class BottlePickup : MonoBehaviour
    {
        [SerializeField] private int points = 25;
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

            if (ScoreTracker.Instance != null)
            {
                ScoreTracker.Instance.AddPoints(points);
                if (points == 25) ScoreTracker.Instance.AddBottle(); // verde
            }
            if (pickupVfxSprite != null)
                VfxSpawner.Spawn(pickupVfxSprite, transform.position, 0.4f, 0.6f, 20);
            if (pickupClip != null && Camera.main != null)
                AudioSource.PlayClipAtPoint(pickupClip, Camera.main.transform.position, volume);

            Destroy(gameObject);
        }
    }
}
