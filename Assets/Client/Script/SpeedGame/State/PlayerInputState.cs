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
            await UniTask.CompletedTask;
        }

        public async UniTask ExitAsync()
        {
            await UniTask.CompletedTask;
        }

        public async UniTask TickAsync()
        {
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
