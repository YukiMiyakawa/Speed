using Cysharp.Threading.Tasks;
using SpeedGame.Core;

namespace SpeedGame.State
{
    public sealed class GameOverState : ISpeedGameState
    {
        private readonly SpeedGameController _controller;

        public GameOverState(SpeedGameController controller)
        {
            _controller = controller;
        }

        public UniTask EnterAsync() => _controller.StopAgentLoopsAsync();
        public UniTask ExitAsync() => UniTask.CompletedTask;
        public UniTask TickAsync() => UniTask.CompletedTask;
    }
}
