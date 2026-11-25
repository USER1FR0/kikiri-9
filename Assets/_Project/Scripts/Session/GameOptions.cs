// Ajustes de audio y calidad del jugador, persistidos en PlayerPrefs. No existe un
// AudioMixer en el proyecto, asi que musica y SFX se controlan como multiplicadores
// simples que MusicController, PlayerSfx y EnemyHealth aplican al reproducir.
// Estatico y sin dependencias de escena: funciona igual en MainMenu que en niveles.

using UnityEngine;

namespace Kikiri9.Session
{
    public static class GameOptions
    {
        private const string MusicKey = "opt_music_vol";
        private const string SfxKey = "opt_sfx_vol";
        private const string QualityKey = "opt_quality";
        private const string ControlsKey = "opt_show_controls";

        public static float MusicVolume { get; private set; }
        public static float SfxVolume { get; private set; }
        public static int QualityLevel { get; private set; } // 0 Baja, 1 Media, 2 Alta
        public static bool ShowControls { get; private set; }

        // MusicController se suscribe para aplicar el volumen en caliente mientras suena.
        public static event System.Action OnMusicVolumeChanged;

        static GameOptions()
        {
            MusicVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
            SfxVolume = PlayerPrefs.GetFloat(SfxKey, 1f);
            QualityLevel = PlayerPrefs.GetInt(QualityKey, 1);
            ShowControls = PlayerPrefs.GetInt(ControlsKey, 1) == 1;
            ApplyQuality();
        }

        public static void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, MusicVolume);
            OnMusicVolumeChanged?.Invoke();
        }

        public static void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxKey, SfxVolume);
        }

        public static void SetQualityLevel(int level)
        {
            QualityLevel = Mathf.Clamp(level, 0, 2);
            PlayerPrefs.SetInt(QualityKey, QualityLevel);
            ApplyQuality();
        }

        public static void SetShowControls(bool show)
        {
            ShowControls = show;
            PlayerPrefs.SetInt(ControlsKey, show ? 1 : 0);
        }

        private static void ApplyQuality()
        {
            var names = QualitySettings.names;
            if (names == null || names.Length == 0) return;
            int index = QualityLevel == 0 ? 0 : QualityLevel == 2 ? names.Length - 1 : names.Length / 2;
            QualitySettings.SetQualityLevel(Mathf.Clamp(index, 0, names.Length - 1), true);
        }
    }
}
