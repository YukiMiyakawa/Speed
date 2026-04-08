using UnityEngine;

namespace Speed.Controllers
{
    public static class SettingsManager
    {
        private const string KeyDifficulty = "CpuDifficulty";
        private const string KeySound      = "SoundOn";
        private const string KeyVibrate    = "VibrateOn";

        public static int CpuDifficulty
        {
            get => PlayerPrefs.GetInt(KeyDifficulty, 3);
            set { PlayerPrefs.SetInt(KeyDifficulty, Mathf.Clamp(value, 1, 5)); PlayerPrefs.Save(); }
        }

        public static bool SoundOn
        {
            get => PlayerPrefs.GetInt(KeySound, 1) == 1;
            set { PlayerPrefs.SetInt(KeySound, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool VibrateOn
        {
            get => PlayerPrefs.GetInt(KeyVibrate, 1) == 1;
            set { PlayerPrefs.SetInt(KeyVibrate, value ? 1 : 0); PlayerPrefs.Save(); }
        }
    }
}
