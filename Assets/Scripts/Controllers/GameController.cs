using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Speed.Domain;
using Speed.Application;
using Speed.Config;

namespace Speed.Controllers
{
    public enum BattlePhase { Idle, Dealing, Playing, StalemateFlipping, GameOver }

    public class GameController : MonoBehaviour
    {
        [Header("Config")]
        public CpuDifficultyConfig DifficultyConfig;

        [Header("Stalemate")]
        [Tooltip("Seconds without playable move before stalemate triggers")]
        public float StalemateDelay = 1.2f;

        // --- State ---
        public GameState   State { get; private set; }
        public BattlePhase Phase { get; private set; } = BattlePhase.Idle;

        // Pile animation lock flags
        public bool LeftPileAnimating  { get; private set; }
        public bool RightPileAnimating { get; private set; }

        // --- Events ---
        public event Action<GameState>              OnDealComplete;
        public event Action<PlayerId, int, PileId>  OnCardPlayed;        // player, originalHandIndex, pile
        public event Action<PlayerId, int, PileId>  OnCardPlayFailed;    // player, handIndex, pile
        public event Action<PlayerId, int, PileId>  OnFalsePlayAttempt;  // CPU false-miss
        public event Action                         OnFlipStart;
        public event Action                         OnFlipComplete;
        public event Action<BattleResult>           OnGameOver;

        // --- Internal ---
        private float _stalemateTimer;
        private BattleInputController _inputController;
        private CpuController         _cpuController;

        private void Awake()
        {
            DOTween.Init();
            _inputController = GetComponent<BattleInputController>();
            _cpuController   = GetComponent<CpuController>();
        }

        private void Start()
        {
            StartBattle();
        }

        public void StartBattle()
        {
            State          = new GameState();
            Phase          = BattlePhase.Dealing;
            _stalemateTimer = 0f;
            LeftPileAnimating  = false;
            RightPileAnimating = false;
            DealService.Deal(State);
            OnDealComplete?.Invoke(State);
            StartCoroutine(BeginPlayAfterDelay(0.4f));
        }

        private IEnumerator BeginPlayAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Phase = BattlePhase.Playing;
            _inputController?.SetActive(true);
            _cpuController?.StartThinking();
        }

        // ---------------------------------------------------------------
        //  Pile animation
        // ---------------------------------------------------------------
        public void NotifyPileAnimationComplete(PileId pileId)
        {
            if (pileId == PileId.Left)  LeftPileAnimating  = false;
            else                        RightPileAnimating = false;
        }

        public bool CanAcceptInput(PileId pileId)
        {
            if (Phase != BattlePhase.Playing) return false;
            if (pileId == PileId.Left  && LeftPileAnimating)  return false;
            if (pileId == PileId.Right && RightPileAnimating) return false;
            return true;
        }

        // ---------------------------------------------------------------
        //  Card play
        // ---------------------------------------------------------------
        public PutCardResult TryPlayerPutCard(int handIndex, PileId pileId)
        {
            if (!CanAcceptInput(pileId)) return PutCardResult.PileBlocked();

            var hand = State.PlayerHand;
            if (handIndex < 0 || handIndex >= hand.Count)
                return PutCardResult.InvalidRule();

            var card    = hand[handIndex];
            var pileTop = pileId == PileId.Left ? State.LeftPileTop : State.RightPileTop;
            if (!RuleService.CanPlace(card, pileTop))
            {
                OnCardPlayFailed?.Invoke(PlayerId.Player, handIndex, pileId);
                return PutCardResult.InvalidRule();
            }

            LockPile(pileId);
            _stalemateTimer = 0f;
            BattleCommandProcessor.TryPutCard(State, PlayerId.Player, handIndex, pileId);
            OnCardPlayed?.Invoke(PlayerId.Player, handIndex, pileId);
            CheckWin();
            return PutCardResult.Success();
        }

        public PutCardResult TryCpuPutCard(int handIndex, PileId pileId)
        {
            if (!CanAcceptInput(pileId)) return PutCardResult.PileBlocked();

            var hand = State.CpuHand;
            if (handIndex < 0 || handIndex >= hand.Count)
                return PutCardResult.InvalidRule();

            var card    = hand[handIndex];
            var pileTop = pileId == PileId.Left ? State.LeftPileTop : State.RightPileTop;
            if (!RuleService.CanPlace(card, pileTop))
            {
                OnCardPlayFailed?.Invoke(PlayerId.Cpu, handIndex, pileId);
                return PutCardResult.InvalidRule();
            }

            LockPile(pileId);
            _stalemateTimer = 0f;
            BattleCommandProcessor.TryPutCard(State, PlayerId.Cpu, handIndex, pileId);
            OnCardPlayed?.Invoke(PlayerId.Cpu, handIndex, pileId);
            CheckWin();
            return PutCardResult.Success();
        }

        public void NotifyCpuFalsePlay(int handIndex, PileId pileId)
        {
            OnFalsePlayAttempt?.Invoke(PlayerId.Cpu, handIndex, pileId);
        }

        private void LockPile(PileId pileId)
        {
            if (pileId == PileId.Left)  LeftPileAnimating  = true;
            else                        RightPileAnimating = true;
        }

        // ---------------------------------------------------------------
        //  Input helpers
        // ---------------------------------------------------------------
        public void NotifyPlayerDragging() => _stalemateTimer = 0f;
        public void ResetStalemateTimer()  => _stalemateTimer = 0f;

        // ---------------------------------------------------------------
        //  Stalemate loop
        // ---------------------------------------------------------------
        private void Update()
        {
            if (Phase != BattlePhase.Playing) return;
            if (LeftPileAnimating || RightPileAnimating) return;

            if (StalemateService.IsStalemate(State))
            {
                _stalemateTimer += Time.deltaTime;
                if (_stalemateTimer >= StalemateDelay)
                {
                    _stalemateTimer = 0f;
                    TriggerStalemate();
                }
            }
            else
            {
                _stalemateTimer = 0f;
            }
        }

        private void TriggerStalemate()
        {
            bool hasDecks = State.PlayerDeck.Count > 0 || State.CpuDeck.Count > 0;
            if (!hasDecks)
            {
                FinishGame(WinJudgeService.CheckStalemateResult(State));
                return;
            }

            Phase = BattlePhase.StalemateFlipping;
            _inputController?.SetActive(false);
            _cpuController?.StopThinking();
            OnFlipStart?.Invoke();
        }

        /// <summary>Called by BattleView after it executes the flip and animation completes.</summary>
        public void ExecuteFlip()
        {
            BattleCommandProcessor.FlipCenterPiles(State);
        }

        public void NotifyFlipAnimationComplete()
        {
            Phase = BattlePhase.Playing;
            OnFlipComplete?.Invoke();
            _inputController?.SetActive(true);
            _cpuController?.StartThinking();
        }

        // ---------------------------------------------------------------
        //  Win check
        // ---------------------------------------------------------------
        private void CheckWin()
        {
            var result = WinJudgeService.CheckHandEmpty(State);
            if (result != null) StartCoroutine(EndGameDelayed(result, 0.6f));
        }

        private IEnumerator EndGameDelayed(BattleResult result, float delay)
        {
            yield return new WaitForSeconds(delay);
            FinishGame(result);
        }

        private void FinishGame(BattleResult result)
        {
            if (Phase == BattlePhase.GameOver) return;
            Phase = BattlePhase.GameOver;
            _inputController?.SetActive(false);
            _cpuController?.StopThinking();
            OnGameOver?.Invoke(result);
        }

        // ---------------------------------------------------------------
        //  CPU settings accessor
        // ---------------------------------------------------------------
        public CpuDifficultySettings GetCpuSettings()
        {
            if (DifficultyConfig == null)
                DifficultyConfig = Resources.Load<CpuDifficultyConfig>("CpuDifficultyConfig");

            int level = SettingsManager.CpuDifficulty;
            return DifficultyConfig != null
                ? DifficultyConfig.GetSettings(level)
                : new CpuDifficultySettings(380f, 0.07f, 0.5f);
        }
    }
}
