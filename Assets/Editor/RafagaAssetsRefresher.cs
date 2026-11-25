// Solo refresca los sprites de la rafaga (boton listo, pulsado, mascara, estela)
// en las escenas L1/L2/L3. NO reconstruye RafagaCanvas, NO toca posiciones,
// anchors, tamanos, HUD ni ningun otro Canvas. Idempotente.
// Util cuando se actualiza el arte en Art/PowerUps/Rafaga/ y solo queremos
// re-referenciar en las escenas sin volver a correr PowerUpsInstaller.Install.

using Kikiri9.Player;
using Kikiri9.Visuales;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Kikiri9.EditorTools
{
    public static class RafagaAssetsRefresher
    {
        private const string RafagaDir = "Assets/_Project/Art/PowerUps/Rafaga";
        private static readonly string[] Scenes =
        {
            "Assets/_Project/Scenes/Level01_Dolores.unity",
            "Assets/_Project/Scenes/Level02_Dolores.unity",
            "Assets/_Project/Scenes/Level03_Guanajuato.unity",
        };

        [MenuItem("Tools/Kikiri-9/Rafaga - Refresh Assets Only")]
        public static void RefreshRafagaAssets()
        {
            var listo = AssetDatabase.LoadAssetAtPath<Sprite>($"{RafagaDir}/boton_listo.png");
            var pulsado = AssetDatabase.LoadAssetAtPath<Sprite>($"{RafagaDir}/boton_pulsado.png");
            var mascara = AssetDatabase.LoadAssetAtPath<Sprite>($"{RafagaDir}/mascara_recarga.png");
            var estela = AssetDatabase.LoadAssetAtPath<Sprite>($"{RafagaDir}/estela_derecha.png");

            int totalUpdates = 0;
            foreach (var scenePath in Scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int updates = 0;

                // 1) RafagaCanvas: Button Image, Mask Image, KikiriRafagaVisual sprites
                var rafagaCanvas = GameObject.Find("RafagaCanvas");
                if (rafagaCanvas != null)
                {
                    var button = rafagaCanvas.transform.Find("RafagaButton");
                    if (button != null)
                    {
                        var btnImg = button.GetComponent<Image>();
                        if (btnImg != null && listo != null)
                        {
                            btnImg.sprite = listo;
                            updates++;
                        }
                        var mask = button.Find("Recarga");
                        if (mask != null)
                        {
                            var maskImg = mask.GetComponent<Image>();
                            if (maskImg != null && mascara != null)
                            {
                                maskImg.sprite = mascara;
                                updates++;
                            }
                        }
                        var visual = button.GetComponent<KikiriRafagaVisual>();
                        if (visual != null)
                        {
                            SetField(visual, "listo", listo);
                            SetField(visual, "pulsado", pulsado);
                            updates += 2;
                        }
                    }
                }

                // 2) Estela del pollo: SpriteRenderer en hijo "Estela" (crear si falta)
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var estelaTrans = player.transform.Find("Estela");
                    if (estelaTrans == null) estelaTrans = player.transform.Find("estela");
                    if (estelaTrans == null) estelaTrans = player.transform.Find("EstelaRafaga");
                    if (estelaTrans == null && estela != null)
                    {
                        // Crear el hijo Estela
                        var newEstela = new GameObject("Estela");
                        newEstela.transform.SetParent(player.transform, false);
                        newEstela.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                        newEstela.transform.localScale = Vector3.one * 0.035f;
                        var newSr = newEstela.AddComponent<SpriteRenderer>();
                        newSr.sprite = estela;
                        newSr.color = new Color(1f, 0.75f, 0.25f, 1f);
                        var polloSr = player.GetComponent<SpriteRenderer>();
                        newSr.sortingOrder = (polloSr != null ? polloSr.sortingOrder : 5) + 1;
                        newEstela.SetActive(false);
                        estelaTrans = newEstela.transform;
                        updates++;
                    }
                    var estelaGo = estelaTrans;
                    if (estelaGo != null && estela != null)
                    {
                        var sr = estelaGo.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.sprite = estela;
                            updates++;
                        }
                    }
                    // Tambien refrescar la referencia en EstelaRafagaDriver si fuera null
                    var driver = player.GetComponent<EstelaRafagaDriver>();
                    if (driver != null && estelaGo != null)
                    {
                        var so = new SerializedObject(driver);
                        var p = so.FindProperty("estela");
                        if (p != null && p.objectReferenceValue == null)
                        {
                            p.objectReferenceValue = estelaGo.gameObject;
                            so.ApplyModifiedProperties();
                            updates++;
                        }
                    }
                    // PlayerPunch.estelaFallbackSprite para VFX garantizado
                    var punch = player.GetComponent<Kikiri9.Player.PlayerPunch>();
                    if (punch != null && estela != null)
                    {
                        var so = new SerializedObject(punch);
                        var p = so.FindProperty("estelaFallbackSprite");
                        if (p != null)
                        {
                            p.objectReferenceValue = estela;
                            so.ApplyModifiedProperties();
                            updates++;
                        }
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, scenePath);
                Debug.Log($"[Kikiri-9] {scenePath}: {updates} refs de rafaga actualizadas.");
                totalUpdates += updates;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Kikiri-9] Rafaga assets refresh terminado ({totalUpdates} refs totales).");
        }

        private static void SetField(Object target, string field, Object value)
        {
            if (target == null || value == null) return;
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p != null) { p.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        }
    }
}
