// Runtime: reproduce un StoryEffect como Image con swap de frames.
// No requiere Animator. Se auto-destruye al terminar si no es loop.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.UI.Story
{
    public class StoryEffectView : MonoBehaviour
    {
        private StoryEffect effect;
        private Image image;

        public static StoryEffectView Spawn(RectTransform parent, StoryEffect effect)
        {
            if (effect == null || effect.frames == null || effect.frames.Length == 0) return null;

            var go = new GameObject("Effect_" + (string.IsNullOrEmpty(effect.id) ? "fx" : effect.id),
                typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = effect.anchor;
            rt.anchorMax = effect.anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = effect.size;

            var img = go.GetComponent<Image>();
            img.sprite = effect.frames[0];
            img.color = effect.tint;
            img.raycastTarget = false;
            img.preserveAspect = true;

            var view = go.AddComponent<StoryEffectView>();
            view.effect = effect;
            view.image = img;
            view.StartCoroutine(view.Run());
            return view;
        }

        private IEnumerator Run()
        {
            if (effect.startDelay > 0f) yield return new WaitForSeconds(effect.startDelay);

            float interval = effect.frameRate > 0f ? 1f / effect.frameRate : 0.15f;

            do
            {
                for (int i = 0; i < effect.frames.Length; i++)
                {
                    if (image == null) yield break;
                    image.sprite = effect.frames[i];
                    yield return new WaitForSeconds(interval);
                }

                if (effect.randomRepeatInterval > 0f)
                {
                    float wait = Random.Range(effect.randomRepeatInterval * 0.5f, effect.randomRepeatInterval);
                    yield return new WaitForSeconds(wait);
                }
            }
            while (effect.loop);

            if (image != null) image.enabled = false;
        }
    }
}
