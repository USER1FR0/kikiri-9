// Rafaga v4: expone FireFromExternal() para que el boton de UI pueda disparar sin
// duplicar la logica. Ademas, enciende la Estela via EstelaRafagaDriver, aplica
// invulnerabilidad 0.5s y anima slide. Dano 2, cooldown 9s.

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
        [SerializeField] private int damage = 3;

        [Header("Hitbox")]
        [SerializeField] private Vector2 hitboxSize = new Vector2(3.2f, 2.2f);
        [SerializeField] private float hitboxForward = 1.4f;
        [SerializeField] private float hitboxLifetime = 0.4f;

        [Header("Animacion e inmunidad")]
        [SerializeField] private float animDuration = 0.45f;
        [SerializeField] private float invulnDuration = 0.5f;

        private InputAction punchAction;
        private PlayerController pc;
        private PlayerHealth health;
        private PlayerSfx sfx;
        private EstelaRafagaDriver estelaDriver;
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
            estelaDriver = GetComponent<EstelaRafagaDriver>();

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
            TryFire();
        }

        // Para que el boton de UI pueda disparar (RafagaButtonHook)
        public void FireFromExternal()
        {
            TryFire();
        }

        [Header("Estela VFX fallback")]
        [SerializeField] private Sprite estelaFallbackSprite;
        [SerializeField] private float estelaVisualScale = 0.04f;
        [SerializeField] private float estelaDuration = 0.45f;

        private void TryFire()
        {
            if (!Ready) return;
            var gm = GameStateManager.Instance;
            if (gm != null && gm.CurrentState != GameState.Playing) return;

            nextPunchTime = Time.time + cooldown;

            if (pc != null) pc.BeginPunchAnimation(animDuration);
            if (health != null) health.SetTemporaryInvulnerability(invulnDuration);
            if (estelaDriver != null) estelaDriver.ShowForDuration();
            // Fallback visual: spawn un sprite estela directo, garantizado (si no se asigno
            // el driver o no se encontro el hijo Estela).
            SpawnEstelaFallback();
            if (sfx != null) sfx.PlaySlide();

            SpawnHitbox();
        }

        private void SpawnEstelaFallback()
        {
            if (estelaFallbackSprite == null) return;
            var go = new GameObject("EstelaVFX");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0.2f, 0.0f, 0f);
            go.transform.localScale = Vector3.one * estelaVisualScale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = estelaFallbackSprite;
            sr.color = new Color(1f, 0.75f, 0.25f, 1f);
            var polloSr = GetComponent<SpriteRenderer>();
            sr.sortingOrder = (polloSr != null ? polloSr.sortingOrder : 5) + 2;
            Destroy(go, estelaDuration);
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
            if (h != null) h.TakeDamage(damage, Kikiri9.Enemies.New.DamageSource.Rafaga, transform.position);
        }
    }
}
