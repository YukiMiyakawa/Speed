using Cysharp.Threading.Tasks;

namespace SpeedGame.Core
{
    public interface ISpeedGameState
    {
        UniTask EnterAsync();
        UniTask ExitAsync();
        UniTask TickAsync();
    }
}
