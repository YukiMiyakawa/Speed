using Cysharp.Threading.Tasks;

namespace SpeedGame.Core.Player
{
    public interface IPlayerAgentState
    {
        UniTask EnterAsync();
        UniTask ExitAsync();
        UniTask TickAsync(float deltaTime);
    }
}
