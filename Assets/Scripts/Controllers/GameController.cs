using System.Collections;
using Speed.Application;
using Speed.Config;
using Speed.Domain;
using Speed.Presentation;
using UnityEngine;

namespace Speed.Controllers
{
    public sealed class GameController : MonoBehaviour
    {
        private const float StalemateSeconds = 1.2f;
        private const float IdleCheckInterval = 0.1f;

        [SerializeField] private CpuDifficultyConfig difficultyConfig;
        [SerializeField] private int selectedDifficultyLevel = 3;
        [SerializeField] private BattleUIController battleUIController;
        [SerializeField] private BattleAnimationController animationController;
        [SerializeField] private BattleInputController inputController;
        [SerializeField] private CpuController cpuController;

        private readonly DealService dealService = new DealService();
        private readonly RuleService ruleService = new RuleService();
        private WinJudgeService winJudgeService;
        private StalemateService stalemateService;
        private BattleCommandProcessor commandProcessor;
        private CpuDecisionService cpuDecisionService;
        private System.Random random;
        private float stalemateTimer;
        private float idleCheckTimer;

        public GameState State { get; private set; }
        public BattleAnimationController AnimationController => animationController;
        public BattleInputController InputController => inputController;
        public CpuDifficultySettings CpuDifficultySettings => difficultyConfig != null
            ? difficultyConfig.GetSettings(selectedDifficultyLevel)
            : CpuDifficultyConfig.CreateDefaultSettings(selectedDifficultyLevel);

        private void Awake()
        {
            winJudgeService = new WinJudgeService();
            stalemateService = new StalemateService(ruleService);
            commandProcessor = new BattleCommandProcessor(ruleService);
            cpuDecisionService = new CpuDecisionService(ruleService);
            random = new System.Random();
        }

        private void Start()
        {
            if (battleUIController == null || animationController == null || inputController == null || cpuController == null)
            {
                Debug.LogError("GameController dependencies are not set.");
                enabled = false;
                return;
            }

            StartMatch();
        }

        private void Update()
        {
            if (State == null || State.IsGameOver)
            {
                return;
            }

            cpuController.Tick(Time.deltaTime);
            UpdateStalemate(Time.deltaTime);
        }

        public void StartMatch()
        {
            State = dealService.CreateInitialState(random.Next());
            stalemateTimer = 0f;
            idleCheckTimer = 0f;

            inputController.Initialize(this);
            cpuController.Initialize(this, cpuDecisionService, random);
            battleUIController.Bind(this);
            battleUIController.RefreshAll();
            battleUIController.ShowWaiting(false);
            battleUIController.ShowInvalidMove(false);
            battleUIController.ShowResult(BattleResult.None);
        }

        public PutCardResult TryPlayerPut(int cardId, PileId pileId, CardView cardView)
        {
            if (State == null || State.IsGameOver || State.IsWaitingForPileRefresh)
            {
                return PutCardResult.InvalidRule;
            }

            var result = commandProcessor.TryPutCard(State.Player, State.GetPile(pileId), cardId, out var playedCard);
            HandleCommandResult(result, pileId, playedCard, cardView);
            return result;
        }

        public PutCardResult TryCpuPut(CpuDecision decision)
        {
            if (!decision.ShouldPlay || State == null || State.IsGameOver || State.IsWaitingForPileRefresh)
            {
                return PutCardResult.InvalidRule;
            }

            var result = commandProcessor.TryPutCard(State.Cpu, State.GetPile(decision.PileId), decision.Card.Id, out var playedCard);
            HandleCommandResult(result, decision.PileId, playedCard, null);
            return result;
        }

        public bool CanAcceptInput()
        {
            return State != null && !State.IsGameOver && !State.IsWaitingForPileRefresh;
        }

        public TablePileView GetPileView(PileId pileId)
        {
            return battleUIController.GetPileView(pileId);
        }

        public void OnPileAnimationCompleted(PileId pileId)
        {
            State.GetPile(pileId).IsPlayingPutCardAnimation = false;
            battleUIController.RefreshAll();
            EvaluateWin();
        }

        public void RestartMatch()
        {
            StartMatch();
        }

        private void HandleCommandResult(PutCardResult result, PileId pileId, Card playedCard, CardView cardView)
        {
            battleUIController.ShowInvalidMove(result == PutCardResult.InvalidRule);

            if (result != PutCardResult.Success)
            {
                if (cardView != null)
                {
                    if (result == PutCardResult.BlockedByAnimation)
                    {
                        animationController.PlayCancelAnimation(cardView);
                    }
                    else
                    {
                        animationController.PlayInvalidReturn(cardView);
                    }
                }

                return;
            }

            stalemateTimer = 0f;
            idleCheckTimer = 0f;
            var pileView = battleUIController.GetPileView(pileId);
            animationController.PlayPutAnimation(cardView, pileView, playedCard, () => OnPileAnimationCompleted(pileId));
            battleUIController.RefreshCounters();
        }

        private void UpdateStalemate(float deltaTime)
        {
            if (State.IsWaitingForPileRefresh)
            {
                return;
            }

            idleCheckTimer += deltaTime;
            if (idleCheckTimer < IdleCheckInterval)
            {
                return;
            }

            idleCheckTimer = 0f;
            var playerCanPlay = stalemateService.HasPlayableCard(State.Player, State);
            var cpuCanPlay = stalemateService.HasPlayableCard(State.Cpu, State);

            if (playerCanPlay || cpuCanPlay)
            {
                stalemateTimer = 0f;
                return;
            }

            stalemateTimer += IdleCheckInterval;
            if (stalemateTimer >= StalemateSeconds)
            {
                StartCoroutine(ResolveStalemate());
            }
        }

        private IEnumerator ResolveStalemate()
        {
            if (State.IsWaitingForPileRefresh)
            {
                yield break;
            }

            stalemateTimer = 0f;
            State.IsWaitingForPileRefresh = true;
            battleUIController.ShowWaiting(true);

            if (!State.Player.Deck.HasCards && !State.Cpu.Deck.HasCards)
            {
                FinishGame(winJudgeService.EvaluateDeckEmptyResolution(State));
                yield break;
            }

            var leftCard = DrawRefreshCard(preferPlayerDeck: true);
            var rightCard = DrawRefreshCard(preferPlayerDeck: false);
            State.LeftPile.IsPlayingPutCardAnimation = true;
            State.RightPile.IsPlayingPutCardAnimation = true;

            yield return animationController.PlayPileRefreshAnimation(
                battleUIController.GetPileView(PileId.Left),
                battleUIController.GetPileView(PileId.Right),
                leftCard,
                rightCard);

            if (leftCard != null)
            {
                State.LeftPile.SetTopCard(leftCard);
            }

            if (rightCard != null)
            {
                State.RightPile.SetTopCard(rightCard);
            }

            State.LeftPile.IsPlayingPutCardAnimation = false;
            State.RightPile.IsPlayingPutCardAnimation = false;
            State.IsWaitingForPileRefresh = false;
            battleUIController.ShowWaiting(false);
            battleUIController.RefreshAll();
            EvaluateWin();
        }

        private Card DrawRefreshCard(bool preferPlayerDeck)
        {
            if (preferPlayerDeck)
            {
                return State.Player.Deck.Draw() ?? State.Cpu.Deck.Draw();
            }

            return State.Cpu.Deck.Draw() ?? State.Player.Deck.Draw();
        }

        private void EvaluateWin()
        {
            var result = winJudgeService.EvaluateImmediate(State);
            if (result != BattleResult.None)
            {
                FinishGame(result);
            }
        }

        private void FinishGame(BattleResult result)
        {
            State.IsWaitingForPileRefresh = false;
            State.Finish(result);
            battleUIController.ShowWaiting(false);
            battleUIController.RefreshAll();
            battleUIController.ShowResult(result);
        }
    }
}
