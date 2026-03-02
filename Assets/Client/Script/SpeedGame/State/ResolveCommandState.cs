using Cysharp.Threading.Tasks;
using SpeedGame.Core;
using SpeedGame.Domain;

namespace SpeedGame.State
{
    public sealed class ResolveCommandState : ISpeedGameState
    {
        private readonly SpeedGameController _controller;
        private readonly SpeedGameContext _context;

        public ResolveCommandState(SpeedGameController controller, SpeedGameContext context)
        {
            _controller = controller;
            _context = context;
        }

        public async UniTask EnterAsync()
        {
            if (!_controller.TryDequeueCommand(out var command))
            {
                await _controller.ChangeStateAsync(_controller.PlayerInputState);
                return;
            }

            // オンライン移行時はここをローカル確定から「サーバー確定結果の適用」に置換する。
            if (_context.Model.TryApplyCommand(command, out _))
            {
                if (_context.Model.IsWin(command.Side))
                {
                    _context.Winner.Value = command.Side;
                    await _controller.ChangeStateAsync(_controller.GameOverState);
                    return;
                }
            }

            await _controller.ChangeStateAsync(_controller.PlayerInputState);
        }

        public UniTask ExitAsync() => UniTask.CompletedTask;
        public UniTask TickAsync() => UniTask.CompletedTask;
    }
}
