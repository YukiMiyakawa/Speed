using System.Collections.Generic;
using SpeedGame.Domain;

namespace SpeedGame.Core.Player
{
    // ネットワーク受信キュー前提の実装。Photon導入時は受信イベントから PushFromNetwork を呼ぶ。
    public sealed class RemoteQueueCommandSource : IPlayerCommandSource
    {
        private readonly Queue<PlayerCommand> _received = new();

        public void PushFromNetwork(PlayerCommand command)
        {
            _received.Enqueue(command);
        }

        public void Reset()
        {
            _received.Clear();
        }

        public bool TryGetNextCommand(SpeedGameModel model, PlayerSide side, float deltaTime, out PlayerCommand command)
        {
            while (_received.Count > 0)
            {
                var next = _received.Dequeue();
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
