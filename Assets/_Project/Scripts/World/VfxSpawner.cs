// Helper to spawn a one-shot visual effect sprite at a position, that fades out and
// self-destroys after a short lifetime. No sound, no collider.

using UnityEngine;

namespace Kikiri9.World
{
    public static class VfxSpawner
    {
        public static void Spawn(Sprite sprite, Vector3 position, float lifetime = 0.4f, float scale = 0.5f, int sortingOrder = 18)
        {
            if (sprite == null) return;
            var go = new GameObject("Vfx_" + sprite.name);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            Object.Destroy(go, lifetime);
        }
    }
}
