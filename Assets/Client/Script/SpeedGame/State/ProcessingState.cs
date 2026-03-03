using Cysharp.Threading.Tasks;
using SpeedGame.Core;
using SpeedGame.Domain;
using UnityEngine;

namespace SpeedGame.State
{
    /// <summary>
    /// ゲーム実行中ステート。キュー処理と場詰み監視を担当する。
    /// </summary>
    public sealed class ProcessingState : ISpeedGameState
    {
        private readonly SpeedGameController _controller;
        private readonly SpeedGameContext _context;

        public ProcessingState(SpeedGameController controller, SpeedGameContext context)
        {
            _controller = controller;
            _context = context;
        }

        public UniTask EnterAsync() => UniTask.CompletedTask;
        public UniTask ExitAsync() => UniTask.CompletedTask;

        public async UniTask TickAsync()
        {
            if (_controller.TryDequeueCommand(out var command))
            {
                if (_controller.Resolver.TryResolve(command, out var winner) && winner.HasValue)
                {
                    _context.Winner.Value = winner.Value;
                    await _controller.ChangeStateAsync(_controller.GameOverState);
                    return;
                }
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
