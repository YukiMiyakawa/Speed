using System;
using Cysharp.Threading.Tasks;
using R3;
using SpeedGame.Core;
using SpeedGame.Domain;
using UnityEngine;

namespace SpeedGame.State
{
    public sealed class PlayerInputState : ISpeedGameState
    {
        private readonly SpeedGameController _controller;
        private readonly SpeedGameContext _context;
        private IDisposable _subscription;
        private float _cpuCountdown;
        private readonly System.Random _random = new();

        public PlayerInputState(SpeedGameController controller, SpeedGameContext context)
        {
            _controller = controller;
            _context = context;
        }

        public UniTask EnterAsync()
        {
            _subscription = _context.CommandStream.Subscribe(command => _controller.EnqueueCommand(command));
            _cpuCountdown = NextCpuDelaySeconds();
            return UniTask.CompletedTask;
        }

        public UniTask ExitAsync()
        {
            _subscription?.Dispose();
            _subscription = null;
            return UniTask.CompletedTask;
        }

        public async UniTask TickAsync()
        {
            if (_controller.HasQueuedCommand)
            {
                await _controller.ChangeStateAsync(_controller.ResolveCommandState);
                return;
            }

            TickCpu();

            var playerCanMove = _context.Model.CanAnyMove(PlayerSide.Player);
            var opponentCanMove = _context.Model.CanAnyMove(PlayerSide.Opponent);

            if (playerCanMove || opponentCanMove)
            {
                _context.StuckTimer = 0f;
                return;
            }

            _context.StuckTimer += Time.deltaTime;
            if (_context.StuckTimer >= _context.Timing.StuckResetSeconds)
            {
                _context.Model.ResetStuckLanes();
                _context.StuckTimer = 0f;
            }
        }

        private void TickCpu()
        {
            _cpuCountdown -= Time.deltaTime;
            if (_cpuCountdown > 0f)
            {
                return;
            }

            _cpuCountdown = NextCpuDelaySeconds();

            if (_random.NextDouble() < _context.CpuDifficulty.MistakeRate)
            {
                return;
            }

            var hand = _context.Model.OpponentHand;
            for (var i = 0; i < hand.Count; i++)
            {
                if (_controller.TryQueuePlayableOpponentCommand(i, PileLane.Left) || _controller.TryQueuePlayableOpponentCommand(i, PileLane.Right))
                {
                    return;
                }
            }
        }

        private float NextCpuDelaySeconds()
        {
            var jitter = (float)(_random.NextDouble() * 2.0 - 1.0) * _context.CpuDifficulty.ReactionJitterMs;
            var ms = Mathf.Max(50f, _context.CpuDifficulty.ReactionMeanMs + jitter);
            return ms / 1000f;
        }
    }
}
