using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Speed.Domain;

namespace Speed.View
{
    public class ResultDialogView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button          _rematchButton;
        [SerializeField] private Button          _titleButton;

        public event Action OnRematch;
        public event Action OnTitle;

        private void Awake()
        {
            _rematchButton.onClick.AddListener(() => OnRematch?.Invoke());
            _titleButton.onClick.AddListener(()   => OnTitle?.Invoke());
            gameObject.SetActive(false);
        }

        public void Show(BattleResult result)
        {
            string msg;
            switch (result.Type)
            {
                case BattleResultType.PlayerWin: msg = "YOU WIN!";   break;
                case BattleResultType.CpuWin:    msg = "CPU WINS";   break;
                default:                         msg = "DRAW";       break;
            }
            _resultText.text = msg;
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
