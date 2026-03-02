using Cysharp.Threading.Tasks;
using SpeedGame.Core;
using SpeedGame.Domain;
using UnityEngine;

namespace SpeedGame.State
{
    public sealed class PlayerInputState : ISpeedGameState
    {
        private readonly SpeedGameController _controller;
        private readonly SpeedGameContext _context;

        public PlayerInputState(SpeedGameController controller, SpeedGameContext context)
        {
            _controller = controller;
            _context = context;
        }

        public async UniTask EnterAsync()
        {
            await _controller.PlayerAgent.EnterAsync();
            await _controller.OpponentAgent.EnterAsync();
        }

        public async UniTask ExitAsync()
        {
            await _controller.PlayerAgent.ExitAsync();
            await _controller.OpponentAgent.ExitAsync();
        }

        public async UniTask TickAsync()
        {
            await _controller.PlayerAgent.TickAsync(Time.deltaTime);
            await _controller.OpponentAgent.TickAsync(Time.deltaTime);

            if (_controller.HasQueuedCommand)
            {
                await _controller.ChangeStateAsync(_controller.ResolveCommandState);
                return;
            }

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
    }
}
