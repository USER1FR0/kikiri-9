// Referencias a los sprites de boton y font asset que la narrativa usa en runtime.
// Lo construye el installer (StoryContentBootstrap). Se carga por Resources.Load.

using TMPro;
using UnityEngine;

namespace Kikiri9.UI.Story
{
    [CreateAssetMenu(fileName = "StoryUIAssets", menuName = "Kikiri-9/Story/UI Assets")]
    public class StoryUIAssets : ScriptableObject
    {
        public const string ResourcesPath = "Story/StoryUIAssets";

        public Sprite buttonNormal;
        public Sprite buttonPressed;
        public Sprite buttonSelected;
        public Sprite buttonDisabled;
        public TMP_FontAsset font;

        private static StoryUIAssets cached;
        public static StoryUIAssets Get()
        {
            if (cached != null) return cached;
            cached = Resources.Load<StoryUIAssets>(ResourcesPath);
            return cached;
        }
    }
}
