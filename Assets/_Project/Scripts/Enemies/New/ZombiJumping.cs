// Zombi saltarin: avanza a la izquierda a velocidad moderada. Cada vez que ya
// pasaron 'jumpInterval' segundos pegado al piso, brinca una vez con telegraph
// corto. Patron bien legible: camina -> breve prep -> salto -> cae -> camina.

using System.Collections;
using UnityEngine;

namespace Kikiri9.Enemies.New
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyHealth))]
    public class ZombiJumping : MonoBehaviour
    {
        [SerializeField] private float horizontalSpeed = -2.2f;
        [SerializeField] private float jumpInterval = 0.2f;
        [SerializeField] private float jumpVelocity = 11f;
        [SerializeField] private float prepareDuration = 0.12f;
        [SerializeField] private float despawnDistanceBehind = 20f;
        [SerializeField] private float groundCheckY = -3.6f;
        [SerializeField] private float groundSnapThreshold = 0.25f;

        [Header("Audio")]
        [SerializeField] private AudioClip warnSfx;
        [SerializeField] private AudioClip landSfx;

        private Rigidbody2D rb;
        private Animator animator;
        private AudioSource audioSrc;
        private Transform camT;
        private float nextJumpTime;
        private bool inAir;
        private bool preparing;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2.2f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            audioSrc = GetComponent<AudioSource>();
            if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            if (Camera.main != null) camT = Camera.main.transform;
            nextJumpTime = Time.time + Random.Range(0.6f, jumpInterval);
            SetAnimState(0);
        }

        private bool IsOnGround()
        {
            return transform.position.y <= groundCheckY + groundSnapThreshold && Mathf.Abs(rb.linearVelocity.y) < 0.4f;
        }

        private void FixedUpdate()
        {
            // Movimiento horizontal siempre a la misma velocidad
            if (!preparing) rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);
            else rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // parado mientras prepara

            if (inAir)
            {
                // Detectar aterrizaje
                if (IsOnGround())
                {
                    inAir = false;
                    SetAnimState(0); // crouch/idle base
                    if (landSfx != null) audioSrc.PlayOneShot(landSfx);
                    nextJumpTime = Time.time + jumpInterval + Random.Range(-0.4f, 0.4f);
                }
                return;
            }

            if (!preparing && Time.time >= nextJumpTime && IsOnGround())
            {
                StartCoroutine(JumpSequence());
            }
        }

        private IEnumerator JumpSequence()
        {
            preparing = true;
            SetAnimState(1); // jump_prepare
            if (warnSfx != null) audioSrc.PlayOneShot(warnSfx);
            yield return new WaitForSeconds(prepareDuration);
            preparing = false;
            rb.linearVelocity = new Vector2(horizontalSpeed, jumpVelocity);
            inAir = true;
            SetAnimState(2); // jump_air
        }

        private void SetAnimState(int s)
        {
            if (animator == null) return;
            bool hasParam = false;
            foreach (var par in animator.parameters) if (par.name == "State") { hasParam = true; break; }
            if (hasParam) animator.SetInteger("State", s);
        }

        private void Update()
        {
            if (camT == null) return;
            if (transform.position.x < camT.position.x - despawnDistanceBehind)
                Destroy(gameObject);
        }
    }
}
