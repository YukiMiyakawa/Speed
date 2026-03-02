using System.Collections.Generic;
using SpeedGame.Domain;

namespace SpeedGame.Core.Player
{
    public sealed class LocalPlayerCommandSource : IPlayerCommandSource
    {
        private readonly Queue<PlayerCommand> _buffer = new();

        public void Enqueue(PlayerCommand command)
        {
            _buffer.Enqueue(command);
        }

        public void Reset()
        {
            _buffer.Clear();
        }

        public bool TryGetNextCommand(SpeedGameModel model, PlayerSide side, float deltaTime, out PlayerCommand command)
        {
            while (_buffer.Count > 0)
            {
                var next = _buffer.Dequeue();
                if (next.Side != side)
                {
                    continue;
                }

                command = next;
                return true;
            }

            command = default;
            return false;
        }
    }
}
