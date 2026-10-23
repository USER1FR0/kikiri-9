// Builds AnimationClips and an AnimatorController for Kikiri-9 from the already-sliced
// sprite sheets. Produces: Kikiri9_Run, Kikiri9_Jump, Kikiri9_Slide, Kikiri9_Throw,
// Kikiri9_Idle (reposo), Kikiri9_Hit (dano) and Kikiri9_Death (derrota), plus a controller
// with transitions driven by PlayerController's parameters.
//
// Run from menu: Tools > Kikiri-9 > Build Player Animations.
// The controller is created at Assets/_Project/Animations/Kikiri9.controller.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Kikiri9.EditorTools
{
    public static class PlayerAnimationBuilder
    {
        private const string MainSheet = "Assets/_Draft_2_Kikiri9/Art/Characters/kikiri9.png";
        private const string ExtraSheet = "Assets/_Draft_2_Kikiri9/Art/Characters/kikiri9_reposo_dano_derrota.png";
        private const string AnimDir = "Assets/_Project/Animations";
        private const string ControllerPath = AnimDir + "/Kikiri9.controller";

        [MenuItem("Tools/Kikiri-9/Build Player Animations")]
        public static void Build()
        {
            System.IO.Directory.CreateDirectory(AnimDir);

            var runClip = CreateClip("Kikiri9_Run", MainSheet, "correr", 14, loop: true);
            var jumpClip = CreateClip("Kikiri9_Jump", MainSheet, "saltar", 8, loop: true);
            var slideClip = CreateClip("Kikiri9_Slide", MainSheet, "deslizar", 8, loop: true);
            var throwClip = CreateClip("Kikiri9_Throw", MainSheet, "lanzar", 14, loop: false);
            var idleClip = CreateClip("Kikiri9_Idle", ExtraSheet, "reposo", 8, loop: true);
            var hitClip = CreateClip("Kikiri9_Hit", ExtraSheet, "dano", 10, loop: false);
            var dieClip = CreateClip("Kikiri9_Death", ExtraSheet, "derrota", 8, loop: false);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsSliding", AnimatorControllerParameterType.Bool);
            controller.AddParameter("SpeedY", AnimatorControllerParameterType.Float);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Slide", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Throw", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Trigger);

            var sm = controller.layers[0].stateMachine;

            var runState = sm.AddState("Run");
            runState.motion = runClip;
            var jumpState = sm.AddState("Jump");
            jumpState.motion = jumpClip;
            var slideState = sm.AddState("Slide");
            slideState.motion = slideClip;
            var throwState = sm.AddState("Throw");
            throwState.motion = throwClip;
            var hitState = sm.AddState("Hit");
            hitState.motion = hitClip;
            var deathState = sm.AddState("Death");
            deathState.motion = dieClip;

            sm.defaultState = runState;

            // Run -> Jump when Jump trigger
            var tRunJump = runState.AddTransition(jumpState);
            tRunJump.hasExitTime = false;
            tRunJump.duration = 0f;
            tRunJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");

            // Jump -> Run when grounded and falling
            var tJumpRun = jumpState.AddTransition(runState);
            tJumpRun.hasExitTime = false;
            tJumpRun.duration = 0.05f;
            tJumpRun.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            tJumpRun.AddCondition(AnimatorConditionMode.Less, 0.01f, "SpeedY");

            // Run -> Slide when Slide trigger
            var tRunSlide = runState.AddTransition(slideState);
            tRunSlide.hasExitTime = false;
            tRunSlide.duration = 0f;
            tRunSlide.AddCondition(AnimatorConditionMode.If, 0, "Slide");

            // Slide -> Run when IsSliding becomes false
            var tSlideRun = slideState.AddTransition(runState);
            tSlideRun.hasExitTime = false;
            tSlideRun.duration = 0.05f;
            tSlideRun.AddCondition(AnimatorConditionMode.IfNot, 0, "IsSliding");

            // Run -> Throw when Throw trigger
            var tRunThrow = runState.AddTransition(throwState);
            tRunThrow.hasExitTime = false;
            tRunThrow.duration = 0f;
            tRunThrow.AddCondition(AnimatorConditionMode.If, 0, "Throw");

            // Throw -> Run on exit time (full clip)
            var tThrowRun = throwState.AddTransition(runState);
            tThrowRun.hasExitTime = true;
            tThrowRun.exitTime = 0.9f;
            tThrowRun.duration = 0.05f;

            // Any -> Hit when Hit trigger (ignored when already in Death)
            var tAnyHit = sm.AddAnyStateTransition(hitState);
            tAnyHit.hasExitTime = false;
            tAnyHit.duration = 0f;
            tAnyHit.canTransitionToSelf = false;
            tAnyHit.AddCondition(AnimatorConditionMode.If, 0f, "Hit");

            // Hit -> Run on exit time
            var tHitRun = hitState.AddTransition(runState);
            tHitRun.hasExitTime = true;
            tHitRun.exitTime = 0.9f;
            tHitRun.duration = 0.05f;

            // Any -> Death when Dead trigger
            var tAnyDeath = sm.AddAnyStateTransition(deathState);
            tAnyDeath.hasExitTime = false;
            tAnyDeath.duration = 0f;
            tAnyDeath.canTransitionToSelf = false;
            tAnyDeath.AddCondition(AnimatorConditionMode.If, 0f, "Dead");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Kikiri-9] Animator controller built at {ControllerPath}");
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
