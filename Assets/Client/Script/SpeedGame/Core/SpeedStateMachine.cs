using Cysharp.Threading.Tasks;

namespace SpeedGame.Core
{
    public sealed class SpeedStateMachine
    {
        private ISpeedGameState _current;

        public async UniTask ChangeStateAsync(ISpeedGameState next)
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

        public UniTask TickAsync()
        {
            return _current == null ? UniTask.CompletedTask : _current.TickAsync();
        }
    }
}
