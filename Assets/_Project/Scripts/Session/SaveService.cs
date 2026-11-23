// Persistencia de progreso en JSON dentro de Application.persistentDataPath. En WebGL,
// Unity respalda esa ruta automaticamente con IndexedDB, asi que no se necesita codigo
// extra para que el guardado sobreviva a cerrar la pestana.
//
// Si el archivo no existe o esta corrupto (JSON invalido o con forma inesperada), se usan
// valores por defecto en vez de tronar.

using System;
using System.IO;
using UnityEngine;

namespace Kikiri9.Session
{
    [Serializable]
    public class SaveData
    {
        public int schemaVersion = SaveService.CurrentSchemaVersion;
        public int highestUnlockedLevel = 1;
        public int[] bestScore = new int[SaveService.LevelCount];
        public bool[] goldBottleWon = new bool[SaveService.LevelCount];
        public int totalGoldBottles = 0;
    }

    public static class SaveService
    {
        public const int CurrentSchemaVersion = 1;
        public const int LevelCount = 4;

        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

        private static SaveData cached;

        public static SaveData Load()
        {
            if (cached != null) return cached;

            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    var data = JsonUtility.FromJson<SaveData>(json);
                    if (data != null && IsValid(data))
                    {
                        cached = data;
                        return cached;
                    }

                    Debug.LogWarning("SaveService: save.json invalido o con forma inesperada, usando valores por defecto.");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveService: no se pudo leer save.json ({e.Message}), usando valores por defecto.");
            }

            cached = CreateDefault();
            return cached;
        }

        public static void SaveNow()
        {
            if (cached == null) return;

            try
            {
                string json = JsonUtility.ToJson(cached, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveService: no se pudo guardar save.json ({e.Message}).");
            }
        }

        // Llamado al completar un nivel (levelNumber es 1-based). Desbloquea el siguiente
        // nivel, actualiza el mejor puntaje y otorga la botella dorada del nivel solo la
        // primera vez que se gana (goldBottleReward es LevelDefinition.goldBottlesReward,
        // 0 si ese nivel no otorga ninguna).
        public static void ReportLevelCompleted(int levelNumber, int score, int goldBottleReward)
        {
            var data = Load();
            int index = levelNumber - 1;
            if (index < 0 || index >= LevelCount) return;

            if (score > data.bestScore[index]) data.bestScore[index] = score;

            if (goldBottleReward > 0 && !data.goldBottleWon[index])
            {
                data.goldBottleWon[index] = true;
                data.totalGoldBottles += goldBottleReward;
            }

            int nextLevel = levelNumber + 1;
            if (nextLevel > data.highestUnlockedLevel && nextLevel <= LevelCount)
                data.highestUnlockedLevel = nextLevel;

            SaveNow();
        }

        public static string SceneNameForLevel(int levelNumber)
        {
            switch (levelNumber)
            {
                case 1: return "Level01_Dolores";
                case 2: return "Level02_Dolores";
                case 3: return "Level03_Guanajuato";
                case 4: return "Level04_Guanajuato"; // aun no existe (#14)
                default: return null;
            }
        }

        public static int LevelNumberForScene(string sceneName)
        {
            for (int i = 1; i <= LevelCount; i++)
                if (SceneNameForLevel(i) == sceneName) return i;
            return -1;
        }

        private static bool IsValid(SaveData data)
        {
            return data.bestScore != null && data.bestScore.Length == LevelCount
                && data.goldBottleWon != null && data.goldBottleWon.Length == LevelCount
                && data.highestUnlockedLevel >= 1 && data.highestUnlockedLevel <= LevelCount;
        }

        private static SaveData CreateDefault()
        {
            return new SaveData
            {
                schemaVersion = CurrentSchemaVersion,
                highestUnlockedLevel = 1,
                bestScore = new int[LevelCount],
                goldBottleWon = new bool[LevelCount],
                totalGoldBottles = 0
            };
        }
    }
}
