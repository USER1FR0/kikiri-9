// Flash blanco corto al recibir daño. Se engancha via TakeDamage(1) desde Bread.
// Requiere SpriteRenderer; no cambia la logica existente de EnemyHealth.

using System.Collections;
using UnityEngine;

namespace Kikiri9.Enemies
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemyDamageFlash : MonoBehaviour
    {
        [SerializeField] private float flashDuration = 0.08f;
        [SerializeField] private Color flashColor = Color.white;

        private SpriteRenderer sr;
        private Color originalColor;
        private Coroutine routine;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            originalColor = sr.color;
        }

        public void Flash()
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            sr.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            sr.color = originalColor;
            routine = null;
        }
    }
}
