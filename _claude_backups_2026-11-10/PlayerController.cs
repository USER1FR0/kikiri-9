// Auto-runner player controller for Kikiri-9 v4.
// Añade BeginPunchAnimation(duration) para que la ráfaga reproduzca la animacion
// de slide sin redimensionar el collider ni activar la logica normal de slide.

using Kikiri9.Combat;
using Kikiri9.Session;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kikiri9.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Run")]
        [SerializeField] private float runSpeed = 4f;

        [Header("Arena")]
        [SerializeField] private float arenaMoveSpeed = 5f;

        [Header("Jump")]
        [SerializeField] private float jumpVelocity = 15f;
        [SerializeField] private float coyoteTime = 0.08f;
        [SerializeField] private float jumpBuffer = 0.12f;
        [SerializeField] private float fallGravityMultiplier = 1.2f;
        [SerializeField] private float lowJumpGravityMultiplier = 1.8f;

        [Header("Slide")]
        [SerializeField] private float slideDuration = 0.9f;
        [SerializeField, Range(0.1f, 0.9f)] private float slideHeightFactor = 0.5f;
        [SerializeField] private float slideBuffer = 0.12f;

        [Header("Ground check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.18f;
        [SerializeField] private LayerMask groundLayer = ~0;

        [Header("References (optional)")]
        [SerializeField] private Animator animator;
        [SerializeField] private BreadLauncher breadLauncher;
        [SerializeField] private PlayerSfx sfx;

        private static readonly int HashIsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int HashIsSliding = Animator.StringToHash("IsSliding");
        private static readonly int HashSpeedY = Animator.StringToHash("SpeedY");
        private static readonly int HashJump = Animator.StringToHash("Jump");
        private static readonly int HashSlide = Animator.StringToHash("Slide");
        private static readonly int HashThrow = Animator.StringToHash("Throw");

        private Rigidbody2D rb;
        private CapsuleCollider2D col;
        private Vector2 colSizeFull;
        private Vector2 colOffsetFull;
        private float baseGravityScale;

        private InputAction jumpAction;
        private InputAction slideAction;
        private InputAction throwAction;
        private InputAction moveAction;

        private bool isGrounded;
        private bool wasGrounded;
        private float lastJumpTime = -999f;
        private bool isSliding;
        private int jumpsUsed;
        [SerializeField] private int maxJumpsInAir = 2;
        public void SetMaxJumpsInAir(int value) => maxJumpsInAir = Mathf.Max(0, value);
        private float slideEndTime;
        private float lastGroundedTime = -999f;
        private float lastJumpPressTime = -999f;
        private float lastSlidePressTime = -999f;
        private bool jumpHeld;
        private bool punchAnimActive;

        private Kikiri9.Levels.LevelController cachedLevelController;

        public Animator Animator => animator;
        public int HashIsSlidingPublic => HashIsSliding;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CapsuleCollider2D>();
            colSizeFull = col.size;
            colOffsetFull = col.offset;
            baseGravityScale = rb.gravityScale;
            if (animator == null) animator = GetComponent<Animator>();
            if (breadLauncher == null) breadLauncher = GetComponent<BreadLauncher>();
            if (sfx == null) sfx = GetComponent<PlayerSfx>();

            jumpAction = new InputAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space");
            jumpAction.AddBinding("<Keyboard>/w");
            jumpAction.AddBinding("<Keyboard>/upArrow");
            jumpAction.AddBinding("<Gamepad>/buttonSouth");
            jumpAction.performed += OnJumpPressed;
            jumpAction.canceled += OnJumpReleased;

            slideAction = new InputAction("Slide", InputActionType.Button);
            slideAction.AddBinding("<Keyboard>/s");
            slideAction.AddBinding("<Keyboard>/downArrow");
            slideAction.AddBinding("<Keyboard>/leftShift");
            slideAction.AddBinding("<Keyboard>/rightShift");
            slideAction.AddBinding("<Gamepad>/buttonEast");
            slideAction.performed += OnSlidePressed;

            throwAction = new InputAction("Throw", InputActionType.Button);
            throwAction.AddBinding("<Keyboard>/j");
            throwAction.AddBinding("<Mouse>/leftButton");
            throwAction.AddBinding("<Gamepad>/buttonWest");
            throwAction.performed += OnThrow;

            moveAction = new InputAction("Move", InputActionType.Value);
            var comp = moveAction.AddCompositeBinding("1DAxis");
            comp.With("negative", "<Keyboard>/a");
            comp.With("positive", "<Keyboard>/d");
            comp.With("negative", "<Keyboard>/leftArrow");
            comp.With("positive", "<Keyboard>/rightArrow");
            comp.With("negative", "<Gamepad>/leftStick/left");
            comp.With("positive", "<Gamepad>/leftStick/right");
        }

        private void OnEnable()  { jumpAction.Enable();  slideAction.Enable();  throwAction.Enable();  moveAction.Enable();  SubscribeStateForGrace(); }
        private void OnDisable() { jumpAction.Disable(); slideAction.Disable(); throwAction.Disable(); moveAction.Disable(); UnsubscribeStateForGrace(); }

        private void OnDestroy()
        {
            jumpAction.performed -= OnJumpPressed;
            jumpAction.canceled -= OnJumpReleased;
            slideAction.performed -= OnSlidePressed;
            throwAction.performed -= OnThrow;
            jumpAction.Dispose();
            slideAction.Dispose();
            throwAction.Dispose();
            moveAction.Dispose();
        }

        private float inputGraceEnd;
        private Kikiri9.Session.GameState lastState = Kikiri9.Session.GameState.Playing;

        private bool InputBlocked() => Time.time < inputGraceEnd;

        private int LiveMaxJumpsInAir()
        {
            if (BossArenaManager.IsActive) return 1;
            if (cachedLevelController == null)
                cachedLevelController = FindFirstObjectByType<Kikiri9.Levels.LevelController>();
            if (cachedLevelController != null && cachedLevelController.Definition != null)
                return Mathf.Max(0, cachedLevelController.Definition.maxJumpsInAir);
            return maxJumpsInAir;
        }

        public void BeginPunchAnimation(float duration)
        {
            StartCoroutine(PunchAnimRoutine(duration));
        }

        private IEnumerator PunchAnimRoutine(float duration)
        {
            punchAnimActive = true;
            if (animator != null)
            {
                animator.SetTrigger(HashSlide);
                animator.SetBool(HashIsSliding, true);
            }
            yield return new WaitForSeconds(duration);
            punchAnimActive = false;
            if (animator != null) animator.SetBool(HashIsSliding, false);
        }

        private void FixedUpdate()
        {
            TrackStateForInputGrace();
            isGrounded = groundCheck != null &&
                Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            if (isGrounded && Time.time - lastJumpTime > 0.1f) jumpsUsed = 0;
            if (isGrounded) lastGroundedTime = Time.time;
            wasGrounded = isGrounded;

            float horizontal;
            if (BossArenaManager.IsActive)
            {
                float axis = moveAction.ReadValue<float>();
                horizontal = axis * arenaMoveSpeed;
            }
            else
            {
                horizontal = runSpeed;
            }
            rb.linearVelocity = new Vector2(horizontal, rb.linearVelocity.y);

            int liveMax = LiveMaxJumpsInAir();
            bool withinCoyote = Time.time - lastGroundedTime <= coyoteTime;
            bool bufferedJump = Time.time - lastJumpPressTime <= jumpBuffer;
            bool canFirstJump = withinCoyote && jumpsUsed == 0;
            bool canAirJump = !isGrounded && jumpsUsed > 0 && jumpsUsed < liveMax;
            if ((canFirstJump || canAirJump) && bufferedJump && !isSliding)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                lastJumpPressTime = -999f;
                lastJumpTime = Time.time;
                jumpsUsed++;
                if (animator != null) animator.SetTrigger(HashJump);
                if (sfx != null) sfx.PlayJump();
            }

            bool bufferedSlide = Time.time - lastSlidePressTime <= slideBuffer;
            if (bufferedSlide && isGrounded && !isSliding)
            {
                StartSlide();
                lastSlidePressTime = -999f;
            }

            if (rb.linearVelocity.y < 0f)
                rb.gravityScale = baseGravityScale * fallGravityMultiplier;
            else if (rb.linearVelocity.y > 0f && !jumpHeld)
                rb.gravityScale = baseGravityScale * lowJumpGravityMultiplier;
            else
                rb.gravityScale = baseGravityScale;

            if (isSliding && Time.time >= slideEndTime)
                EndSlide();

            if (animator != null)
            {
                animator.SetBool(HashIsGrounded, isGrounded);
                if (!punchAnimActive) animator.SetBool(HashIsSliding, isSliding);
                animator.SetFloat(HashSpeedY, rb.linearVelocity.y);
            }
        }

        private void TrackStateForInputGrace()
        {
            var gm = Kikiri9.Session.GameStateManager.Instance;
            if (gm == null) return;
            if (gm.CurrentState != lastState)
            {
                if (gm.CurrentState == Kikiri9.Session.GameState.Playing && lastState != Kikiri9.Session.GameState.Playing)
                    inputGraceEnd = Time.time + 0.2f;
                lastState = gm.CurrentState;
            }
        }

        private bool IsGameplayActive()
        {
            var gsm = GameStateManager.Instance;
            return gsm == null || gsm.CurrentState == GameState.Playing;
        }

        private void OnJumpPressed(InputAction.CallbackContext _)
        {
            if (InputBlocked()) return;
            if (!IsGameplayActive()) return;
            lastJumpPressTime = Time.time;
            jumpHeld = true;
            if (isSliding) EndSlide();
        }

        private void OnJumpReleased(InputAction.CallbackContext _) { jumpHeld = false; }

        private void OnSlidePressed(InputAction.CallbackContext _)
        {
            if (InputBlocked()) return;
            if (!IsGameplayActive()) return;
            lastSlidePressTime = Time.time;
        }

        private void OnThrow(InputAction.CallbackContext _)
        {
            if (InputBlocked()) return;
            if (breadLauncher != null && breadLauncher.TryLaunch(rb.linearVelocity.x))
            {
                if (animator != null) animator.SetTrigger(HashThrow);
                if (sfx != null) sfx.PlayThrow();
            }
        }

        private void StartSlide()
        {
            isSliding = true;
            slideEndTime = Time.time + slideDuration;
            float newHeight = colSizeFull.y * slideHeightFactor;
            col.size = new Vector2(colSizeFull.x, newHeight);
            col.offset = new Vector2(colOffsetFull.x, colOffsetFull.y - (colSizeFull.y - newHeight) * 0.5f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            if (animator != null) animator.SetTrigger(HashSlide);
            if (sfx != null) sfx.PlaySlide();
        }

        private void EndSlide()
        {
            isSliding = false;
            col.size = colSizeFull;
            col.offset = colOffsetFull;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        private void SubscribeStateForGrace()
        {
            var gm = Kikiri9.Session.GameStateManager.Instance;
            if (gm != null) gm.OnStateChanged += HandleGraceOnStateChanged;
        }

        private void UnsubscribeStateForGrace()
        {
            var gm = Kikiri9.Session.GameStateManager.Instance;
            if (gm != null) gm.OnStateChanged -= HandleGraceOnStateChanged;
        }

        private void HandleGraceOnStateChanged(Kikiri9.Session.GameState next)
        {
            if (next == Kikiri9.Session.GameState.Playing && lastState != Kikiri9.Session.GameState.Playing)
                inputGraceEnd = Time.time + 0.25f;
            lastState = next;
        }
    }
}
