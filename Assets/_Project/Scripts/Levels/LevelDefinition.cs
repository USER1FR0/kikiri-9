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
        public float maxSpeed = 6f;
        public float targetDistance = 500f;

        [Header("Spawn aleatorio")]
        public GameObject[] enemyPrefabs;
        public GameObject[] obstaclePrefabs;
        public GameObject[] pickupPrefabs;
        public Vector2 spawnIntervalRange = new Vector2(2f, 3.5f);
        public int maxActiveEntities = 5;

        [Header("Tutorial")]
        [TextArea(3, 6)] public string tutorialText = "ESPACIO / W / Flecha arriba: Saltar\nS / Flecha abajo / Shift: Deslizar\nJ / Click: Lanzar pan\n\nMeta: 500 metros sin perder los 3 corazones.";

        [Header("Dificultad / variacion")]
        [Range(0f, 1f)] public float pickupHighChance = 0f;
        public float pickupHighYBonus = 2.2f;
        public float minSpawnSeparation = 7f;
        public int maxJumpsInAir = 2;
        public float breadLowRatio = 0f;
        public float enemyWeight = 0f;
        public float obstacleWeight = 0f;
        public float pickupWeight = 0f;
        public float safeStartMeters = 0f;    // primeros N metros sin obstaculos/enemigos

        [Header("Narrativa final")]
        [TextArea(3, 6)] public string endingNarrative = "Dolores quedo atras. El pollo sigue su camino hacia Guanajuato.";
        public int goldBottlesReward = 1;
    }
}
