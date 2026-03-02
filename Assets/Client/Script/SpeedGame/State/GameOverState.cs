using Cysharp.Threading.Tasks;
using SpeedGame.Core;

namespace SpeedGame.State
{
    public sealed class GameOverState : ISpeedGameState
    {
        public UniTask EnterAsync() => UniTask.CompletedTask;
        public UniTask ExitAsync() => UniTask.CompletedTask;
        public UniTask TickAsync() => UniTask.CompletedTask;
    }
}
