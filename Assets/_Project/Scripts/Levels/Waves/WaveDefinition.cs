// Describe una oleada: metrage en que se dispara, mix de enemigos, rewards al terminar.

using UnityEngine;

namespace Kikiri9.Levels.Waves
{
    [System.Serializable]
    public class WaveEntry
    {
        public GameObject enemyPrefab;
        public int count = 1;
    }

    [System.Serializable]
    public class WaveReward
    {
        public GameObject prefab;
        public int count = 1;
    }

    [CreateAssetMenu(fileName = "WaveDefinition", menuName = "Kikiri9/Wave Definition")]
    public class WaveDefinition : ScriptableObject
    {
        public float triggerMeters = 400f;
        public string bannerLabel = "OLEADA 1/3";
        public float spawnInterval = 0.8f;
        public float waveTimeLimit = 60f;
        public WaveEntry[] entries;
        public WaveReward[] rewards;
        public bool endsLevelOnClear = false;
        public bool groupSpawn = false; // true = spawn por grupos (todos de un tipo juntos)
        public bool freeMovementMode = false; // true = congela mundo + libertad A/D (estilo Metal Slug)
    }
}
