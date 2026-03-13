using Speed.Controllers;
using Speed.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Speed.Presentation
{
    public sealed class BattleUIController : MonoBehaviour
    {
        [SerializeField] private TablePileView leftPileView;
        [SerializeField] private TablePileView rightPileView;
        [SerializeField] private RectTransform playerHandRoot;
        [SerializeField] private CardView cardViewPrefab;
        [SerializeField] private Text playerDeckCountText;
        [SerializeField] private Text cpuDeckCountText;
        [SerializeField] private Text playerHandCountText;
        [SerializeField] private Text cpuHandCountText;
        [SerializeField] private Text waitingText;
        [SerializeField] private Text invalidMoveText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultText;
        [SerializeField] private Button rematchButton;

        private GameController gameController;

        public void Bind(GameController controller)
        {
            gameController = controller;
            rematchButton.onClick.RemoveAllListeners();
            rematchButton.onClick.AddListener(gameController.RestartMatch);
        }

        public void RefreshAll()
        {
            RefreshCounters();
            RefreshPiles();
            RebuildPlayerHand();
        }

        public void RefreshCounters()
        {
            var state = gameController.State;
            playerDeckCountText.text = $"Player Deck: {state.Player.Deck.Count}";
            cpuDeckCountText.text = $"CPU Deck: {state.Cpu.Deck.Count}";
            playerHandCountText.text = $"Player Hand: {state.Player.Hand.Count}";
            cpuHandCountText.text = $"CPU Hand: {state.Cpu.Hand.Count}";
        }

        public void RefreshPiles()
        {
            leftPileView.SetCard(gameController.State.LeftPile.TopCard);
            rightPileView.SetCard(gameController.State.RightPile.TopCard);
            leftPileView.SetBusy(gameController.State.LeftPile.IsPlayingPutCardAnimation);
            rightPileView.SetBusy(gameController.State.RightPile.IsPlayingPutCardAnimation);
        }

        public void RebuildPlayerHand()
        {
            for (var i = playerHandRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(playerHandRoot.GetChild(i).gameObject);
            }

            foreach (var card in gameController.State.Player.Hand.Cards)
            {
                var view = Instantiate(cardViewPrefab, playerHandRoot);
                view.gameObject.SetActive(true);
                view.Bind(card, gameController.InputController);
            }
        }

        public void ShowWaiting(bool visible)
        {
            waitingText.gameObject.SetActive(visible);
            waitingText.text = visible ? "Waiting..." : string.Empty;
        }

        public void ShowInvalidMove(bool visible)
        {
            invalidMoveText.gameObject.SetActive(visible);
            invalidMoveText.text = visible ? "Invalid Move" : string.Empty;
        }

        public void ShowResult(BattleResult result)
        {
            var visible = result != BattleResult.None;
            resultPanel.SetActive(visible);
            if (!visible)
            {
                resultText.text = string.Empty;
                return;
            }

            resultText.text = result switch
            {
                BattleResult.PlayerWin => "WIN",
                BattleResult.CpuWin => "LOSE",
                _ => "DRAW"
            };
        }

        public TablePileView GetPileView(PileId pileId)
        {
            return pileId == PileId.Left ? leftPileView : rightPileView;
        }
    }
}
