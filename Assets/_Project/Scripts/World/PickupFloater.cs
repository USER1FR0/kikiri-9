// Visual flair for pickups: gentle vertical oscillation plus a soft pulse of scale,
// so coins and bottles read as collectibles instead of blending with static props.

using UnityEngine;

namespace Kikiri9.World
{
    public class PickupFloater : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.18f;
        [SerializeField] private float frequency = 1.6f;
        [SerializeField] private float scalePulse = 0.06f;

        private Vector3 basePos;
        private Vector3 baseScale;
        private float phase;

        private void Start()
        {
            basePos = transform.position;
            baseScale = transform.localScale;
            phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            float t = Time.time * frequency + phase;
            float y = Mathf.Sin(t) * amplitude;
            // Keep X in sync with current position (ObstacleScroller doesn't move it, but future-safe)
            var p = transform.position;
            p.y = basePos.y + y;
            transform.position = p;

            float sp = 1f + Mathf.Sin(t * 2f) * scalePulse;
            transform.localScale = baseScale * sp;
        }
    }
}
