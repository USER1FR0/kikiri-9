// Spawns straight-line Bread projectiles. The pan sprite is scaled down because the
// atlas cell is 3+ world units which would look huge next to the pollo.

using UnityEngine;

namespace Kikiri9.Combat
{
    public class BreadLauncher : MonoBehaviour
    {
        [SerializeField] private Transform muzzle;
        [SerializeField] private Sprite breadSprite;
        [SerializeField] private float launchSpeed = 14f;
        [SerializeField] private float cooldown = 0.3f;
        [SerializeField] private float visualScale = 0.35f;

        private float nextFireTime;

        public void SetSprite(Sprite s) => breadSprite = s;

        public bool TryLaunch(float extraForwardSpeed = 0f)
        {
            if (Time.time < nextFireTime) return false;
            nextFireTime = Time.time + cooldown;

            var origin = muzzle != null ? muzzle.position : transform.position;
            var go = new GameObject("Pan");
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * visualScale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = breadSprite;
            sr.sortingOrder = 15;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var cc = go.AddComponent<CircleCollider2D>();
            cc.radius = 0.4f;
            cc.isTrigger = true;

            var bread = go.AddComponent<Bread>();
            bread.Launch(new Vector2(launchSpeed + extraForwardSpeed, 0f));
            return true;
        }
    }
}
