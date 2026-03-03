using Cysharp.Threading.Tasks;
using SpeedGame.Core;

namespace SpeedGame.State
{
    /// <summary>
    /// 一時停止ステート（将来実装用スタブ）。
    /// </summary>
    public sealed class PauseState : ISpeedGameState
    {
        public UniTask EnterAsync() => UniTask.CompletedTask;
        public UniTask ExitAsync() => UniTask.CompletedTask;
        public UniTask TickAsync() => UniTask.CompletedTask;
    }
}
