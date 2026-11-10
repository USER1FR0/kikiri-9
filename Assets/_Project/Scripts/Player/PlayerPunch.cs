// Ráfaga v3: usa PlayerController.BeginPunchAnimation() para una animacion visible,
// hace al pollo invulnerable por 0.5s mientras golpea (asi ya no te quitan vida al
// pegarse al enemigo), daño 2, cooldown 9s.

using Kikiri9.Enemies;
using Kikiri9.Session;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kikiri9.Player
{
    public class PlayerPunch : MonoBehaviour
    {
        [Header("Cooldown y daño")]
        [SerializeField] private float cooldown = 9f;
        [SerializeField] private int damage = 2;

        [Header("Hitbox")]
        [SerializeField] private Vector2 hitboxSize = new Vector2(2.4f, 1.6f);
        [SerializeField] private float hitboxForward = 1.2f;
        [SerializeField] private float hitboxLifetime = 0.3f;

        [Header("Animacion e inmunidad")]
        [SerializeField] private float animDuration = 0.45f;
        [SerializeField] private float invulnDuration = 0.5f;

        private InputAction punchAction;
        private PlayerController pc;
        private PlayerHealth health;
        private PlayerSfx sfx;
        private float nextPunchTime;

        public float Cooldown => cooldown;
        public bool Ready => Time.time >= nextPunchTime;
        public float Remaining => Mathf.Max(0f, nextPunchTime - Time.time);
        public float Progress01 => Ready ? 1f : Mathf.Clamp01(1f - Remaining / cooldown);

        private void Awake()
        {
            pc = GetComponent<PlayerController>();
            health = GetComponent<PlayerHealth>();
            sfx = GetComponent<PlayerSfx>();

            punchAction = new InputAction("Punch", InputActionType.Button);
            punchAction.AddBinding("<Keyboard>/k");
            punchAction.AddBinding("<Mouse>/rightButton");
            punchAction.AddBinding("<Gamepad>/buttonNorth");
            punchAction.performed += OnPunch;
        }

        private void OnEnable() { punchAction.Enable(); }
        private void OnDisable() { punchAction.Disable(); }
        private void OnDestroy()
        {
            punchAction.performed -= OnPunch;
            punchAction.Dispose();
        }

        private void OnPunch(InputAction.CallbackContext _)
        {
            if (!Ready) return;
            var gm = GameStateManager.Instance;
            if (gm != null && gm.CurrentState != GameState.Playing) return;

            nextPunchTime = Time.time + cooldown;

            if (pc != null) pc.BeginPunchAnimation(animDuration);
            if (health != null) health.SetTemporaryInvulnerability(invulnDuration);
            if (sfx != null) sfx.PlaySlide();

            SpawnHitbox();
        }

        private void SpawnHitbox()
        {
            var go = new GameObject("PunchHitbox");
            go.transform.position = transform.position + new Vector3(hitboxForward, 0f, 0f);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = hitboxSize;
            go.AddComponent<PunchHitbox>().Setup(damage, hitboxLifetime);
        }
    }

    public class PunchHitbox : MonoBehaviour
    {
        private int damage;

        public void Setup(int d, float lifetime)
        {
            damage = d;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;
            var h = other.GetComponent<EnemyHealth>();
            if (h != null) h.TakeDamage(damage);
        }
    }
}
