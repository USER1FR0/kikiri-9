// Spawns straight-line Bread projectiles. The pan sprite is scaled down because the
// atlas cell is 3+ world units which would look huge next to the pollo.

using UnityEngine;

namespace Kikiri9.Combat
{
    public class BreadLauncher : MonoBehaviour
    {
        [SerializeField] private Transform muzzle;
        [SerializeField] private Sprite breadSprite;
        [SerializeField] private Sprite splashSprite;
        [SerializeField] private float launchSpeed = 9f;
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
            rb.gravityScale = 0.1f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var cc = go.AddComponent<CircleCollider2D>();
            cc.radius = 0.4f;
            cc.isTrigger = false;

            var bread = go.AddComponent<Bread>();
            if (splashSprite != null) bread.SetSplashSprite(splashSprite);
            bread.Launch(new Vector2(launchSpeed + extraForwardSpeed, 0f));
            return true;
        }
    }
}
