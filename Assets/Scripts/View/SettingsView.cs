using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Speed.Controllers;

namespace Speed.View
{
    public class SettingsView : MonoBehaviour
    {
        [SerializeField] private Slider          _difficultySlider;
        [SerializeField] private TextMeshProUGUI _difficultyLabel;
        [SerializeField] private Toggle          _soundToggle;
        [SerializeField] private Toggle          _vibrateToggle;
        [SerializeField] private Button          _closeButton;

        private void Awake()
        {
            _difficultySlider.minValue     = 1;
            _difficultySlider.maxValue     = 5;
            _difficultySlider.wholeNumbers = true;

            _difficultySlider.onValueChanged.AddListener(v =>
                _difficultyLabel.text = $"Level {(int)v}");

            _closeButton.onClick.AddListener(() =>
            {
                SaveSettings();
                gameObject.SetActive(false);
            });

            gameObject.SetActive(false);
        }

        public void Show()
        {
            _difficultySlider.value = SettingsManager.CpuDifficulty;
            _soundToggle.isOn       = SettingsManager.SoundOn;
            _vibrateToggle.isOn     = SettingsManager.VibrateOn;
            _difficultyLabel.text   = $"Level {SettingsManager.CpuDifficulty}";
            gameObject.SetActive(true);
        }

        private void SaveSettings()
        {
            SettingsManager.CpuDifficulty = (int)_difficultySlider.value;
            SettingsManager.SoundOn       = _soundToggle.isOn;
            SettingsManager.VibrateOn     = _vibrateToggle.isOn;
        }
    }
}
