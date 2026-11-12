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

        [SerializeField] private Sprite panBombaSprite;
        [SerializeField] private Sprite panBombaExplosionSprite;

        public bool TryLaunch(float extraForwardSpeed = 0f)
        {
            if (Time.time < nextFireTime) return false;
            var ammo = GetComponent<Kikiri9.Player.PlayerAmmo>();
            if (ammo != null && !ammo.TryConsume(1)) return false;
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
            cc.isTrigger = true;
            try { go.tag = "Bread"; } catch (UnityEngine.UnityException) { /* tag opcional */ }

            var bread = go.AddComponent<Bread>();
            if (splashSprite != null) bread.SetSplashSprite(splashSprite);
            bread.Launch(new Vector2(launchSpeed + extraForwardSpeed, 0f));
            return true;
        }
        public bool TryLaunchBarredor(float extraForwardSpeed = 0f)
        {
            var ammo = GetComponent<Kikiri9.Combat.Barredor.PanBarredorAmmo>();
            if (ammo == null || ammo.Count <= 0) return false;
            if (!ammo.TryConsume()) return false;
            SpawnPanBarredor(extraForwardSpeed);
            return true;
        }

        private void SpawnPanBarredor(float extraForwardSpeed)
        {
            var origin = muzzle != null ? muzzle.position : transform.position;
            var go = new GameObject("PanBarredor");
            go.transform.position = origin;
            // Pan GRANDE (x2.5 del normal)
            go.transform.localScale = Vector3.one * (visualScale * 2.5f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = breadSprite;
            sr.color = new Color(1f, 0.85f, 0.45f, 0.6f); // dorado semi-transparente
            sr.sortingOrder = 15;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // vuela recto
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            var cc = go.AddComponent<CircleCollider2D>();
            cc.radius = 0.8f;
            cc.isTrigger = true;
            try { go.tag = "Bread"; } catch (UnityEngine.UnityException) { }
            var barredor = go.AddComponent<Kikiri9.Combat.Barredor.PanBarredor>();
            barredor.Launch(new Vector2(launchSpeed + extraForwardSpeed, 0f));
        }

        private void SpawnPanBomba(float extraForwardSpeed)
        {
            var origin = muzzle != null ? muzzle.position : transform.position;
            var go = new GameObject("PanBomba");
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * (visualScale * 1.25f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = panBombaSprite != null ? panBombaSprite : breadSprite;
            sr.color = new Color(0.7f, 0.4f, 1f, 1f); // tinte morado para distinguirlo
            sr.sortingOrder = 15;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0.1f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var cc = go.AddComponent<CircleCollider2D>();
            cc.radius = 0.4f;
            cc.isTrigger = true;
            try { go.tag = "Bread"; } catch (UnityEngine.UnityException) { }
            var bomba = go.AddComponent<Kikiri9.Combat.Bomba.PanBomba>();
            if (panBombaExplosionSprite != null) bomba.SetExplosionSprite(panBombaExplosionSprite);
            bomba.Launch(new Vector2(launchSpeed + extraForwardSpeed, 0f));
        }
    }
}