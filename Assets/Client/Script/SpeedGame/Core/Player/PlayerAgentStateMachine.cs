using Cysharp.Threading.Tasks;

namespace SpeedGame.Core.Player
{
    public sealed class PlayerAgentStateMachine
    {
        private IPlayerAgentState _current;

        public async UniTask ChangeStateAsync(IPlayerAgentState next)
        {
            if (_current != null)
            {
                await _current.ExitAsync();
            }

            _current = next;

            if (_current != null)
            {
                await _current.EnterAsync();
            }
        }

        public UniTask TickAsync(float deltaTime)
        {
            return _current == null ? UniTask.CompletedTask : _current.TickAsync(deltaTime);
        }
    }
}
