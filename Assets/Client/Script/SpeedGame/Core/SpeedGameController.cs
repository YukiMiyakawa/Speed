using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using SpeedGame.Animation;
using SpeedGame.Config;
using SpeedGame.Domain;
using SpeedGame.State;
using UnityEngine;

namespace SpeedGame.Core
{
    public sealed class SpeedGameController : MonoBehaviour
    {
        [Header("Rule / Timing")]
        [SerializeField] private SpeedRuleSettings ruleSettings = new();
        [SerializeField] private SpeedTimingSettings timingSettings = new();
        [SerializeField] private CpuDifficultyProfile cpuDifficulty;

        [Header("Animation")]
        [SerializeField] private SpeedAnimationProfile animationProfile;

        private SpeedGameContext _context;
        private SpeedStateMachine _stateMachine;
        private readonly Queue<PlayerCommand> _queuedCommands = new();

        public SetupState SetupState { get; private set; }
        public PlayerInputState PlayerInputState { get; private set; }
        public ResolveCommandState ResolveCommandState { get; private set; }
        public GameOverState GameOverState { get; private set; }

        public ReadOnlyReactiveProperty<PlayerSide?> Winner => _context.Winner;
        public bool HasQueuedCommand => _queuedCommands.Count > 0;

        private async UniTaskVoid Start()
        {
            if (cpuDifficulty == null)
            {
                cpuDifficulty = ScriptableObject.CreateInstance<CpuDifficultyProfile>();
            }

            ICardAnimationService animation = animationProfile != null
                ? new DotweenCardAnimationService(animationProfile)
                : null;

            _context = new SpeedGameContext(ruleSettings, timingSettings, cpuDifficulty, animation, destroyCancellationToken);
            _stateMachine = new SpeedStateMachine();

            SetupState = new SetupState(this, _context);
            PlayerInputState = new PlayerInputState(this, _context);
            ResolveCommandState = new ResolveCommandState(this, _context);
            GameOverState = new GameOverState();

            await ChangeStateAsync(SetupState);

            while (!destroyCancellationToken.IsCancellationRequested)
            {
                await _stateMachine.TickAsync();
                await UniTask.Yield(PlayerLoopTiming.Update, destroyCancellationToken);
            }
        }

        public UniTask ChangeStateAsync(ISpeedGameState next)
        {
            return _stateMachine.ChangeStateAsync(next);
        }

        public void RequestPlayFromPlayer(int handIndex, PileLane lane)
        {
            if (_context == null)
            {
                return;
            }

            _context.CommandStream.OnNext(PlayerCommand.Play(PlayerSide.Player, handIndex, lane));
        }

        public void RequestDrawFromPlayerStock()
        {
            if (_context == null)
            {
                return;
            }

            _context.CommandStream.OnNext(PlayerCommand.Draw(PlayerSide.Player));
        }

        public void EnqueueCommand(PlayerCommand command)
        {
            _queuedCommands.Enqueue(command);
        }

        public bool TryDequeueCommand(out PlayerCommand command)
        {
            if (_queuedCommands.Count > 0)
            {
                command = _queuedCommands.Dequeue();
                return true;
            }

            command = default;
            return false;
        }

        public bool TryQueuePlayableOpponentCommand(int handIndex, PileLane lane)
        {
            if (_context == null)
            {
                return false;
            }

            var cmd = PlayerCommand.Play(PlayerSide.Opponent, handIndex, lane);
            if (!_context.Model.CanApplyCommand(cmd))
            {
                return false;
            }

            _queuedCommands.Enqueue(cmd);
            return true;
        }

        public UniTask AnimateMoveToLaneAsync(RectTransform card, RectTransform lane)
        {
            return _context.Animation == null
                ? UniTask.CompletedTask
                : _context.Animation.MoveToFoundationAsync(card, lane, destroyCancellationToken);
        }

        public UniTask AnimateDrawFromStockAsync(RectTransform card, RectTransform stock, RectTransform handSlot)
        {
            return _context.Animation == null
                ? UniTask.CompletedTask
                : _context.Animation.DrawFromStockAsync(card, stock, handSlot, destroyCancellationToken);
        }

        public UniTask AnimateBounceBackAsync(RectTransform card, Vector2 origin)
        {
            return _context.Animation == null
                ? UniTask.CompletedTask
                : _context.Animation.BounceBackAsync(card, origin, destroyCancellationToken);
        }

        public string GetStateSummaryForDebug()
        {
            if (_context == null)
            {
                return "not-initialized";
            }

            var left = _context.Model.LeftTop?.EffectiveRank.ToString() ?? "-";
            var right = _context.Model.RightTop?.EffectiveRank.ToString() ?? "-";
            return $"PHand:{_context.Model.PlayerHand.Count} OHand:{_context.Model.OpponentHand.Count} " +
                   $"PStock:{_context.Model.PlayerStockCount} OStock:{_context.Model.OpponentStockCount} " +
                   $"L:{left} R:{right}";
        }
    }
}
