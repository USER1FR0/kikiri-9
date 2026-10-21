// Builds AnimationClips and an AnimatorController for the momia from the already-sliced
// sprite sheet. Produces Momia_Walk (caminar) and Momia_Defeat (derrota), plus a controller
// with a single Defeat trigger used by EnemyHealth. Same approach as PlayerAnimationBuilder.
//
// Run from menu: Tools > Kikiri-9 > Build Enemy Animations.
// The controller is created at Assets/_Project/Animations/Momia.controller.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class EnemyAnimationBuilder
    {
        private const string MomiaSheet = "Assets/_Draft_2_Kikiri9/Art/Characters/momia.png";
        private const string AnimDir = "Assets/_Project/Animations";
        private const string ControllerPath = AnimDir + "/Momia.controller";

        [MenuItem("Tools/Kikiri-9/Build Enemy Animations")]
        public static void Build()
        {
            System.IO.Directory.CreateDirectory(AnimDir);

            var walkClip = CreateClip("Momia_Walk", MomiaSheet, "caminar", 10, loop: true);
            var defeatClip = CreateClip("Momia_Defeat", MomiaSheet, "derrota", 8, loop: false);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Defeat", AnimatorControllerParameterType.Trigger);

            var sm = controller.layers[0].stateMachine;

            var walkState = sm.AddState("Walk");
            walkState.motion = walkClip;
            var defeatState = sm.AddState("Defeat");
            defeatState.motion = defeatClip;

            sm.defaultState = walkState;

            // Walk -> Defeat when Defeat trigger, immediate.
            var tWalkDefeat = walkState.AddTransition(defeatState);
            tWalkDefeat.hasExitTime = false;
            tWalkDefeat.duration = 0f;
            tWalkDefeat.AddCondition(AnimatorConditionMode.If, 0, "Defeat");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Kikiri-9] Momia animator controller built at {ControllerPath}");
        }

        private static AnimationClip CreateClip(string clipName, string sheetPath, string rowPrefix, int fps, bool loop)
        {
            var sprites = LoadRowSprites(sheetPath, rowPrefix);
            if (sprites.Count == 0)
            {
                Debug.LogWarning($"[Kikiri-9] No sprites '{rowPrefix}_*' in {sheetPath}");
                return null;
            }

            var clip = new AnimationClip { frameRate = fps };
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
            {
                keys[i] = new ObjectReferenceKeyframe
                {
                    time = i / (float)fps,
                    value = sprites[i]
                };
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var assetPath = $"{AnimDir}/{clipName}.anim";
            AssetDatabase.CreateAsset(clip, assetPath);
            return clip;
        }

        private static List<Sprite> LoadRowSprites(string sheetPath, string rowPrefix)
        {
            var list = new List<Sprite>();
            var reps = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheetPath);
            foreach (var o in reps)
            {
                if (o is Sprite s && s.name.StartsWith(rowPrefix + "_"))
                    list.Add(s);
            }
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return list;
        }
    }
}
