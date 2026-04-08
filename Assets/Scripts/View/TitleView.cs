using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace Speed.View
{
    public class TitleView : MonoBehaviour
    {
        [SerializeField] private Button       _cpuBattleButton;
        [SerializeField] private Button       _onlineButton;
        [SerializeField] private Button       _settingsButton;
        [SerializeField] private SettingsView _settingsView;

        private void Awake()
        {
            DOTween.Init();
        }

        private void Start()
        {
            _cpuBattleButton.onClick.AddListener(() =>
                SceneManager.LoadScene("BattleScene"));

            _onlineButton.interactable = false;
            _onlineButton.onClick.AddListener(() =>
                Debug.Log("[Speed] Online not implemented"));

            _settingsButton.onClick.AddListener(() => _settingsView.Show());
        }
    }
}
