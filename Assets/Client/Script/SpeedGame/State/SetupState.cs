using Cysharp.Threading.Tasks;
using SpeedGame.Core;

namespace SpeedGame.State
{
    public sealed class SetupState : ISpeedGameState
    {
        private readonly SpeedGameController _controller;
        private readonly SpeedGameContext _context;

        public SetupState(SpeedGameController controller, SpeedGameContext context)
        {
            _controller = controller;
            _context = context;
        }

        public UniTask EnterAsync()
        {
            _context.Model.Setup();
            _context.StuckTimer = 0f;
            _controller.ResetRound();
            return _controller.ChangeStateAsync(_controller.PlayerInputState);
        }

        public UniTask ExitAsync() => UniTask.CompletedTask;
        public UniTask TickAsync() => UniTask.CompletedTask;
    }
}
