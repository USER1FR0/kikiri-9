// Dev: borra el progreso de niveles (desbloqueo, best score, botellas doradas)
// para poder probar flujos completos desde el nivel 1.
// Tools > Kikiri-9 > Reset Progreso Niveles

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class ProgressResetter
    {
        [MenuItem("Tools/Kikiri-9/Reset Progreso Niveles")]
        public static void Reset()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "save.json");
            bool deleted = false;
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                deleted = true;
            }

            // Limpiar cualquier flag de progreso por si existe en PlayerPrefs
            PlayerPrefs.DeleteKey("kikiri9_highest_unlocked_level");
            for (int i = 1; i <= 4; i++) PlayerPrefs.DeleteKey("kikiri9_level_completed_" + i);
            PlayerPrefs.Save();

            EditorUtility.DisplayDialog("Kikiri-9",
                deleted
                    ? "Progreso reseteado. Solo el Nivel 1 queda disponible.\n\n" + savePath
                    : "No habia save.json en disco; progreso ya estaba limpio.",
                "OK");
        }
    }
}
