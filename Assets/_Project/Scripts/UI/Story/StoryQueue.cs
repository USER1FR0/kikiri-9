// Cola estatica de pantallas narrativas. StoryScene la consume.
// Flags "kikiri9_story_before_L<n>" por nivel.

using System.Collections.Generic;
using Kikiri9.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kikiri9.UI.Story
{
    public static class StoryQueue
    {
        public const string LevelFlagPrefix = "kikiri9_story_before_L";

        public static readonly Queue<StoryScreenDefinition> Pending = new Queue<StoryScreenDefinition>();
        public static string NextSceneName;
        public static int PendingLevelToMarkSeen = -1;

        public static bool TryEnqueueBeforeLevel(int levelNumber, string targetSceneName, bool force = false)
        {
            if (!force && HasSeenBeforeLevel(levelNumber)) return false;
            var flow = StoryFlow.Get();
            if (flow == null) return false;
            var screens = flow.GetBeforeLevel(levelNumber);
            if (screens == null || screens.Length == 0) return false;

            Pending.Clear();
            foreach (var s in screens) if (s != null) Pending.Enqueue(s);
            NextSceneName = targetSceneName;
            PendingLevelToMarkSeen = levelNumber;
            return Pending.Count > 0;
        }

        public static bool TryEnqueueEnding(string targetSceneName)
        {
            var flow = StoryFlow.Get();
            if (flow == null || flow.endingScreens == null || flow.endingScreens.Length == 0) return false;

            Pending.Clear();
            foreach (var s in flow.endingScreens) if (s != null) Pending.Enqueue(s);
            NextSceneName = targetSceneName;
            PendingLevelToMarkSeen = -1;
            return Pending.Count > 0;
        }

        // Boton "Ver historia": reproduce todo en orden, sin marcar flags, y vuelve al selector.
        public static bool TryEnqueueFullReplay(string targetSceneName)
        {
            var flow = StoryFlow.Get();
            if (flow == null) return false;
            var all = flow.GetFullStory();
            if (all == null || all.Length == 0) return false;

            Pending.Clear();
            foreach (var s in all) if (s != null) Pending.Enqueue(s);
            NextSceneName = targetSceneName;
            PendingLevelToMarkSeen = -1;
            return Pending.Count > 0;
        }

        public static void MarkBeforeLevelSeen(int levelNumber)
        {
            if (levelNumber <= 0) return;
            PlayerPrefs.SetInt(LevelFlagPrefix + levelNumber, 1);
            PlayerPrefs.Save();
        }

        public static bool HasSeenBeforeLevel(int levelNumber)
        {
            if (levelNumber <= 0) return true;
            return PlayerPrefs.GetInt(LevelFlagPrefix + levelNumber, 0) == 1;
        }

        public static void ResetAll()
        {
            for (int i = 1; i <= 4; i++) PlayerPrefs.DeleteKey(LevelFlagPrefix + i);
            PlayerPrefs.DeleteKey("kikiri9_main_story_seen");       // legacy
            for (int i = 1; i <= 4; i++) PlayerPrefs.DeleteKey("kikiri9_intro_seen_" + i); // legacy
            PlayerPrefs.Save();
        }

        public static void LoadStoryScene()
        {
            if (Pending.Count == 0) return;
            if (SceneFader.Instance != null && Application.CanStreamedLevelBeLoaded("StoryScene"))
                SceneFader.Instance.LoadSceneWithFade("StoryScene");
            else if (Application.CanStreamedLevelBeLoaded("StoryScene"))
                SceneManager.LoadScene("StoryScene");
        }
    }
}
