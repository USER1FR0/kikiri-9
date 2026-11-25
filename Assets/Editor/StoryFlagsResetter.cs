// Herramienta de dev: borra los PlayerPrefs que marcan narrativas vistas.
// Tools > Kikiri-9 > Reset Story Flags

using Kikiri9.UI.Story;
using UnityEditor;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class StoryFlagsResetter
    {
        [MenuItem("Tools/Kikiri-9/Reset Story Flags")]
        public static void Reset()
        {
            StoryQueue.ResetAll();
            EditorUtility.DisplayDialog("Kikiri-9",
                "Flags de historia reseteados. La proxima vez que entres a cada nivel se reproducira su narrativa.",
                "OK");
        }
    }
}
