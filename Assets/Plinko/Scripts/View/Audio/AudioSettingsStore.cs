using UnityEngine;

namespace Plinko.Scripts.View.Audio
{
    public static class AudioSettingsStore
    {
        private const string VolumeKey = "plinko.audio.volume";
        private const string MutedKey = "plinko.audio.muted";

        public static float Volume => PlayerPrefs.GetFloat(VolumeKey, 0.85f);
        public static bool IsMuted => PlayerPrefs.GetInt(MutedKey, 0) != 0;

        public static void Apply()
        {
            AudioListener.volume = IsMuted ? 0f : Volume;
        }

        public static void SetMuted(bool value)
        {
            PlayerPrefs.SetInt(MutedKey, value ? 1 : 0);
            PlayerPrefs.Save();
            Apply();
        }

        public static void SetVolume(float value)
        {
            PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            Apply();
        }
    }
}
