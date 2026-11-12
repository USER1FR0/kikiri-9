// Mariachi fantasma: vuela a altura del salto del pollo. Se coloca al spawn en una Y
// fija de vuelo (no respeta la groundY del spawner). Se mueve a la izquierda en
// trayectoria senoidal suave. Vulnerable solo a pan en el aire. Al tocar al pollo = dano.

using UnityEngine;

namespace Kikiri9.Enemies.New
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyHealth))]
    public class MariachiGhost : MonoBehaviour
    {
        [SerializeField] private float speedX = -2.4f;
        [SerializeField] private float amplitude = 0.5f;
        [SerializeField] private float frequency = 1.3f;
        [Tooltip("Altura mundial fija a la que vuela el mariachi (arco de salto del pollo).")]
        [SerializeField] private float flightY = -1.8f;
        [SerializeField] private float despawnDistanceBehind = 20f;

        [Header("Audio")]
        [SerializeField] private AudioClip trumpetSfx;
        [SerializeField] private Vector2 trumpetIntervalRange = new Vector2(2.5f, 4.5f);

        private Rigidbody2D rb;
        private Transform camT;
        private float phase;
        private float nextTrumpetTime;
        private AudioSource audioSrc;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.useFullKinematicContacts = true;
            if (Camera.main != null) camT = Camera.main.transform;
            phase = Random.value * Mathf.PI * 2f;

            audioSrc = GetComponent<AudioSource>();
            if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            audioSrc.spatialBlend = 0f;
            nextTrumpetTime = Time.time + Random.Range(trumpetIntervalRange.x, trumpetIntervalRange.y);
        }

        private void Start()
        {
            // Forzamos la altura de vuelo al spawn (el spawner usa groundY).
            var p = transform.position;
            p.y = flightY;
            transform.position = p;
        }

        private void FixedUpdate()
        {
            float y = flightY + Mathf.Sin(Time.time * frequency + phase) * amplitude;
            float x = transform.position.x + speedX * Time.fixedDeltaTime;
            transform.position = new Vector3(x, y, transform.position.z);
        }

        private void Update()
        {
            if (trumpetSfx != null && Time.time >= nextTrumpetTime)
            {
                audioSrc.PlayOneShot(trumpetSfx);
                nextTrumpetTime = Time.time + Random.Range(trumpetIntervalRange.x, trumpetIntervalRange.y);
            }
            if (camT == null) return;
            if (transform.position.x < camT.position.x - despawnDistanceBehind)
                Destroy(gameObject);
        }
    }
}
