// Flujo narrativo por nivel.
//  - beforeLevel<n>: pantallas que se muestran al elegir ese nivel por primera vez.
//  - endingScreens: epilogo al ganar el ultimo nivel, antes de los creditos.
//  - fullReplay: usada por el boton "Ver historia" para reproducir TODO en orden.

using System.Collections.Generic;
using UnityEngine;

namespace Kikiri9.UI.Story
{
    [CreateAssetMenu(fileName = "StoryFlow", menuName = "Kikiri-9/Story/Flow")]
    public class StoryFlow : ScriptableObject
    {
        public const string ResourcesPath = "Story/StoryFlow";

        [Tooltip("Pantallas antes del Nivel 1 (setup inicial).")]
        public StoryScreenDefinition[] beforeLevel1;

        [Tooltip("Pantalla puente al pasar al Nivel 2.")]
        public StoryScreenDefinition[] beforeLevel2;

        [Tooltip("Pantallas antes del Nivel 3 (viaje a Guanajuato).")]
        public StoryScreenDefinition[] beforeLevel3;

        [Tooltip("Pantallas antes del Nivel 4 (la Coronela).")]
        public StoryScreenDefinition[] beforeLevel4;

        [Tooltip("Epilogo al ganar el Nivel 4.")]
        public StoryScreenDefinition[] endingScreens;

        public string creditsSceneName = "CreditsScene";

        private static StoryFlow cached;

        public static StoryFlow Get()
        {
            if (cached != null) return cached;
            cached = Resources.Load<StoryFlow>(ResourcesPath);
            return cached;
        }

        public StoryScreenDefinition[] GetBeforeLevel(int levelNumber)
        {
            switch (levelNumber)
            {
                case 1: return beforeLevel1;
                case 2: return beforeLevel2;
                case 3: return beforeLevel3;
                case 4: return beforeLevel4;
                default: return null;
            }
        }

        public StoryScreenDefinition[] GetFullStory()
        {
            var list = new List<StoryScreenDefinition>();
            if (beforeLevel1 != null) list.AddRange(beforeLevel1);
            if (beforeLevel2 != null) list.AddRange(beforeLevel2);
            if (beforeLevel3 != null) list.AddRange(beforeLevel3);
            if (beforeLevel4 != null) list.AddRange(beforeLevel4);
            if (endingScreens != null) list.AddRange(endingScreens);
            return list.ToArray();
        }
    }
}
