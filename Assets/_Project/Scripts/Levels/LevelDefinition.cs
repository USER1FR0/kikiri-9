// Per-level configuration. Create instances via Assets/Create/Kikiri9/Level Definition.

using UnityEngine;

namespace Kikiri9.Levels
{
    [CreateAssetMenu(fileName = "LevelDefinition", menuName = "Kikiri9/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        public string levelName = "Dolores Hidalgo";
        public Sprite backgroundSprite;
        public AudioClip musicClip;

        [Header("Carrera")]
        public float worldSpeed = 5f;
        public float maxSpeed = 6f; // velocidad al llegar a targetDistance
        public float targetDistance = 500f;

        [Header("Spawn aleatorio")]
        public GameObject[] enemyPrefabs;
        public GameObject[] obstaclePrefabs;
        public GameObject[] pickupPrefabs;
        public Vector2 spawnIntervalRange = new Vector2(2f, 3.5f);
        public int maxActiveEntities = 5;

        [Header("Tutorial")]
        [TextArea(3, 6)] public string tutorialText = "ESPACIO / W / Flecha arriba: Saltar\nS / Flecha abajo / Shift: Deslizar\nJ / Click: Lanzar pan\n\nMeta: 500 metros sin perder los 3 corazones.";

        [Header("Narrativa final")]
        [TextArea(3, 6)] public string endingNarrative = "Dolores quedo atras. El pollo sigue su camino hacia Guanajuato.";
        public int goldBottlesReward = 1;
    }
}
